using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using Milstone3_WebApp.Services;

namespace Milstone3_WebApp.Controllers
{
    [Authorize]
    public class ShiftsController : Controller
    {
        private readonly HrPayrollSystemContext _context;
        private readonly NotificationService _notificationService;

        public ShiftsController(HrPayrollSystemContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // -------------------- LIST ALL SHIFTS --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        public async Task<IActionResult> Index()
        {
            var shifts = await _context.ShiftSchedules
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(shifts);
        }

        // -------------------- SYSTEM ADMIN: CREATE SHIFT TYPE --------------------
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShiftSchedule shift)
        {
            if (!ModelState.IsValid)
            {
                return View(shift);
            }

            // Validation
            if (shift.EndTime <= shift.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
                return View(shift);
            }

            shift.IsActive = true;
            shift.Status = "Active";
            shift.CreatedAt = DateTime.Now;

            _context.ShiftSchedules.Add(shift);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Shift type created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // -------------------- VIEW SHIFT DETAILS --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var shift = await _context.ShiftSchedules
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(sa => sa.Employee)
                .Include(s => s.ShiftAssignments)
                    .ThenInclude(sa => sa.Department)
                .FirstOrDefaultAsync(s => s.ShiftId == id);

            if (shift == null) return NotFound();

            return View(shift);
        }

        // -------------------- SYSTEM ADMIN: EDIT SHIFT --------------------
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var shift = await _context.ShiftSchedules.FindAsync(id);
            if (shift == null) return NotFound();

            return View(shift);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShiftSchedule shift)
        {
            if (id != shift.ShiftId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(shift);
            }

            if (shift.EndTime <= shift.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
                return View(shift);
            }

            try
            {
                _context.Update(shift);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Shift updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(shift.ShiftId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // -------------------- HR ADMIN: CONFIGURE ROTATIONAL SHIFTS --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> CreateRotationalShift()
        {
            ViewData["Shifts"] = new SelectList(
                await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                "ShiftId", "Name");
            return View();
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRotationalShift(ShiftCycle cycle, int[] shiftIds, int[] daySequences)
        {
            if (shiftIds == null || shiftIds.Length == 0)
            {
                ModelState.AddModelError("", "Please select at least one shift for the cycle.");
                ViewData["Shifts"] = new SelectList(
                    await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                    "ShiftId", "Name");
                return View(cycle);
            }

            cycle.IsActive = true;
            _context.ShiftCycles.Add(cycle);
            await _context.SaveChangesAsync();

            // Create shift cycle assignments
            for (int i = 0; i < shiftIds.Length; i++)
            {
                var assignment = new ShiftCycleAssignment
                {
                    CycleId = cycle.CycleId,
                    ShiftId = shiftIds[i],
                    DaySequence = daySequences != null && i < daySequences.Length ? daySequences[i] : i + 1,
                    OrderNumber = i + 1
                };
                _context.ShiftCycleAssignments.Add(assignment);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Rotational shift cycle created successfully!";
            return RedirectToAction("RotationalShifts");
        }

        // -------------------- HR ADMIN: VIEW ROTATIONAL SHIFTS --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        public async Task<IActionResult> RotationalShifts()
        {
            var cycles = await _context.ShiftCycles
                .Include(c => c.ShiftCycleAssignments)
                    .ThenInclude(sca => sca.Shift)
                .Where(c => c.IsActive == true)
                .ToListAsync();

            return View(cycles);
        }

        // -------------------- HR ADMIN: CONFIGURE SPLIT SHIFTS --------------------
        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpGet]
        public IActionResult CreateSplitShift()
        {
            return View();
        }

        [Authorize(Roles = "HRAdmin, SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSplitShift(string name, TimeOnly firstStart, TimeOnly firstEnd, 
            TimeOnly secondStart, TimeOnly secondEnd, int breakDuration)
        {
            // Create first part of split shift
            var firstShift = new ShiftSchedule
            {
                Name = $"{name} - First Part",
                Type = "Split",
                StartTime = firstStart,
                EndTime = firstEnd,
                BreakDuration = 0,
                IsActive = true,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _context.ShiftSchedules.Add(firstShift);
            await _context.SaveChangesAsync();

            // Create second part of split shift
            var secondShift = new ShiftSchedule
            {
                Name = $"{name} - Second Part",
                Type = "Split",
                StartTime = secondStart,
                EndTime = secondEnd,
                BreakDuration = breakDuration,
                IsActive = true,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _context.ShiftSchedules.Add(secondShift);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Split shift created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // -------------------- SYSTEM ADMIN & MANAGER: ASSIGN SHIFT TO EMPLOYEE --------------------
        [Authorize(Roles = "SystemAdmin, LineManager")]
        [HttpGet]
        public async Task<IActionResult> AssignToEmployee()
        {
            ViewData["EmployeeId"] = new SelectList(
                await _context.Employees.Where(e => e.IsActive == true).ToListAsync(),
                "EmployeeId", "FullName");
            ViewData["ShiftId"] = new SelectList(
                await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                "ShiftId", "Name");
            return View();
        }

        [Authorize(Roles = "SystemAdmin, LineManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToEmployee(ShiftAssignment assignment)
        {
            if (!ModelState.IsValid)
            {
                ViewData["EmployeeId"] = new SelectList(
                    await _context.Employees.Where(e => e.IsActive == true).ToListAsync(),
                    "EmployeeId", "FullName");
                ViewData["ShiftId"] = new SelectList(
                    await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                    "ShiftId", "Name");
                return View(assignment);
            }

            // Deactivate any existing active assignments for this employee
            var existingAssignments = await _context.ShiftAssignments
                .Where(sa => sa.EmployeeId == assignment.EmployeeId && 
                            sa.IsActive == true &&
                            sa.StartDate <= assignment.StartDate &&
                            (sa.EndDate == null || sa.EndDate >= assignment.StartDate))
                .ToListAsync();

            foreach (var existing in existingAssignments)
            {
                existing.IsActive = false;
            }

            assignment.AssignmentType = "Individual";
            assignment.IsActive = true;
            assignment.Status = "Active";

            _context.ShiftAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Send notification to employee about shift reassignment
            if (assignment.EmployeeId.HasValue)
            {
                var shift = await _context.ShiftSchedules.FindAsync(assignment.ShiftId);
                var shiftName = shift?.Name ?? "New Shift";
                await _notificationService.CreateShiftReassignmentNotification(
                    assignment.EmployeeId.Value,
                    shiftName,
                    assignment.StartDate
                );
            }

            TempData["SuccessMessage"] = "Shift assigned to employee successfully!";
            return RedirectToAction("Assignments");
        }

        // -------------------- SYSTEM ADMIN & MANAGER: ASSIGN SHIFT TO DEPARTMENT --------------------
        [Authorize(Roles = "SystemAdmin, LineManager")]
        [HttpGet]
        public async Task<IActionResult> AssignToDepartment()
        {
            ViewData["DepartmentId"] = new SelectList(
                await _context.Departments.ToListAsync(),
                "DepartmentId", "DepartmentName");
            ViewData["ShiftId"] = new SelectList(
                await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                "ShiftId", "Name");
            return View();
        }

        [Authorize(Roles = "SystemAdmin, LineManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToDepartment(ShiftAssignment assignment)
        {
            if (!ModelState.IsValid)
            {
                ViewData["DepartmentId"] = new SelectList(
                    await _context.Departments.ToListAsync(),
                    "DepartmentId", "DepartmentName");
                ViewData["ShiftId"] = new SelectList(
                    await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                    "ShiftId", "Name");
                return View(assignment);
            }

            // Get all active employees in the department
            var departmentEmployees = await _context.Employees
                .Where(e => e.DepartmentId == assignment.DepartmentId && e.IsActive == true)
                .ToListAsync();

            foreach (var employee in departmentEmployees)
            {
                // Deactivate existing assignments
                var existingAssignments = await _context.ShiftAssignments
                    .Where(sa => sa.EmployeeId == employee.EmployeeId && 
                                sa.IsActive == true &&
                                sa.StartDate <= assignment.StartDate &&
                                (sa.EndDate == null || sa.EndDate >= assignment.StartDate))
                    .ToListAsync();

                foreach (var existing in existingAssignments)
                {
                    existing.IsActive = false;
                }

                // Create new assignment
                var newAssignment = new ShiftAssignment
                {
                    EmployeeId = employee.EmployeeId,
                    DepartmentId = assignment.DepartmentId,
                    ShiftId = assignment.ShiftId,
                    AssignmentType = "Department",
                    StartDate = assignment.StartDate,
                    EndDate = assignment.EndDate,
                    IsActive = true,
                    Status = "Active"
                };

                _context.ShiftAssignments.Add(newAssignment);

                // Send notification to employee about shift reassignment
                var shift = await _context.ShiftSchedules.FindAsync(assignment.ShiftId);
                var shiftName = shift?.Name ?? "New Shift";
                await _notificationService.CreateShiftReassignmentNotification(
                    employee.EmployeeId,
                    shiftName,
                    assignment.StartDate
                );
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Shift assigned to {departmentEmployees.Count} employees in the department!";
            return RedirectToAction("Assignments");
        }

        // -------------------- SYSTEM ADMIN: ASSIGN CUSTOM/SPECIAL SHIFT --------------------
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> AssignCustomShift()
        {
            ViewData["EmployeeId"] = new SelectList(
                await _context.Employees.Where(e => e.IsActive == true).ToListAsync(),
                "EmployeeId", "FullName");
            return View();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCustomShift(int employeeId, string shiftName, 
            TimeOnly startTime, TimeOnly endTime, DateOnly startDate, DateOnly? endDate, int? breakDuration)
        {
            // Create custom shift
            var customShift = new ShiftSchedule
            {
                Name = shiftName,
                Type = "Custom",
                StartTime = startTime,
                EndTime = endTime,
                BreakDuration = breakDuration ?? 0,
                IsActive = true,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _context.ShiftSchedules.Add(customShift);
            await _context.SaveChangesAsync();

            // Assign to employee
            var assignment = new ShiftAssignment
            {
                EmployeeId = employeeId,
                ShiftId = customShift.ShiftId,
                AssignmentType = "Custom",
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                Status = "Active"
            };

            _context.ShiftAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Custom shift created and assigned successfully!";
            return RedirectToAction("Assignments");
        }

        // -------------------- VIEW ALL SHIFT ASSIGNMENTS --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        public async Task<IActionResult> Assignments()
        {
            var assignments = await _context.ShiftAssignments
                .Include(sa => sa.Employee)
                .Include(sa => sa.Department)
                .Include(sa => sa.Shift)
                .Where(sa => sa.IsActive == true)
                .OrderByDescending(sa => sa.StartDate)
                .ToListAsync();

            return View(assignments);
        }

        // -------------------- SYSTEM ADMIN: UPDATE SHIFT ASSIGNMENT --------------------
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditAssignment(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.ShiftAssignments
                .Include(sa => sa.Employee)
                .Include(sa => sa.Shift)
                .FirstOrDefaultAsync(sa => sa.AssignmentId == id);

            if (assignment == null) return NotFound();

            ViewData["ShiftId"] = new SelectList(
                await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                "ShiftId", "Name", assignment.ShiftId);

            return View(assignment);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssignment(int id, ShiftAssignment assignment)
        {
            if (id != assignment.AssignmentId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ShiftId"] = new SelectList(
                    await _context.ShiftSchedules.Where(s => s.IsActive == true).ToListAsync(),
                    "ShiftId", "Name", assignment.ShiftId);
                return View(assignment);
            }

            try
            {
                _context.Update(assignment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Shift assignment updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftAssignmentExists(assignment.AssignmentId))
                    return NotFound();
                throw;
            }

            return RedirectToAction("Assignments");
        }

        // -------------------- DELETE SHIFT --------------------
        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.ShiftSchedules.FindAsync(id);
            if (shift != null)
            {
                // Soft delete - mark as inactive
                shift.IsActive = false;
                shift.Status = "Inactive";
                _context.Update(shift);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Shift deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ShiftExists(int id)
        {
            return _context.ShiftSchedules.Any(e => e.ShiftId == id);
        }

        private bool ShiftAssignmentExists(int id)
        {
            return _context.ShiftAssignments.Any(e => e.AssignmentId == id);
        }
    }
}

