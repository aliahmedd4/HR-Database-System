using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using Milstone3_WebApp.Services;
using Milstone3_WebApp.Models;
using System.Security.Claims;

namespace Milstone3_WebApp.Controllers
{
    [Authorize] // Require authentication for all actions
    public class LeavesController : Controller
    {
        private readonly HrPayrollSystemContext _context;
        private readonly NotificationService _notificationService;

        public LeavesController(HrPayrollSystemContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // -------------------- LIST LEAVE TYPES --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager, Employee")]
        public async Task<IActionResult> Index()
        {
            var leaves = await _context.Leaves
                .FromSqlRaw("SELECT * FROM Leave")
                .ToListAsync();
            return View(leaves);
        }

        // -------------------- LEAVE DETAILS --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager, Employee")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var leave = await _context.Leaves
                .FirstOrDefaultAsync(m => m.LeaveId == id);

            if (leave == null) return NotFound();
            return View(leave);
        }

        // -------------------- SUBMIT LEAVE REQUEST --------------------
        [Authorize(Roles = "Employee, HRAdmin, SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Get current user's employee ID
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "Employee ID not found. Please contact HR.";
                return RedirectToAction(nameof(Index));
            }

            // Load leave types for dropdown
            var leaveTypes = await _context.Leaves.ToListAsync();
            ViewBag.LeaveTypes = leaveTypes;
            ViewBag.EmployeeId = employeeId;

            return View();
        }

        [Authorize(Roles = "Employee, HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequestDto dto, string LeaveType)
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "Employee ID not found. Please contact HR.";
                return RedirectToAction(nameof(Index));
            }

            // If LeaveType is provided as text, find the LeaveId
            if (!string.IsNullOrEmpty(LeaveType) && dto.LeaveId == 0)
            {
                var trimmedLeaveType = LeaveType.Trim();
                
                // First, try to find existing leave type
                var leaveType = await _context.Leaves
                    .Where(l => l.IsActive == true)
                    .FirstOrDefaultAsync(l => l.LeaveType.ToLower() == trimmedLeaveType.ToLower() || 
                                             l.LeaveDescription.ToLower().Contains(trimmedLeaveType.ToLower()));
                
                // If not found, try to create it if it's a valid type
                if (leaveType == null)
                {
                    var validTypes = new[] { "Sick", "Vacation", "Probation", "Holiday", "Medical", "Special" };
                    var matchedType = validTypes.FirstOrDefault(t => t.ToLower() == trimmedLeaveType.ToLower());
                    
                    if (matchedType != null)
                    {
                        // Create the leave type if it doesn't exist
                        var newLeave = new Leave
                        {
                            LeaveType = matchedType,
                            LeaveDescription = $"{matchedType} Leave",
                            Description = $"Leave type for {matchedType}",
                            IsPaid = true,
                            MaxDaysPerYear = matchedType == "Sick" ? 15 : matchedType == "Vacation" ? 21 : 10,
                            RequiresApproval = true,
                            IsActive = true
                        };
                        
                        _context.Leaves.Add(newLeave);
                        await _context.SaveChangesAsync();
                        
                        leaveType = newLeave;
                    }
                }
                
                if (leaveType != null)
                {
                    dto.LeaveId = leaveType.LeaveId;
                }
                else
                {
                    ModelState.AddModelError("LeaveType", $"Leave type '{LeaveType}' not found. Please use: Sick, Vacation, Probation, Holiday, Medical, or Special.");
                    ViewBag.EmployeeId = employeeId;
                    return View(dto);
                }
            }

            // Validation: End date must be after start date
            if (dto.EndDate < dto.StartDate)
            {
                ModelState.AddModelError("", "End date must be after start date.");
                ViewBag.EmployeeId = employeeId;
                return View(dto);
            }

            // Validation: LeaveId must be set
            if (dto.LeaveId == 0)
            {
                ModelState.AddModelError("LeaveType", "Please enter a valid leave type.");
                ViewBag.EmployeeId = employeeId;
                return View(dto);
            }

            try
            {
                // Calculate total days
                var totalDays = (dto.EndDate - dto.StartDate).TotalDays + 1;
                
                // Handle file upload if provided
                string? attachmentPath = null;
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "leave-documents");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var fileName = $"{employeeId}_{DateTime.Now:yyyyMMddHHmmss}_{file.FileName}";
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        attachmentPath = $"/uploads/leave-documents/{fileName}";
                    }
                }

                // Verify the leave type exists
                var leaveType = await _context.Leaves.FindAsync(dto.LeaveId);
                if (leaveType == null)
                {
                    ModelState.AddModelError("LeaveType", "Selected leave type not found.");
                    ViewBag.EmployeeId = employeeId;
                    return View(dto);
                }

                // Verify employee exists
                var employee = await _context.Employees.FindAsync(employeeId.Value);
                if (employee == null)
                {
                    ModelState.AddModelError("", "Employee not found.");
                    ViewBag.EmployeeId = employeeId;
                    return View(dto);
                }

                // Use raw SQL to insert leave request to avoid navigation property issues
                var reason = dto.Reason ?? dto.Justification ?? string.Empty;
                var justification = dto.Justification ?? dto.Reason ?? string.Empty;
                
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO LeaveRequest 
                      (employee_id, leave_id, start_date, end_date, total_days, reason, justification, status, submitted_at, is_irregular)
                      VALUES 
                      ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})",
                    employeeId.Value,
                    dto.LeaveId,
                    DateOnly.FromDateTime(dto.StartDate),
                    DateOnly.FromDateTime(dto.EndDate),
                    (decimal)totalDays,
                    reason,
                    justification,
                    "Pending",
                    DateTime.Now,
                    totalDays > 5
                );

                // Get the newly created request ID
                var newRequestId = await _context.LeaveRequests
                    .Where(lr => lr.EmployeeId == employeeId.Value && 
                                lr.LeaveId == dto.LeaveId &&
                                lr.StartDate == DateOnly.FromDateTime(dto.StartDate) &&
                                lr.Status == "Pending")
                    .OrderByDescending(lr => lr.SubmittedAt)
                    .Select(lr => lr.RequestId)
                    .FirstOrDefaultAsync();

                // If attachment was uploaded, create LeaveDocument record
                if (!string.IsNullOrEmpty(attachmentPath) && newRequestId > 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO LeaveDocument 
                          (leave_request_id, document_type, file_path, uploaded_at)
                          VALUES 
                          ({0}, {1}, {2}, {3})",
                        newRequestId,
                        "Attachment",
                        attachmentPath,
                        DateOnly.FromDateTime(DateTime.Now)
                    );
                }

                TempData["SuccessMessage"] = "Leave request submitted successfully.";
                return RedirectToAction(nameof(MyLeaveHistory));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error submitting leave request: {ex.Message}");
                ViewBag.EmployeeId = employeeId;
                return View(dto);
            }
        }

        // -------------------- EDIT LEAVE TYPE (HR Admin) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditLeaveType(int? id)
        {
            if (id == null) return NotFound();

            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return NotFound();

            return View(leave);
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeaveType(int id, Leave leave)
        {
            if (id != leave.LeaveId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Leaves.Update(leave);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Leave type updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating leave type: {ex.Message}";
                }
            }
            return View(leave);
        }

        // -------------------- EDIT LEAVE REQUEST (Manager/HR) --------------------
        [Authorize(Roles = "HRAdmin, LineManager, SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditLeaveRequest(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null) return NotFound();

            return View(request);
        }

        [Authorize(Roles = "HRAdmin, LineManager, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeaveRequest(int id, int approverId, string status)
        {
            try
            {
                // Get leave request details before updating
                var leaveRequest = await _context.LeaveRequests
                    .Include(lr => lr.Leave)
                    .FirstOrDefaultAsync(lr => lr.RequestId == id);

                if (leaveRequest == null)
                {
                    TempData["ErrorMessage"] = "Leave request not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get previous status to handle entitlement reversal if needed
                var previousStatus = leaveRequest.Status;

                // Update leave request directly
                leaveRequest.Status = status;
                leaveRequest.ApprovedBy = approverId;
                leaveRequest.ApprovedAt = DateTime.Now;
                if (status == "Rejected" && string.IsNullOrEmpty(leaveRequest.RejectionReason))
                {
                    leaveRequest.RejectionReason = "Rejected by manager";
                }
                await _context.SaveChangesAsync();

                // Update LeaveEntitlement if status changed to/from Approved
                try
                {
                    var currentYear = DateTime.Now.Year;
                    var entitlement = await _context.LeaveEntitlements
                        .FirstOrDefaultAsync(le => le.EmployeeId == leaveRequest.EmployeeId && 
                                                   le.LeaveId == leaveRequest.LeaveId && 
                                                   le.Year == currentYear);
                    
                    if (entitlement != null)
                    {
                        // If previously approved and now rejected/cancelled, reverse the deduction
                        if ((previousStatus?.ToLower() == "approved") && (status.ToLower() != "approved"))
                        {
                            entitlement.UsedDays = Math.Max(0, (entitlement.UsedDays ?? 0) - leaveRequest.TotalDays);
                            entitlement.BalanceDays = (entitlement.AllocatedDays ?? 0) + (entitlement.CarryForwardDays ?? 0) - (entitlement.UsedDays ?? 0);
                        }
                        // If newly approved, deduct the days
                        else if ((previousStatus?.ToLower() != "approved") && (status.ToLower() == "approved"))
                        {
                            entitlement.UsedDays = (entitlement.UsedDays ?? 0) + leaveRequest.TotalDays;
                            entitlement.BalanceDays = (entitlement.AllocatedDays ?? 0) + (entitlement.CarryForwardDays ?? 0) - (entitlement.UsedDays ?? 0);
                        }
                        _context.LeaveEntitlements.Update(entitlement);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the update
                    System.Diagnostics.Debug.WriteLine($"Leave entitlement update error during edit: {ex.Message}");
                }

                // If approved, sync attendance
                if (status == "Approved")
                {
                    try
                    {
                        var startDate = leaveRequest.StartDate;
                        var endDate = leaveRequest.EndDate;
                        var currentDate = startDate;

                        while (currentDate <= endDate)
                        {
                            var existingAttendance = await _context.Attendances
                                .FirstOrDefaultAsync(a => a.EmployeeId == leaveRequest.EmployeeId && 
                                                          a.AttendanceDate == currentDate);

                            if (existingAttendance == null)
                            {
                                var attendance = new Attendance
                                {
                                    EmployeeId = leaveRequest.EmployeeId,
                                    AttendanceDate = currentDate,
                                    Status = "OnLeave",
                                    SourceType = "Leave",
                                    LeaveRequestId = leaveRequest.RequestId,
                                    IsLeaveException = true,
                                    CreatedAt = DateTime.Now
                                };
                                _context.Attendances.Add(attendance);
                            }
                            else
                            {
                                existingAttendance.Status = "OnLeave";
                                existingAttendance.SourceType = "Leave";
                                existingAttendance.LeaveRequestId = leaveRequest.RequestId;
                                existingAttendance.IsLeaveException = true;
                                _context.Attendances.Update(existingAttendance);
                            }

                            currentDate = currentDate.AddDays(1);
                        }
                        
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Attendance sync error: {ex.Message}");
                    }
                }

                // Send notification to employee about leave approval/rejection
                if (status == "Approved" || status == "Rejected")
                {
                    var leaveType = leaveRequest.Leave?.LeaveType ?? "Leave";
                    await _notificationService.CreateLeaveApprovalNotification(
                        leaveRequest.EmployeeId,
                        status,
                        leaveType,
                        leaveRequest.StartDate,
                        leaveRequest.EndDate
                    );
                }

                TempData["SuccessMessage"] = "Leave request updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating leave request: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // -------------------- DELETE LEAVE TYPE (HR Admin) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var leave = await _context.Leaves
                .FirstOrDefaultAsync(m => m.LeaveId == id);

            if (leave == null) return NotFound();
            return View(leave);
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost, ActionName("Delete")]    
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave != null)
            {
                // Instead of deleting, deactivate the leave type
                leave.IsActive = false;
                _context.Leaves.Update(leave);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave type deactivated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // -------------------- LEAVE BALANCE --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, Employee")]
        public async Task<IActionResult> Balance(int? employeeId)
        {
            var currentEmployeeId = employeeId ?? GetCurrentEmployeeId();
            if (currentEmployeeId == null)
            {
                TempData["ErrorMessage"] = "Employee ID not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check authorization - employees can only view their own balance
            var isHRAdmin = User.IsInRole("HRAdmin") || User.IsInRole("SystemAdmin");
            if (!isHRAdmin && currentEmployeeId != GetCurrentEmployeeId())
            {
                TempData["ErrorMessage"] = "You are not authorized to view this employee's leave balance.";
                return RedirectToAction(nameof(Balance));
            }

            try
            {
                // Try using stored procedure first
                var balance = await _context
                    .Set<LeaveBalanceDto>()
                    .FromSqlRaw("EXEC GetEmployeeLeaveBalance {0}", currentEmployeeId)
                    .ToListAsync();
                
                if (balance != null && balance.Any())
                {
                    return View(balance);
                }
            }
            catch
            {
                // Fallback to direct query if stored procedure doesn't exist
            }

            // Fallback: Calculate balance directly from LeaveEntitlement
            var currentYear = DateTime.Now.Year;
            var entitlements = await _context.LeaveEntitlements
                .Include(le => le.Leave)
                .Where(le => le.EmployeeId == currentEmployeeId.Value && le.Year == currentYear)
                .Select(le => new LeaveBalanceDto
                {
                    LeaveId = le.LeaveId,
                    LeaveType = le.Leave.LeaveType,
                    Allocated_Days = le.AllocatedDays ?? 0,
                    Balance_Days = le.BalanceDays ?? 0,
                    TotalDays = le.AllocatedDays ?? 0,
                    UsedDays = (int)(le.UsedDays ?? 0)
                })
                .ToListAsync();

            return View(entitlements);
        }

        // -------------------- MY LEAVE HISTORY --------------------
        [Authorize(Roles = "Employee, HRAdmin, SystemAdmin")]
        public async Task<IActionResult> MyLeaveHistory()
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "Employee ID not found.";
                return RedirectToAction(nameof(Index));
            }

            var history = await _context.LeaveRequests
                .Include(lr => lr.Leave)
                .Include(lr => lr.Employee)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return View(history);
        }

        // -------------------- VIEW LEAVE REQUEST DETAILS (Employee) --------------------
        [Authorize(Roles = "Employee, HRAdmin, SystemAdmin, LineManager")]
        public async Task<IActionResult> ViewLeaveRequest(int? id)
        {
            if (id == null) return NotFound();

            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.Leave)
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedByNavigation)
                .Include(lr => lr.LeaveDocuments)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (leaveRequest == null) return NotFound();

            // Check if user is viewing their own request or is a manager/HR
            var currentEmployeeId = GetCurrentEmployeeId();
            var isManager = User.IsInRole("HRAdmin") || User.IsInRole("LineManager") || User.IsInRole("SystemAdmin");
            
            if (!isManager && leaveRequest.EmployeeId != currentEmployeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to view this leave request.";
                return RedirectToAction(nameof(MyLeaveHistory));
            }

            return View(leaveRequest);
        }

        // -------------------- TEAM LEAVE REQUESTS (Manager) --------------------
        [Authorize(Roles = "HRAdmin, LineManager, SystemAdmin")]
        public async Task<IActionResult> Pending(int? managerId, string? statusFilter)
        {
            // Build query for ALL leave requests from ALL employees
            // Managers and HR Admins can see all requests
            var query = _context.LeaveRequests
                .Include(lr => lr.Leave)
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedByNavigation)
                .AsQueryable();

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(lr => lr.Status != null && lr.Status.ToLower() == statusFilter.ToLower());
            }

            var allRequests = await query
                .OrderByDescending(lr => lr.SubmittedAt)
                .ToListAsync();

            // Convert to DTO for display
            var requestDtos = allRequests.Select(lr => new PendingLeaveDto
            {
                RequestId = lr.RequestId,
                EmployeeId = lr.EmployeeId,
                EmployeeName = lr.Employee?.FullName ?? $"Employee #{lr.EmployeeId}",
                LeaveId = lr.LeaveId,
                LeaveType = lr.Leave?.LeaveType ?? lr.Leave?.LeaveDescription ?? "Unknown",
                StartDate = lr.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = lr.EndDate.ToDateTime(TimeOnly.MinValue),
                TotalDays = lr.TotalDays,
                Status = lr.Status ?? "Pending",
                Reason = lr.Reason ?? lr.Justification,
                IsIrregular = lr.IsIrregular ?? false
            })
            .OrderByDescending(lr => lr.StartDate)
            .ToList();

            ViewBag.StatusFilter = statusFilter;
            return View(requestDtos);
        }

        // -------------------- APPROVE LEAVE (Manager) --------------------
        [Authorize(Roles = "HRAdmin, LineManager, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var approverId = GetCurrentEmployeeId();
            if (approverId == null)
            {
                TempData["ErrorMessage"] = "Approver ID not found.";
                return RedirectToAction(nameof(Pending));
            }

            try
            {
                // Update leave request directly instead of using stored procedure
                var leave = await _context.LeaveRequests
                    .Include(lr => lr.Leave)
                    .FirstOrDefaultAsync(lr => lr.RequestId == id);

                if (leave == null)
                {
                    TempData["ErrorMessage"] = "Leave request not found.";
                    return RedirectToAction(nameof(Pending));
                }

                // Update leave request status
                leave.Status = "Approved";
                leave.ApprovedBy = approverId;
                leave.ApprovedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Update LeaveEntitlement - deduct used days from balance
                try
                {
                    var currentYear = DateTime.Now.Year;
                    var entitlement = await _context.LeaveEntitlements
                        .FirstOrDefaultAsync(le => le.EmployeeId == leave.EmployeeId && 
                                                   le.LeaveId == leave.LeaveId && 
                                                   le.Year == currentYear);
                    
                    if (entitlement != null)
                    {
                        entitlement.UsedDays = (entitlement.UsedDays ?? 0) + leave.TotalDays;
                        entitlement.BalanceDays = (entitlement.AllocatedDays ?? 0) + (entitlement.CarryForwardDays ?? 0) - (entitlement.UsedDays ?? 0);
                        _context.LeaveEntitlements.Update(entitlement);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the approval
                    System.Diagnostics.Debug.WriteLine($"Leave entitlement update error: {ex.Message}");
                }

                // Sync attendance - mark days as OnLeave for all days in the leave period
                try
                {
                    var startDate = leave.StartDate;
                    var endDate = leave.EndDate;
                    var currentDate = startDate;

                    while (currentDate <= endDate)
                    {
                        // Check if attendance record already exists
                        var existingAttendance = await _context.Attendances
                            .FirstOrDefaultAsync(a => a.EmployeeId == leave.EmployeeId && 
                                                      a.AttendanceDate == currentDate);

                        if (existingAttendance == null)
                        {
                            // Create new attendance record for leave
                            var attendance = new Attendance
                            {
                                EmployeeId = leave.EmployeeId,
                                AttendanceDate = currentDate,
                                Status = "OnLeave",
                                SourceType = "Leave",
                                LeaveRequestId = leave.RequestId,
                                IsLeaveException = true,
                                CreatedAt = DateTime.Now
                            };
                            _context.Attendances.Add(attendance);
                        }
                        else
                        {
                            // Update existing attendance record to OnLeave
                            existingAttendance.Status = "OnLeave";
                            existingAttendance.SourceType = "Leave";
                            existingAttendance.LeaveRequestId = leave.RequestId;
                            existingAttendance.IsLeaveException = true;
                            _context.Attendances.Update(existingAttendance);
                        }

                        currentDate = currentDate.AddDays(1);
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the leave approval
                    // The leave is already approved, attendance sync is secondary
                    System.Diagnostics.Debug.WriteLine($"Attendance sync error: {ex.Message}");
                }

                // Send notification
                var leaveType = leave.Leave?.LeaveType ?? "Leave";
                await _notificationService.CreateLeaveApprovalNotification(
                    leave.EmployeeId,
                    "Approved",
                    leaveType,
                    leave.StartDate,
                    leave.EndDate
                );

                TempData["SuccessMessage"] = "Leave request approved and attendance synchronized.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving leave request: {ex.Message}";
            }

            return RedirectToAction(nameof(Pending));
        }

        // -------------------- REJECT LEAVE (Manager) --------------------
        [Authorize(Roles = "HRAdmin, LineManager, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? rejectionReason)
        {
            var approverId = GetCurrentEmployeeId();
            if (approverId == null)
            {
                TempData["ErrorMessage"] = "Approver ID not found.";
                return RedirectToAction(nameof(Pending));
            }

            try
            {
                // Get leave request
                var leave = await _context.LeaveRequests
                    .Include(lr => lr.Leave)
                    .FirstOrDefaultAsync(lr => lr.RequestId == id);

                if (leave == null)
                {
                    TempData["ErrorMessage"] = "Leave request not found.";
                    return RedirectToAction(nameof(Pending));
                }

                // Update leave request status
                leave.Status = "Rejected";
                leave.ApprovedBy = approverId;
                leave.ApprovedAt = DateTime.Now;
                leave.RejectionReason = rejectionReason ?? "Rejected by manager";
                await _context.SaveChangesAsync();

                // Send notification
                var leaveType = leave.Leave?.LeaveType ?? "Leave";
                await _notificationService.CreateLeaveApprovalNotification(
                    leave.EmployeeId,
                    "Rejected",
                    leaveType,
                    leave.StartDate,
                    leave.EndDate
                );

                TempData["SuccessMessage"] = "Leave request rejected successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting leave request: {ex.Message}";
            }

            return RedirectToAction(nameof(Pending));
        }

        // -------------------- FLAG IRREGULAR LEAVE (Manager) --------------------
        [Authorize(Roles = "HRAdmin, LineManager, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlagIrregular(int id)
        {
            var managerId = GetCurrentEmployeeId();
            if (managerId == null)
            {
                TempData["ErrorMessage"] = "Manager ID not found.";
                return RedirectToAction(nameof(Pending));
            }

            try
            {
                var leave = await _context.LeaveRequests
                    .Include(lr => lr.Employee)
                    .Include(lr => lr.Leave)
                    .FirstOrDefaultAsync(lr => lr.RequestId == id);
                    
                if (leave == null)
                {
                    TempData["ErrorMessage"] = "Leave request not found.";
                    return RedirectToAction(nameof(Pending));
                }

                // Check if already flagged
                if (leave.IsIrregular == true)
                {
                    TempData["InfoMessage"] = "This leave request is already flagged as irregular.";
                    return RedirectToAction(nameof(Pending));
                }

                // Get manager info
                var manager = await _context.Employees.FindAsync(managerId.Value);
                var managerName = manager?.FullName ?? "Unknown Manager";

                // Flag as irregular
                leave.IsIrregular = true;
                await _context.SaveChangesAsync();

                // Notify HR Admins about the irregular leave
                try
                {
                    var employeeName = leave.Employee?.FullName ?? $"Employee #{leave.EmployeeId}";
                    var leaveType = leave.Leave?.LeaveType ?? "Unknown";
                    await _notificationService.NotifyHRAdminIrregularLeave(
                        leave.RequestId,
                        leave.EmployeeId,
                        employeeName,
                        leaveType,
                        leave.StartDate,
                        leave.EndDate,
                        managerId.Value,
                        managerName
                    );
                }
                catch (Exception notifEx)
                {
                    // Log but don't fail the flagging operation
                    System.Diagnostics.Debug.WriteLine($"Error sending HR notification: {notifEx.Message}");
                }

                TempData["SuccessMessage"] = "Leave request flagged as irregular. HR administrators have been notified.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error flagging leave request: {ex.Message}";
            }

            return RedirectToAction(nameof(Pending));
        }

        // -------------------- HR OVERRIDE LEAVE DECISION --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Override(int requestId, string status, string overrideReason)
        {
            var hrAdminId = GetCurrentEmployeeId();
            if (hrAdminId == null)
            {
                TempData["ErrorMessage"] = "HR Admin ID not found.";
                return RedirectToAction(nameof(Pending));
            }

            try
            {
                // Get leave request info
                var leave = await _context.LeaveRequests
                    .Include(lr => lr.Leave)
                    .FirstOrDefaultAsync(x => x.RequestId == requestId);

                if (leave == null)
                {
                    TempData["ErrorMessage"] = "Leave request not found.";
                    return RedirectToAction(nameof(Pending));
                }

                // Get previous status to handle entitlement reversal if needed
                var previousStatus = leave.Status;

                // Update leave request status
                leave.Status = status;
                leave.ApprovedBy = hrAdminId;
                leave.ApprovedAt = DateTime.Now;
                if (!string.IsNullOrEmpty(overrideReason))
                {
                    leave.OverrideReason = overrideReason;
                }
                leave.OverriddenAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Update LeaveEntitlement if status changed to/from Approved
                try
                {
                    var currentYear = DateTime.Now.Year;
                    var entitlement = await _context.LeaveEntitlements
                        .FirstOrDefaultAsync(le => le.EmployeeId == leave.EmployeeId && 
                                                   le.LeaveId == leave.LeaveId && 
                                                   le.Year == currentYear);
                    
                    if (entitlement != null)
                    {
                        // If previously approved and now rejected/cancelled, reverse the deduction
                        if ((previousStatus?.ToLower() == "approved") && (status.ToLower() != "approved"))
                        {
                            entitlement.UsedDays = Math.Max(0, (entitlement.UsedDays ?? 0) - leave.TotalDays);
                            entitlement.BalanceDays = (entitlement.AllocatedDays ?? 0) + (entitlement.CarryForwardDays ?? 0) - (entitlement.UsedDays ?? 0);
                        }
                        // If newly approved, deduct the days
                        else if ((previousStatus?.ToLower() != "approved") && (status.ToLower() == "approved"))
                        {
                            entitlement.UsedDays = (entitlement.UsedDays ?? 0) + leave.TotalDays;
                            entitlement.BalanceDays = (entitlement.AllocatedDays ?? 0) + (entitlement.CarryForwardDays ?? 0) - (entitlement.UsedDays ?? 0);
                        }
                        _context.LeaveEntitlements.Update(entitlement);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the override
                    System.Diagnostics.Debug.WriteLine($"Leave entitlement update error during override: {ex.Message}");
                }

                // Remove old attendance records for this leave
                try
                {
                    var oldAttendance = _context.Attendances
                        .Where(a => a.EmployeeId == leave.EmployeeId && 
                                   a.AttendanceDate >= leave.StartDate && 
                                   a.AttendanceDate <= leave.EndDate &&
                                   a.Status == "OnLeave");

                    _context.Attendances.RemoveRange(oldAttendance);

                    // If HR approved → re-sync attendance
                    if (status == "Approved")
                    {
                        var startDate = leave.StartDate;
                        var endDate = leave.EndDate;
                        var currentDate = startDate;

                        while (currentDate <= endDate)
                        {
                            var existingAttendance = await _context.Attendances
                                .FirstOrDefaultAsync(a => a.EmployeeId == leave.EmployeeId && 
                                                          a.AttendanceDate == currentDate);

                            if (existingAttendance == null)
                            {
                                var attendance = new Attendance
                                {
                                    EmployeeId = leave.EmployeeId,
                                    AttendanceDate = currentDate,
                                    Status = "OnLeave",
                                    SourceType = "Leave",
                                    LeaveRequestId = leave.RequestId,
                                    IsLeaveException = true,
                                    CreatedAt = DateTime.Now
                                };
                                _context.Attendances.Add(attendance);
                            }
                            else
                            {
                                existingAttendance.Status = "OnLeave";
                                existingAttendance.SourceType = "Leave";
                                existingAttendance.LeaveRequestId = leave.RequestId;
                                existingAttendance.IsLeaveException = true;
                                _context.Attendances.Update(existingAttendance);
                            }

                            currentDate = currentDate.AddDays(1);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Attendance sync failed, but override succeeded
                }

                // Send notification
                var leaveType = leave.Leave?.LeaveType ?? "Leave";
                await _notificationService.CreateLeaveApprovalNotification(
                    leave.EmployeeId,
                    status,
                    leaveType,
                    leave.StartDate,
                    leave.EndDate
                );

                TempData["SuccessMessage"] = "Leave decision overridden and attendance synchronized.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error overriding leave decision: {ex.Message}";
            }

            return RedirectToAction(nameof(Pending));
        }

        // -------------------- IRREGULAR LEAVE PATTERNS --------------------
        [Authorize(Roles = "HRAdmin, LineManager, SystemAdmin")]
        public async Task<IActionResult> IrregularLeaves()
        {
            // Build query for ALL leave requests - Managers and HR Admins see all irregular leaves
            var query = _context.LeaveRequests
                .Include(lr => lr.Leave)
                .Include(lr => lr.Employee)
                .AsQueryable();

            var allLeaves = await query.ToListAsync();

            // Filter leaves > 5 consecutive days or already flagged as irregular
            var irregularLeaves = allLeaves
                .Where(l => l.IsIrregular == true || 
                           (l.EndDate.ToDateTime(TimeOnly.MinValue) - l.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays + 1 > 5)
                .OrderByDescending(l => l.SubmittedAt)
                .ToList();

            return View(irregularLeaves);
        }

        // -------------------- ALL LEAVE REQUESTS (HR) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        public async Task<IActionResult> AllLeaveRequests(string? status, int? employeeId)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.Leave)
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedByNavigation)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(lr => lr.Status != null && lr.Status.ToLower() == status.ToLower());
            }

            // Filter by employee if provided
            if (employeeId.HasValue)
            {
                query = query.Where(lr => lr.EmployeeId == employeeId.Value);
            }

            var leaveRequests = await query
                .OrderByDescending(lr => lr.SubmittedAt)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.EmployeeIdFilter = employeeId;
            return View(leaveRequests);
        }

        // -------------------- CREATE LEAVE TYPE (HR Admin) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpGet]
        public IActionResult CreateLeaveType()
        {
            return View();
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeaveType(Leave leave, bool? IsPaid, bool? RequiresApproval, bool? IsActive)
        {
            // Handle checkbox values
            leave.IsPaid = IsPaid ?? false;
            leave.RequiresApproval = RequiresApproval ?? true;
            leave.IsActive = IsActive ?? true;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Leaves.Add(leave);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Leave type created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error creating leave type: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMsg += $" Inner exception: {ex.InnerException.Message}";
                    }
                    TempData["ErrorMessage"] = errorMsg;
                }
            }
            return View(leave);
        }

        // -------------------- MANAGE LEAVE ENTITLEMENTS (HR Admin) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        public async Task<IActionResult> ManageLeaveEntitlements(int? employeeId, string? leaveTypeFilter)
        {
            var query = _context.LeaveEntitlements
                .Include(le => le.Employee)
                .Include(le => le.Leave)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(le => le.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrEmpty(leaveTypeFilter))
            {
                query = query.Where(le => le.Leave != null && 
                                         (le.Leave.LeaveType.ToLower().Contains(leaveTypeFilter.ToLower()) ||
                                          le.Leave.LeaveDescription.ToLower().Contains(leaveTypeFilter.ToLower())));
            }

            var entitlements = await query
                .OrderBy(le => le.EmployeeId)
                .ThenBy(le => le.LeaveId)
                .ToListAsync();

            ViewBag.Employees = await _context.Employees.OrderBy(e => e.FullName).ToListAsync();
            ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
            ViewBag.EmployeeIdFilter = employeeId;
            ViewBag.LeaveTypeFilter = leaveTypeFilter;

            return View(entitlements);
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignLeaveEntitlement(int employeeId, string leaveType, decimal allocatedDays, int year)
        {
            try
            {
                // Find leave type by name
                var leave = await _context.Leaves
                    .FirstOrDefaultAsync(l => l.LeaveType.ToLower() == leaveType.ToLower().Trim() || 
                                             l.LeaveDescription.ToLower().Contains(leaveType.ToLower().Trim()));

                if (leave == null)
                {
                    TempData["ErrorMessage"] = $"Leave type '{leaveType}' not found. Please create it first.";
                    return RedirectToAction(nameof(ManageLeaveEntitlements));
                }

                var currentYear = year > 0 ? year : DateTime.Now.Year;
                var existing = await _context.LeaveEntitlements
                    .FirstOrDefaultAsync(le => le.EmployeeId == employeeId && 
                                              le.LeaveId == leave.LeaveId && 
                                              le.Year == currentYear);

                if (existing != null)
                {
                    existing.AllocatedDays = allocatedDays;
                    existing.BalanceDays = allocatedDays - (existing.UsedDays ?? 0);
                    _context.LeaveEntitlements.Update(existing);
                }
                else
                {
                    var entitlement = new LeaveEntitlement
                    {
                        EmployeeId = employeeId,
                        LeaveId = leave.LeaveId,
                        Year = currentYear,
                        AllocatedDays = allocatedDays,
                        UsedDays = 0,
                        BalanceDays = allocatedDays,
                        CarryForwardDays = 0
                    };
                    _context.LeaveEntitlements.Add(entitlement);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave entitlement assigned successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error assigning leave entitlement: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageLeaveEntitlements));
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustLeaveEntitlement(int employeeId, int leaveId, decimal adjustment, int year)
        {
            try
            {
                var currentYear = year > 0 ? year : DateTime.Now.Year;
                var entitlement = await _context.LeaveEntitlements
                    .FirstOrDefaultAsync(le => le.EmployeeId == employeeId && 
                                              le.LeaveId == leaveId && 
                                              le.Year == currentYear);

                if (entitlement != null)
                {
                    entitlement.AllocatedDays = (entitlement.AllocatedDays ?? 0) + adjustment;
                    entitlement.BalanceDays = (entitlement.AllocatedDays ?? 0) - (entitlement.UsedDays ?? 0);
                    _context.LeaveEntitlements.Update(entitlement);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Leave entitlement adjusted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Leave entitlement not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adjusting leave entitlement: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageLeaveEntitlements));
        }

        // -------------------- MANAGE SPECIAL LEAVES (HR Admin) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        public async Task<IActionResult> ManageSpecialLeaves()
        {
            var specialLeaves = await _context.Leaves
                .Where(l => l.LeaveType.ToLower().Contains("maternity") || 
                           l.LeaveType.ToLower().Contains("paternity") ||
                           l.LeaveType.ToLower().Contains("bereavement") ||
                           l.LeaveType.ToLower().Contains("jury") ||
                           l.LeaveType.ToLower().Contains("special"))
                .OrderBy(l => l.LeaveType)
                .ToListAsync();

            return View(specialLeaves);
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpecialLeave(string leaveType, string description, int maxDays, bool? isPaid, bool? requiresApproval)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(leaveType))
                {
                    TempData["ErrorMessage"] = "Leave type is required.";
                    return RedirectToAction(nameof(ManageSpecialLeaves));
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    TempData["ErrorMessage"] = "Description is required.";
                    return RedirectToAction(nameof(ManageSpecialLeaves));
                }

                // Check if leave type already exists
                var existingLeave = await _context.Leaves
                    .FirstOrDefaultAsync(l => l.LeaveType.ToLower().Trim() == leaveType.ToLower().Trim());

                if (existingLeave != null)
                {
                    TempData["ErrorMessage"] = $"Leave type '{leaveType}' already exists.";
                    return RedirectToAction(nameof(ManageSpecialLeaves));
                }

                var specialLeave = new Leave
                {
                    LeaveType = leaveType.Trim(),
                    LeaveDescription = description.Trim(),
                    Description = $"Special leave type: {description.Trim()}",
                    MaxDaysPerYear = maxDays,
                    IsPaid = isPaid ?? false,
                    RequiresApproval = requiresApproval ?? true,
                    IsActive = true
                };

                _context.Leaves.Add(specialLeave);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Special leave type '{leaveType}' created successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating special leave: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["ErrorMessage"] += $" Inner exception: {ex.InnerException.Message}";
                }
            }

            return RedirectToAction(nameof(ManageSpecialLeaves));
        }

        // -------------------- CONFIGURE LEAVE POLICIES (HR Admin) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        public async Task<IActionResult> ConfigureLeavePolicies(int? leaveId)
        {
            var query = _context.LeavePolicies
                .Include(lp => lp.Leave)
                .AsQueryable();

            if (leaveId.HasValue)
            {
                query = query.Where(lp => lp.LeaveId == leaveId.Value);
            }

            var policies = await query
                .OrderBy(lp => lp.LeaveId)
                .ThenBy(lp => lp.Name)
                .ToListAsync();

            ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
            ViewBag.LeaveIdFilter = leaveId;

            return View(policies);
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> CreateLeavePolicy(int? leaveId)
        {
            ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
            ViewBag.SelectedLeaveId = leaveId;
            return View();
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeavePolicy(LeavePolicy policy, string leaveType, bool? ResetOnNewYear, bool? IsActive)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(leaveType))
                {
                    TempData["ErrorMessage"] = "Leave type is required.";
                    ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
                    return View(policy);
                }

                // Look up or create leave type
                var leave = await _context.Leaves
                    .FirstOrDefaultAsync(l => l.LeaveType.ToLower().Trim() == leaveType.ToLower().Trim());

                if (leave == null)
                {
                    // Create new leave type if it doesn't exist
                    leave = new Leave
                    {
                        LeaveType = leaveType.Trim(),
                        LeaveDescription = !string.IsNullOrWhiteSpace(policy.Name) ? policy.Name : $"Leave type: {leaveType.Trim()}",
                        Description = policy.Purpose,
                        IsActive = true,
                        IsPaid = true,
                        RequiresApproval = true
                    };
                    _context.Leaves.Add(leave);
                    await _context.SaveChangesAsync();
                }

                policy.LeaveId = leave.LeaveId;

                if (policy.EffectiveDate == null)
                {
                    policy.EffectiveDate = DateOnly.FromDateTime(DateTime.Now);
                }

                // Handle checkbox values
                policy.ResetOnNewYear = ResetOnNewYear ?? false;
                policy.IsActive = IsActive ?? true;

                _context.LeavePolicies.Add(policy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave policy created successfully.";
                return RedirectToAction(nameof(ConfigureLeavePolicies), new { leaveId = policy.LeaveId });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error creating leave policy: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $" Inner exception: {ex.InnerException.Message}";
                }
                TempData["ErrorMessage"] = errorMsg;
                ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
                return View(policy);
            }
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditLeavePolicy(int? id)
        {
            if (id == null) return NotFound();

            var policy = await _context.LeavePolicies
                .Include(lp => lp.Leave)
                .FirstOrDefaultAsync(lp => lp.PolicyId == id);

            if (policy == null) return NotFound();

            ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
            return View(policy);
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeavePolicy(int id, LeavePolicy policy, string leaveType, bool? ResetOnNewYear, bool? IsActive)
        {
            if (id != policy.PolicyId) return NotFound();

            try
            {
                if (string.IsNullOrWhiteSpace(leaveType))
                {
                    TempData["ErrorMessage"] = "Leave type is required.";
                    ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
                    return View(policy);
                }

                // Look up or create leave type
                var leave = await _context.Leaves
                    .FirstOrDefaultAsync(l => l.LeaveType.ToLower().Trim() == leaveType.ToLower().Trim());

                if (leave == null)
                {
                    // Create new leave type if it doesn't exist
                    leave = new Leave
                    {
                        LeaveType = leaveType.Trim(),
                        LeaveDescription = $"Leave type: {leaveType.Trim()}",
                        IsActive = true,
                        IsPaid = true,
                        RequiresApproval = true
                    };
                    _context.Leaves.Add(leave);
                    await _context.SaveChangesAsync();
                }

                policy.LeaveId = leave.LeaveId;

                // Handle checkbox values
                policy.ResetOnNewYear = ResetOnNewYear ?? false;
                policy.IsActive = IsActive ?? true;

                _context.LeavePolicies.Update(policy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave policy updated successfully.";
                return RedirectToAction(nameof(ConfigureLeavePolicies), new { leaveId = policy.LeaveId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating leave policy: {ex.Message}";
                ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
                return View(policy);
            }
        }

        // -------------------- CONFIGURE ELIGIBILITY RULES (HR Admin) --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        public async Task<IActionResult> ConfigureEligibilityRules()
        {
            // Get all leave policies with eligibility rules
            var policies = await _context.LeavePolicies
                .Include(lp => lp.Leave)
                .Where(lp => !string.IsNullOrEmpty(lp.EligibilityRules) || lp.EligibilityMonths > 0)
                .OrderBy(lp => lp.Leave.LeaveType)
                .ToListAsync();

            ViewBag.Leaves = await _context.Leaves.Where(l => l.IsActive == true).OrderBy(l => l.LeaveType).ToListAsync();
            ViewBag.AllPolicies = await _context.LeavePolicies
                .Include(lp => lp.Leave)
                .OrderBy(lp => lp.Leave.LeaveType)
                .ToListAsync();

            return View(policies);
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEligibilityRules(int policyId, string eligibilityRules, int eligibilityMonths)
        {
            try
            {
                var policy = await _context.LeavePolicies.FindAsync(policyId);
                if (policy == null)
                {
                    TempData["ErrorMessage"] = "Leave policy not found.";
                    return RedirectToAction(nameof(ConfigureEligibilityRules));
                }

                policy.EligibilityRules = eligibilityRules;
                policy.EligibilityMonths = eligibilityMonths;
                _context.LeavePolicies.Update(policy);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Eligibility rules updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating eligibility rules: {ex.Message}";
            }

            return RedirectToAction(nameof(ConfigureEligibilityRules));
        }

        // -------------------- HELPER METHOD --------------------
        private int? GetCurrentEmployeeId()
        {
            // Use Email to find employee (matching current project's pattern)
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            // Get employee ID from database by email
            var employee = _context.Employees
                .FirstOrDefault(e => e.Email == userEmail);
            
            return employee?.EmployeeId;
        }
    }
}
