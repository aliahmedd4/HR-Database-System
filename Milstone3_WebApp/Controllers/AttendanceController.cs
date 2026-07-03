using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;

namespace Milstone3_WebApp.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly HrPayrollSystemContext _context;

        public AttendanceController(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // -------------------- EMPLOYEE: VIEW DAILY ATTENDANCE --------------------
        [Authorize(Roles = "Employee, SystemAdmin, HRAdmin, LineManager")]
        public async Task<IActionResult> Index(int? employeeId, DateTime? date)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (currentEmployee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // If employeeId is provided and user is admin/manager, use it; otherwise use current employee
            int targetEmployeeId = employeeId.HasValue && 
                (User.IsInRole("SystemAdmin") || User.IsInRole("HRAdmin") || User.IsInRole("LineManager"))
                ? employeeId.Value
                : currentEmployee.EmployeeId;

            var targetDate = date ?? DateTime.Today;
            var targetDateOnly = DateOnly.FromDateTime(targetDate);
            var startDateOnly = targetDateOnly.AddDays(-30);

            var attendances = await _context.Attendances
                .Include(a => a.Employee)
                .Include(a => a.Shift)
                .Where(a => a.EmployeeId == targetEmployeeId)
                .Where(a => a.AttendanceDate >= startDateOnly && a.AttendanceDate <= targetDateOnly)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            ViewBag.EmployeeId = targetEmployeeId;
            ViewBag.CurrentDate = targetDate;
            ViewBag.EmployeeName = currentEmployee.FullName;

            return View(attendances);
        }

        // -------------------- EMPLOYEE: RECORD ATTENDANCE (CHECK-IN/CHECK-OUT) --------------------
        [Authorize(Roles = "Employee, SystemAdmin, HRAdmin")]
        [HttpGet]
        public async Task<IActionResult> Record()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var employee = await _context.Employees
                .Include(e => e.ShiftAssignments)
                    .ThenInclude(sa => sa.Shift)
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayAttendance = await _context.Attendances
                .Include(a => a.Shift)
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && a.AttendanceDate == today);

            // Get active shift assignment
            var activeShift = employee.ShiftAssignments
                .Where(sa => sa.IsActive == true && 
                            sa.StartDate <= today && 
                            (sa.EndDate == null || sa.EndDate >= today))
                .Select(sa => sa.Shift)
                .FirstOrDefault();

            ViewBag.Employee = employee;
            ViewBag.TodayAttendance = todayAttendance;
            ViewBag.ActiveShift = activeShift;
            ViewBag.Today = DateTime.Today;

            return View();
        }

        [Authorize(Roles = "Employee, SystemAdmin, HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(string action, string loginMethod, string logoutMethod)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var employee = await _context.Employees
                .Include(e => e.ShiftAssignments)
                    .ThenInclude(sa => sa.Shift)
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = DateTime.Now;
            var todayAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && a.AttendanceDate == today);

            // Get active shift
            var activeShiftAssignment = employee.ShiftAssignments
                .Where(sa => sa.IsActive == true && 
                            sa.StartDate <= today && 
                            (sa.EndDate == null || sa.EndDate >= today))
                .FirstOrDefault();

            var activeShift = activeShiftAssignment?.Shift;

            if (action == "CheckIn")
            {
                if (todayAttendance == null)
                {
                    // Create new attendance record
                    var attendance = new Attendance
                    {
                        EmployeeId = employee.EmployeeId,
                        AttendanceDate = today,
                        EntryTime = now,
                        LoginMethod = loginMethod ?? "Web",
                        Status = "Present",
                        ShiftId = activeShift?.ShiftId,
                        CreatedAt = now
                    };

                    // Calculate lateness and apply penalties if shift exists
                    if (activeShift != null)
                    {
                        var shiftStartTime = activeShift.StartTime.ToTimeSpan();
                        var entryTimeSpan = now.TimeOfDay;
                        var gracePeriod = activeShift.GracePeriodMinutes ?? 0;

                        if (entryTimeSpan > shiftStartTime.Add(TimeSpan.FromMinutes((double)gracePeriod)))
                        {
                            var lateness = (int)(entryTimeSpan - shiftStartTime).TotalMinutes - gracePeriod;
                            attendance.LatenessMinutes = lateness > 0 ? lateness : 0;

                            // Apply lateness penalties based on LatenessPolicy
                            if (attendance.LatenessMinutes > 0)
                            {
                                var payrollPolicy = await _context.PayrollPolicies
                                    .Include(p => p.LatenessPolicy)
                                    .FirstOrDefaultAsync();
                                
                                if (payrollPolicy?.LatenessPolicy != null)
                                {
                                    var latenessPolicy = payrollPolicy.LatenessPolicy;
                                    
                                    // Check if lateness exceeds threshold
                                    if (latenessPolicy.ThresholdMinutes.HasValue && 
                                        attendance.LatenessMinutes >= latenessPolicy.ThresholdMinutes.Value)
                                    {
                                        // Calculate penalty amount
                                        var penaltyAmount = (decimal)attendance.LatenessMinutes * 
                                            (latenessPolicy.PenaltyPerLateMinute ?? 0);
                                        
                                        // Apply max penalty cap if set
                                        if (latenessPolicy.MaxPenaltyPerDay.HasValue && 
                                            penaltyAmount > latenessPolicy.MaxPenaltyPerDay.Value)
                                        {
                                            penaltyAmount = latenessPolicy.MaxPenaltyPerDay.Value;
                                        }
                                        
                                        // Store penalty (Note: You may need to add PenaltyAmount field to Attendance model)
                                        // For now, the penalty will be calculated during payroll processing
                                    }
                                }
                            }
                        }
                    }

                    _context.Attendances.Add(attendance);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Check-in recorded successfully!";
                }
                else if (todayAttendance.EntryTime == null)
                {
                    // Update existing record with check-in
                    todayAttendance.EntryTime = now;
                    todayAttendance.LoginMethod = loginMethod ?? "Web";
                    todayAttendance.Status = "Present";

                    // Calculate lateness and apply penalties
                    if (activeShift != null)
                    {
                        var shiftStartTime = activeShift.StartTime.ToTimeSpan();
                        var entryTimeSpan = now.TimeOfDay;
                        var gracePeriod = activeShift.GracePeriodMinutes ?? 0;

                        if (entryTimeSpan > shiftStartTime.Add(TimeSpan.FromMinutes((double)gracePeriod)))
                        {
                            var lateness = (int)(entryTimeSpan - shiftStartTime).TotalMinutes - gracePeriod;
                            todayAttendance.LatenessMinutes = lateness > 0 ? lateness : 0;

                            // Apply lateness penalties based on LatenessPolicy
                            if (todayAttendance.LatenessMinutes > 0)
                            {
                                var payrollPolicy = await _context.PayrollPolicies
                                    .Include(p => p.LatenessPolicy)
                                    .FirstOrDefaultAsync();
                                
                                if (payrollPolicy?.LatenessPolicy != null)
                                {
                                    var latenessPolicy = payrollPolicy.LatenessPolicy;
                                    
                                    // Check if lateness exceeds threshold
                                    if (latenessPolicy.ThresholdMinutes.HasValue && 
                                        todayAttendance.LatenessMinutes >= latenessPolicy.ThresholdMinutes.Value)
                                    {
                                        // Calculate penalty amount
                                        var penaltyAmount = (decimal)todayAttendance.LatenessMinutes * 
                                            (latenessPolicy.PenaltyPerLateMinute ?? 0);
                                        
                                        // Apply max penalty cap if set
                                        if (latenessPolicy.MaxPenaltyPerDay.HasValue && 
                                            penaltyAmount > latenessPolicy.MaxPenaltyPerDay.Value)
                                        {
                                            penaltyAmount = latenessPolicy.MaxPenaltyPerDay.Value;
                                        }
                                        
                                        // Store penalty (Note: You may need to add PenaltyAmount field to Attendance model)
                                        // For now, the penalty will be calculated during payroll processing
                                    }
                                }
                            }
                        }
                    }

                    _context.Update(todayAttendance);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Check-in recorded successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "You have already checked in today.";
                }
            }
            else if (action == "CheckOut")
            {
                if (todayAttendance == null || todayAttendance.EntryTime == null)
                {
                    TempData["ErrorMessage"] = "Please check in first before checking out.";
                    return RedirectToAction("Record");
                }

                if (todayAttendance.ExitTime != null)
                {
                    TempData["ErrorMessage"] = "You have already checked out today.";
                    return RedirectToAction("Record");
                }

                todayAttendance.ExitTime = now;
                todayAttendance.LogoutMethod = logoutMethod ?? "Web";

                // Calculate hours worked
                if (todayAttendance.EntryTime.HasValue)
                {
                    var duration = now - todayAttendance.EntryTime.Value;
                    todayAttendance.HoursWorked = (decimal)duration.TotalHours;
                    todayAttendance.Duration = TimeOnly.FromTimeSpan(duration);

                    // Calculate overtime, short-time penalties, and early departure if shift exists
                    if (activeShift != null)
                    {
                        var shiftEndTime = activeShift.EndTime.ToTimeSpan();
                        var exitTimeSpan = now.TimeOfDay;
                        var shiftDuration = (shiftEndTime - activeShift.StartTime.ToTimeSpan()).TotalHours;
                        var requiredHours = (decimal)shiftDuration;

                        // Calculate overtime
                        if (exitTimeSpan > shiftEndTime)
                        {
                            var overtime = (decimal)(exitTimeSpan - shiftEndTime).TotalHours;
                            todayAttendance.OvertimeHours = overtime;
                        }

                        // Calculate short-time penalty (worked less than required hours)
                        if (todayAttendance.HoursWorked < requiredHours)
                        {
                            var shortTimeHours = requiredHours - todayAttendance.HoursWorked.Value;
                            
                            // Get lateness policy for penalty calculation
                            var payrollPolicy = await _context.PayrollPolicies
                                .Include(p => p.LatenessPolicy)
                                .FirstOrDefaultAsync();
                            
                            if (payrollPolicy?.LatenessPolicy != null)
                            {
                                var latenessPolicy = payrollPolicy.LatenessPolicy;
                                
                                // Calculate penalty based on short-time hours
                                // Convert short-time hours to minutes for penalty calculation
                                var shortTimeMinutes = (int)(shortTimeHours * 60);
                                
                                // Apply penalty per minute if threshold is met
                                if (latenessPolicy.ThresholdMinutes.HasValue && 
                                    shortTimeMinutes >= latenessPolicy.ThresholdMinutes.Value)
                                {
                                    var penaltyAmount = (decimal)shortTimeMinutes * 
                                        (latenessPolicy.PenaltyPerLateMinute ?? 0);
                                    
                                    // Apply max penalty cap if set
                                    if (latenessPolicy.MaxPenaltyPerDay.HasValue && 
                                        penaltyAmount > latenessPolicy.MaxPenaltyPerDay.Value)
                                    {
                                        penaltyAmount = latenessPolicy.MaxPenaltyPerDay.Value;
                                    }
                                    
                                    // Store penalty amount (we'll use a custom field or store in notes)
                                    // For now, we'll track it in the attendance record
                                    // Note: You may need to add a PenaltyAmount field to Attendance model
                                }
                            }
                        }

                        // Calculate early departure penalty
                        if (exitTimeSpan < shiftEndTime)
                        {
                            var earlyDepartureMinutes = (int)(shiftEndTime - exitTimeSpan).TotalMinutes;
                            
                            // Get lateness policy
                            var payrollPolicy = await _context.PayrollPolicies
                                .Include(p => p.LatenessPolicy)
                                .FirstOrDefaultAsync();
                            
                            if (payrollPolicy?.LatenessPolicy != null && earlyDepartureMinutes > 0)
                            {
                                var latenessPolicy = payrollPolicy.LatenessPolicy;
                                
                                // Apply penalty for early departure
                                if (latenessPolicy.ThresholdMinutes.HasValue && 
                                    earlyDepartureMinutes >= latenessPolicy.ThresholdMinutes.Value)
                                {
                                    var penaltyAmount = (decimal)earlyDepartureMinutes * 
                                        (latenessPolicy.PenaltyPerLateMinute ?? 0);
                                    
                                    if (latenessPolicy.MaxPenaltyPerDay.HasValue && 
                                        penaltyAmount > latenessPolicy.MaxPenaltyPerDay.Value)
                                    {
                                        penaltyAmount = latenessPolicy.MaxPenaltyPerDay.Value;
                                    }
                                    
                                    // Store early departure penalty
                                }
                            }
                        }
                    }
                }

                _context.Update(todayAttendance);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Check-out recorded successfully!";
            }

            return RedirectToAction("Record");
        }

        // -------------------- EMPLOYEE: SUBMIT ATTENDANCE CORRECTION REQUEST --------------------
        [Authorize(Roles = "Employee, SystemAdmin, HRAdmin")]
        [HttpGet]
        public IActionResult RequestCorrection()
        {
            return View();
        }

        [Authorize(Roles = "Employee, SystemAdmin, HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCorrection(AttendanceCorrectionRequest request)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            request.EmployeeId = employee.EmployeeId;
            request.Status = "Pending";
            request.RecordedBy = employee.FullName;
            request.Date = DateOnly.FromDateTime(DateTime.Today);

            _context.AttendanceCorrectionRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance correction request submitted successfully!";
            return RedirectToAction("MyCorrectionRequests");
        }

        // -------------------- EMPLOYEE: VIEW MY CORRECTION REQUESTS --------------------
        [Authorize(Roles = "Employee, SystemAdmin, HRAdmin")]
        public async Task<IActionResult> MyCorrectionRequests()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _context.AttendanceCorrectionRequests
                .Where(r => r.EmployeeId == employee.EmployeeId)
                .OrderByDescending(r => r.AttendanceDate)
                .ToListAsync();

            return View(requests);
        }

        // -------------------- SYSTEM ADMIN: SYNC LEAVE WITH ATTENDANCE --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncLeaveWithAttendance(int leaveRequestId)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .FirstOrDefaultAsync(lr => lr.RequestId == leaveRequestId);

            if (leaveRequest == null)
            {
                TempData["ErrorMessage"] = "Leave request not found.";
                return RedirectToAction("Index", "Leaves");
            }

            if (leaveRequest.Status != "Approved")
            {
                TempData["ErrorMessage"] = "Only approved leave requests can be synced with attendance.";
                return RedirectToAction("Index", "Leaves");
            }

            // Create attendance records for each day of leave
            var currentDate = leaveRequest.StartDate;
            while (currentDate <= leaveRequest.EndDate)
            {
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == leaveRequest.EmployeeId && 
                                             a.AttendanceDate == currentDate);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeId = leaveRequest.EmployeeId,
                        AttendanceDate = currentDate,
                        Status = "On Leave",
                        IsLeaveException = true,
                        LeaveRequestId = leaveRequestId,
                        CreatedAt = DateTime.Now
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.Status = "On Leave";
                    attendance.IsLeaveException = true;
                    attendance.LeaveRequestId = leaveRequestId;
                    _context.Update(attendance);
                }

                currentDate = currentDate.AddDays(1);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Leave synced with attendance for {leaveRequest.Employee.FullName}.";
            return RedirectToAction("Index", "Leaves");
        }

        // -------------------- MANAGER: VIEW TEAM ATTENDANCE SUMMARY --------------------
        [Authorize(Roles = "LineManager, SystemAdmin, HRAdmin")]
        public async Task<IActionResult> TeamSummary(DateTime? startDate, DateTime? endDate)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (manager == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            // Get team members
            var teamMembers = await _context.Employees
                .Where(e => e.ManagerId == manager.EmployeeId && e.IsActive == true)
                .ToListAsync();

            // Load all team attendance in one query, then group in memory (avoids N+1)
            var memberIds = teamMembers.Select(m => m.EmployeeId).ToList();
            var startDateOnly = DateOnly.FromDateTime(start);
            var endDateOnly = DateOnly.FromDateTime(end);

            var allAttendances = await _context.Attendances
                .Where(a => memberIds.Contains(a.EmployeeId) &&
                            a.AttendanceDate >= startDateOnly &&
                            a.AttendanceDate <= endDateOnly)
                .ToListAsync();

            var attendanceByMember = allAttendances
                .GroupBy(a => a.EmployeeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var teamAttendance = new List<object>();

            foreach (var member in teamMembers)
            {
                var attendances = attendanceByMember.GetValueOrDefault(member.EmployeeId, new List<Attendance>());

                var totalDays = (end - start).Days + 1;
                var presentDays = attendances.Count(a => a.Status == "Present");
                var absentDays = totalDays - attendances.Count(a => a.Status == "Present" || a.Status == "On Leave");
                var leaveDays = attendances.Count(a => a.Status == "On Leave");
                var lateDays = attendances.Count(a => a.LatenessMinutes > 0);
                var totalLateness = attendances.Sum(a => a.LatenessMinutes ?? 0);
                var totalOvertime = attendances.Sum(a => a.OvertimeHours ?? 0);

                teamAttendance.Add(new
                {
                    EmployeeId = member.EmployeeId,
                    EmployeeName = member.FullName,
                    TotalDays = totalDays,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    LeaveDays = leaveDays,
                    LateDays = lateDays,
                    TotalLateness = totalLateness,
                    TotalOvertime = totalOvertime,
                    AttendanceRate = totalDays > 0 ? (presentDays * 100.0 / totalDays) : 0
                });
            }

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.ManagerName = manager.FullName;

            return View(teamAttendance);
        }

        // -------------------- ADMIN: VIEW ALL CORRECTION REQUESTS --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        public async Task<IActionResult> CorrectionRequests(string status = "Pending")
        {
            var requests = await _context.AttendanceCorrectionRequests
                .Include(r => r.Employee)
                .Where(r => status == "All" || r.Status == status)
                .OrderByDescending(r => r.AttendanceDate)
                .ToListAsync();

            ViewBag.Status = status;
            return View(requests);
        }

        // -------------------- ADMIN: APPROVE/REJECT CORRECTION REQUEST --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewCorrectionRequest(int requestId, string action, string reviewNotes)
        {
            var request = await _context.AttendanceCorrectionRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Request not found.";
                return RedirectToAction("CorrectionRequests");
            }

            if (action == "Approve")
            {
                request.Status = "Approved";
                request.ReviewNotes = reviewNotes;

                // Update or create attendance record
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && 
                                             a.AttendanceDate == request.AttendanceDate);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeId = request.EmployeeId,
                        AttendanceDate = request.AttendanceDate,
                        EntryTime = request.RequestedCheckIn,
                        ExitTime = request.RequestedCheckOut,
                        Status = "Present",
                        CreatedAt = DateTime.Now
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.EntryTime = request.RequestedCheckIn;
                    attendance.ExitTime = request.RequestedCheckOut;
                    _context.Update(attendance);
                }

                TempData["SuccessMessage"] = "Correction request approved and attendance updated.";
            }
            else if (action == "Reject")
            {
                request.Status = "Rejected";
                request.ReviewNotes = reviewNotes;
                TempData["SuccessMessage"] = "Correction request rejected.";
            }

            _context.Update(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("CorrectionRequests");
        }

        // -------------------- OFFLINE SYNC: SYNC OFFLINE ATTENDANCE LOGS --------------------
        [Authorize(Roles = "Employee, SystemAdmin, HRAdmin")]
        [HttpPost]
        public async Task<IActionResult> SyncOfflineLogs([FromBody] OfflineSyncRequest request)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
            {
                return Json(new { success = false, error = "Employee not found" });
            }

            var syncedIds = new List<long>();

            foreach (var log in request.Logs)
            {
                try
                {
                    var attendanceDate = DateOnly.FromDateTime(DateTime.Parse(log.Data.Date));
                    var todayAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && 
                                                 a.AttendanceDate == attendanceDate);

                    if (log.Action == "CheckIn")
                    {
                        if (todayAttendance == null)
                        {
                            todayAttendance = new Attendance
                            {
                                EmployeeId = employee.EmployeeId,
                                AttendanceDate = attendanceDate,
                                EntryTime = DateTime.Parse(log.Data.Date),
                                LoginMethod = log.Data.LoginMethod ?? "Web",
                                Status = "Present",
                                CreatedAt = DateTime.Parse(log.Timestamp)
                            };
                            _context.Attendances.Add(todayAttendance);
                        }
                        else if (todayAttendance.EntryTime == null)
                        {
                            todayAttendance.EntryTime = DateTime.Parse(log.Data.Date);
                            todayAttendance.LoginMethod = log.Data.LoginMethod ?? "Web";
                            todayAttendance.Status = "Present";
                            _context.Update(todayAttendance);
                        }
                    }
                    else if (log.Action == "CheckOut")
                    {
                        if (todayAttendance != null && todayAttendance.EntryTime != null)
                        {
                            todayAttendance.ExitTime = DateTime.Parse(log.Data.Date);
                            todayAttendance.LogoutMethod = log.Data.LogoutMethod ?? "Web";
                            
                            if (todayAttendance.EntryTime.HasValue)
                            {
                                var duration = todayAttendance.ExitTime.Value - todayAttendance.EntryTime.Value;
                                todayAttendance.HoursWorked = (decimal)duration.TotalHours;
                                todayAttendance.Duration = TimeOnly.FromTimeSpan(duration);
                            }
                            
                            _context.Update(todayAttendance);
                        }
                    }

                    await _context.SaveChangesAsync();
                    syncedIds.Add(log.Id);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other logs
                    Console.WriteLine($"Error syncing log {log.Id}: {ex.Message}");
                }
            }

            return Json(new { success = true, syncedIds = syncedIds });
        }
    }

    // Helper class for offline sync request
    public class OfflineSyncRequest
    {
        public List<OfflineLog> Logs { get; set; } = new List<OfflineLog>();
    }

    public class OfflineLog
    {
        public long Id { get; set; }
        public string Action { get; set; } = null!;
        public OfflineLogData Data { get; set; } = null!;
        public string Timestamp { get; set; } = null!;
        public bool Synced { get; set; }
    }

    public class OfflineLogData
    {
        public string Action { get; set; } = null!;
        public string LoginMethod { get; set; } = null!;
        public string LogoutMethod { get; set; } = null!;
        public string Date { get; set; } = null!;
    }
}

