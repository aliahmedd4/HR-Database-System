using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace Milstone3_WebApp.Controllers
{
    [Authorize]
    public class HierarchyController : Controller
    {
        private readonly HrPayrollSystemContext _context;

        public HierarchyController(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // -------------------- SHOW ORGANIZATIONAL HIERARCHY --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin")]
        public async Task<IActionResult> Index(int? departmentId, int? managerId)
        {
            // Get all active employees with their relationships
            var employeesQuery = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .Where(e => e.IsActive == true);

            // Filter by department if specified
            if (departmentId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
            }

            // Filter by manager if specified
            if (managerId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.ManagerId == managerId.Value);
            }

            var employees = await employeesQuery
                .OrderBy(e => e.DepartmentId)
                .ThenBy(e => e.ManagerId)
                .ThenBy(e => e.FullName)
                .ToListAsync();

            // Get departments for filtering
            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            // Get managers for filtering
            var managers = await _context.Employees
                .Where(e => e.IsActive == true && 
                           _context.Employees.Any(sub => sub.ManagerId == e.EmployeeId))
                .OrderBy(e => e.FullName)
                .ToListAsync();

            // Build hierarchy structure for visual display
            var hierarchy = new
            {
                Departments = departments.Select(d => new
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    DepartmentHead = d.DepartmentHeadId.HasValue ? 
                        employees.FirstOrDefault(e => e.EmployeeId == d.DepartmentHeadId.Value)?.FullName : null,
                    Employees = employees.Where(e => e.DepartmentId == d.DepartmentId)
                        .GroupBy(e => e.ManagerId)
                        .Select(g => new
                        {
                            ManagerId = g.Key,
                            ManagerName = g.Key.HasValue ? 
                                employees.FirstOrDefault(e => e.EmployeeId == g.Key.Value)?.FullName : "No Manager",
                            TeamMembers = g.Select(e => new
                            {
                                e.EmployeeId,
                                e.FullName,
                                e.Email,
                                Position = e.Position != null ? e.Position.PositionTitle : "N/A",
                                HasSubordinates = employees.Any(sub => sub.ManagerId == e.EmployeeId)
                            }).ToList()
                        }).ToList()
                }).ToList()
            };

            ViewBag.Departments = departments;
            ViewBag.Managers = managers;
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SelectedManagerId = managerId;
            ViewBag.Hierarchy = hierarchy;

            return View(employees);
        }

        // -------------------- REASSIGN EMPLOYEE --------------------
        [HttpGet]
        public async Task<IActionResult> Reassign(int? id)
        {
            // No employee chosen yet → show a picker of all employees
            if (id == null)
            {
                var allEmployees = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.Manager)
                    .Where(e => e.IsActive == true)
                    .OrderBy(e => e.FullName)
                    .ToListAsync();
                return View("ReassignPicker", allEmployees);
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            // Get list of potential managers (excluding the employee themselves)
            var potentialManagers = await _context.Employees
                .Where(e => e.IsActive == true && e.EmployeeId != id)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.EmployeeId, e.FullName, e.PositionId })
                .ToListAsync();

            // Get all departments
            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            ViewBag.PotentialManagers = potentialManagers;
            ViewBag.Departments = departments;

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reassign(int employeeId, int? newManagerId, int newDepartmentId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            // Validate: Employee cannot be their own manager
            if (newManagerId.HasValue && newManagerId.Value == employeeId)
            {
                TempData["ErrorMessage"] = "An employee cannot be their own manager.";
                return RedirectToAction(nameof(Reassign), new { id = employeeId });
            }

            try
            {
                employee.ManagerId = newManagerId;
                employee.DepartmentId = newDepartmentId;
                employee.UpdatedAt = System.DateTime.Now;

                _context.Update(employee);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{employee.FullName} has been reassigned successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error reassigning employee: {ex.Message}";
                return RedirectToAction(nameof(Reassign), new { id = employeeId });
            }
        }

        // -------------------- DEPARTMENT VIEW --------------------
        public async Task<IActionResult> DepartmentView(int? id)
        {
            // No department chosen yet → show a picker of all departments
            if (id == null)
            {
                var departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                var counts = await _context.Employees
                    .Where(e => e.IsActive == true && e.DepartmentId != null)
                    .GroupBy(e => e.DepartmentId!.Value)
                    .Select(g => new { g.Key, C = g.Count() })
                    .ToListAsync();
                ViewBag.DeptCounts = counts.ToDictionary(x => x.Key, x => x.C);

                return View("DepartmentPicker", departments);
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null) return NotFound();

            var employees = await _context.Employees
                .Where(e => e.DepartmentId == id && e.IsActive == true)
                .Include(e => e.Position)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            ViewBag.DepartmentName = department.DepartmentName;
            ViewBag.DepartmentId = id;

            return View(employees);
        }

        // -------------------- MANAGER'S TEAM VIEW --------------------
        public async Task<IActionResult> TeamView(int? id)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentUser = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            // If no ID provided and user is a manager, use their own ID
            if (!id.HasValue && User.IsInRole("LineManager") && currentUser != null)
            {
                id = currentUser.EmployeeId;
            }

            if (id == null) return NotFound();

            var manager = await _context.Employees
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (manager == null) return NotFound();

            // Check authorization: Managers can only view their own team, Admins can view any team
            if (User.IsInRole("LineManager") && currentUser != null && currentUser.EmployeeId != id.Value)
            {
                if (!User.IsInRole("SystemAdmin") && !User.IsInRole("HRAdmin"))
                {
                    return Forbid();
                }
            }

            // Get all employees reporting to this manager
            var teamMembers = await _context.Employees
                .Where(e => e.ManagerId == id && e.IsActive == true)
                .Include(e => e.Position)
                .Include(e => e.Department)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            ViewBag.ManagerName = manager.FullName;
            ViewBag.ManagerPosition = manager.Position?.PositionTitle;
            ViewBag.ManagerId = id;

            return View(teamMembers);
        }

        // -------------------- SEARCH EMPLOYEES --------------------
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.IsActive == true &&
                           (e.FullName.Contains(searchTerm) ||
                            e.Email.Contains(searchTerm) ||
                            e.EmployeeCode.Contains(searchTerm)))
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            return View("Index", employees);
        }
    }
}