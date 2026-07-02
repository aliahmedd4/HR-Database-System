using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Milstone3_WebApp;
using Milstone3_WebApp.Services;

namespace Milstone3_WebApp.Controllers
{
    [Authorize] // Require authentication for all mission actions
    public class MissionsController : Controller
    {
        private readonly HrPayrollSystemContext _context;
        private readonly NotificationService _notificationService;

        public MissionsController(HrPayrollSystemContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // -------------------- LIST MISSIONS --------------------
        // HR Admins see all missions, Managers see their team's missions, Employees see their own
        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentUser = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            IQueryable<Mission> missionsQuery = _context.Missions
                .Include(m => m.Employee)
                .Include(m => m.Manager)
                .Include(m => m.AssignedByNavigation);

            // Filter based on role
            if (User.IsInRole("HRAdmin") || User.IsInRole("SystemAdmin"))
            {
                // HR Admins and System Admins see all missions
                missionsQuery = missionsQuery.OrderByDescending(m => m.StartDate);
            }
            else if (User.IsInRole("LineManager") && currentUser != null)
            {
                // Managers see missions for their team
                missionsQuery = missionsQuery
                    .Where(m => m.ManagerId == currentUser.EmployeeId || m.EmployeeId == currentUser.EmployeeId)
                    .OrderByDescending(m => m.StartDate);
            }
            else if (User.IsInRole("Employee") && currentUser != null)
            {
                // Employees see only their own missions
                missionsQuery = missionsQuery
                    .Where(m => m.EmployeeId == currentUser.EmployeeId)
                    .OrderByDescending(m => m.StartDate);
            }
            else
            {
                // Default: show nothing if role not recognized
                missionsQuery = missionsQuery.Where(m => false);
            }

            var missions = await missionsQuery.ToListAsync();
            return View(missions);
        }

        // -------------------- MISSION DETAILS --------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var mission = await _context.Missions
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null) return NotFound();
            return View(mission);
        }

        // -------------------- ASSIGN MISSION (HR Admin Only) --------------------
        [HttpGet]
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> AssignMission()
        {
            try
            {
                // Check if user has the required role
                if (!User.IsInRole("HRAdmin") && !User.IsInRole("SystemAdmin"))
                {
                    TempData["ErrorMessage"] = "You do not have permission to assign missions. Only HR Admins and System Admins can assign missions.";
                    return RedirectToAction(nameof(Index));
                }

                // Populate dropdown lists for employees and managers
                var employees = await _context.Employees
                    .Where(e => e.IsActive == true)
                    .OrderBy(e => e.FullName)
                    .Select(e => new { e.EmployeeId, e.FullName })
                    .ToListAsync();

                var managers = await _context.Employees
                    .Where(e => e.IsActive == true)
                    .OrderBy(e => e.FullName)
                    .Select(e => new { e.EmployeeId, e.FullName })
                    .ToListAsync();

                if (!employees.Any())
                {
                    TempData["ErrorMessage"] = "No active employees found. Please add employees before assigning missions.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(employees, "EmployeeId", "FullName");
                ViewBag.ManagerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName");
                ViewBag.AssignedBy = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName");

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Keep Create action for backward compatibility (redirects to AssignMission)
        [HttpGet]
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> Create()
        {
            return await AssignMission();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> AssignMission(Mission mission)
        {
            // Check if user has the required role
            if (!User.IsInRole("HRAdmin") && !User.IsInRole("SystemAdmin"))
            {
                TempData["ErrorMessage"] = "You do not have permission to assign missions.";
                return RedirectToAction(nameof(Index));
            }

            // Check if mission is null (model binding failed)
            if (mission == null)
            {
                TempData["ErrorMessage"] = "Invalid form data. Please try again.";
                return RedirectToAction(nameof(AssignMission));
            }

            // Try to get EmployeeId from form if model binding failed
            if (mission.EmployeeId == 0)
            {
                // Try different possible form field names
                var employeeIdFromForm = Request.Form["EmployeeId"].FirstOrDefault() ??
                                        Request.Form["mission.EmployeeId"].FirstOrDefault() ??
                                        Request.Form["Mission.EmployeeId"].FirstOrDefault();

                if (!string.IsNullOrEmpty(employeeIdFromForm) && employeeIdFromForm != "")
                {
                    if (int.TryParse(employeeIdFromForm, out int parsedEmployeeId) && parsedEmployeeId > 0)
                    {
                        mission.EmployeeId = parsedEmployeeId;
                        // Clear the model state error for EmployeeId if we successfully parsed it
                        ModelState.Remove("EmployeeId");
                    }
                }
            }

            // Validate required fields first
            if (string.IsNullOrWhiteSpace(mission.MissionName))
            {
                ModelState.AddModelError("MissionName", "Mission name is required.");
            }

            // Check if EmployeeId is 0 or not set (model binding might fail and default to 0)
            if (mission.EmployeeId == 0 || mission.EmployeeId == default(int))
            {
                ModelState.AddModelError("EmployeeId", "The Employee field is required.");
            }

            // Validate StartDate (DateOnly default is DateOnly.MinValue which is 0001-01-01)
            if (mission.StartDate == DateOnly.MinValue || mission.StartDate == default(DateOnly))
            {
                ModelState.AddModelError("StartDate", "Start date is required.");
            }

            // Validate end date
            if (mission.EndDate.HasValue && mission.StartDate != DateOnly.MinValue && mission.EndDate < mission.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");
            }

            // Repopulate dropdowns if validation fails
            if (!ModelState.IsValid)
            {
                await RepopulateDropdownsAsync(mission);
                return View(mission);
            }

            if (ModelState.IsValid && mission != null)
            {
                try
                {
                    // Ensure ManagerId is null if it's 0 (empty selection)
                    if (mission.ManagerId == 0)
                    {
                        mission.ManagerId = null;
                    }

                    // Ensure AssignedBy is null if it's 0 (empty selection)
                    if (mission.AssignedBy == 0)
                    {
                        mission.AssignedBy = null;
                    }

                    // Get current logged-in user's employee ID for AssignedBy
                    var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var currentUser = await _context.Employees
                            .FirstOrDefaultAsync(e => e.Email == userEmail);
                        if (currentUser != null)
                        {
                            mission.AssignedBy = currentUser.EmployeeId;
                        }
                    }

                    // Set default status to "Pending" for manager approval, otherwise "Assigned"
                    if (string.IsNullOrWhiteSpace(mission.Status))
                    {
                        mission.Status = mission.ManagerId.HasValue ? "Pending" : "Assigned";
                    }

                    // Verify employee exists
                    var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == mission.EmployeeId);
                    if (!employeeExists)
                    {
                        ModelState.AddModelError("EmployeeId", "Selected employee does not exist.");
                        throw new InvalidOperationException("Selected employee does not exist.");
                    }

                    // Verify manager exists if provided
                    if (mission.ManagerId.HasValue)
                    {
                        var managerExists = await _context.Employees.AnyAsync(e => e.EmployeeId == mission.ManagerId.Value);
                        if (!managerExists)
                        {
                            ModelState.AddModelError("ManagerId", "Selected manager does not exist.");
                            throw new InvalidOperationException("Selected manager does not exist.");
                        }
                    }

                    // Insert directly into Mission table
                    _context.Missions.Add(mission);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        // Send notification to employee about mission assignment
                        await _notificationService.CreateMissionUpdateNotification(
                            mission.EmployeeId,
                            mission.MissionName,
                            mission.Status ?? "Assigned",
                            $"A new mission '{mission.MissionName}' has been assigned to you. Status: {mission.Status ?? "Assigned"}"
                        );

                        TempData["SuccessMessage"] = "Mission assigned successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to save the mission. Please try again.";
                    }
                }
                catch (DbUpdateException ex)
                {
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", $"Database error: {errorMessage}");
                    TempData["ErrorMessage"] = $"Database error: {errorMessage}";
                    System.Diagnostics.Debug.WriteLine($"Database error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", $"An error occurred: {errorMessage}");
                    TempData["ErrorMessage"] = $"Error: {errorMessage}";
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
            else
            {
                // Log validation errors for debugging
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToList();
                foreach (var error in errors)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation error in {error.Key}: {err.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
            }

            // Repopulate dropdown lists if validation fails
            var employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            var managers = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(employees, "EmployeeId", "FullName", mission?.EmployeeId ?? 0);
            ViewBag.ManagerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName", mission?.ManagerId);
            ViewBag.AssignedBy = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName", mission?.AssignedBy);

            return View(mission ?? new Mission());
        }

        // Helper method to repopulate dropdowns
        private async Task RepopulateDropdownsAsync(Mission mission = null)
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            var managers = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(employees, "EmployeeId", "FullName", mission?.EmployeeId ?? 0);
            ViewBag.ManagerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName", mission?.ManagerId);
            ViewBag.AssignedBy = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName", mission?.AssignedBy);
        }

        // -------------------- MANAGER: APPROVE/REJECT MISSION REQUESTS --------------------
        [HttpGet]
        [Authorize(Roles = "LineManager")]
        public async Task<IActionResult> PendingApprovals()
        {
            // Get current logged-in manager
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (manager == null)
            {
                return NotFound("Manager not found");
            }

            // Get missions pending approval for this manager
            var pendingMissions = await _context.Missions
                .Include(m => m.Employee)
                .Include(m => m.AssignedByNavigation)
                .Where(m => m.ManagerId == manager.EmployeeId &&
                           (m.Status == "Pending" || m.Status == "Assigned"))
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();

            ViewBag.ManagerName = manager.FullName;
            return View(pendingMissions);
        }

        [HttpGet]
        [Authorize(Roles = "LineManager")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null) return NotFound();

            var mission = await _context.Missions
                .Include(m => m.Employee)
                .Include(m => m.Manager)
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null) return NotFound();

            // Verify the current user is the manager for this mission
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (manager == null || mission.ManagerId != manager.EmployeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to approve this mission.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            return View(mission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "LineManager")]
        public async Task<IActionResult> Approve(int id, string remarks)
        {
            var mission = await _context.Missions
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null)
            {
                return NotFound();
            }

            // Verify the current user is the manager for this mission
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (manager == null || mission.ManagerId != manager.EmployeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to approve this mission.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            mission.Status = "Approved";
            await _context.SaveChangesAsync();

            // Send notification to employee about mission approval
            await _notificationService.CreateMissionUpdateNotification(
                mission.EmployeeId,
                mission.MissionName,
                "Approved",
                $"Your mission '{mission.MissionName}' has been approved by your manager."
            );

            TempData["SuccessMessage"] = "Mission approved successfully.";
            return RedirectToAction(nameof(PendingApprovals));
        }

        [HttpGet]
        [Authorize(Roles = "LineManager")]
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null) return NotFound();

            var mission = await _context.Missions
                .Include(m => m.Employee)
                .Include(m => m.Manager)
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null) return NotFound();

            // Verify the current user is the manager for this mission
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (manager == null || mission.ManagerId != manager.EmployeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to reject this mission.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            return View(mission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "LineManager")]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            var mission = await _context.Missions
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null)
            {
                return NotFound();
            }

            // Verify the current user is the manager for this mission
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (manager == null || mission.ManagerId != manager.EmployeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to reject this mission.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                ModelState.AddModelError("RejectionReason", "Rejection reason is required.");
                return View(mission);
            }

            mission.Status = "Rejected";
            await _context.SaveChangesAsync();

            // Send notification to employee about mission rejection
            await _notificationService.CreateMissionUpdateNotification(
                mission.EmployeeId,
                mission.MissionName,
                "Rejected",
                $"Your mission '{mission.MissionName}' has been rejected. Reason: {rejectionReason ?? "No reason provided"}"
            );

            TempData["SuccessMessage"] = $"Mission rejected. Reason: {rejectionReason}";
            return RedirectToAction(nameof(PendingApprovals));
        }

        // -------------------- EDIT MISSION (HR Admin Only) --------------------
        [HttpGet]
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mission = await _context.Missions
                .Include(m => m.Employee)
                .Include(m => m.Manager)
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null) return NotFound();

            // Populate dropdown lists
            var employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            var managers = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(employees, "EmployeeId", "FullName", mission.EmployeeId);
            ViewBag.ManagerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName", mission.ManagerId);

            return View(mission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> Edit(int id, Mission mission)
        {
            if (id != mission.MissionId)
            {
                return NotFound();
            }

            // Validate end date
            if (mission.EndDate.HasValue && mission.EndDate < mission.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mission);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Mission updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MissionExists(mission.MissionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Repopulate dropdown lists if validation fails
            var employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            var managers = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName })
                .ToListAsync();

            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(employees, "EmployeeId", "FullName", mission.EmployeeId);
            ViewBag.ManagerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(managers, "EmployeeId", "FullName", mission.ManagerId);

            return View(mission);
        }

        // -------------------- DELETE MISSION (HR Admin Only) --------------------
        [HttpGet]
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var mission = await _context.Missions
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null) return NotFound();
            return View(mission);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission != null)
            {
                _context.Missions.Remove(mission);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mission deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // -------------------- VIEW MY MISSIONS (Employees Only) --------------------
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyMissions()
        {
            // Get current logged-in user's employee ID
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Get missions assigned to this employee
            var myMissions = await _context.Missions
                .Include(m => m.Manager)
                .Include(m => m.AssignedByNavigation)
                .Where(m => m.EmployeeId == employee.EmployeeId)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();

            ViewBag.EmployeeName = employee.FullName;
            return View(myMissions);
        }
        private bool MissionExists(int id)
        {
            return _context.Missions.Any(e => e.MissionId == id);
        }
    }
}
