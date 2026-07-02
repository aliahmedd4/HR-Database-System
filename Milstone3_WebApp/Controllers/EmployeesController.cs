using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milstone3_WebApp.Controllers
{
    [Authorize] // Require authentication for all employee actions
    public class EmployeesController : Controller
    {
        private readonly HrPayrollSystemContext _context;

        public EmployeesController(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentUser = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            IQueryable<Employee> employeesQuery = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Contract)
                .Where(e => e.IsActive == true);

            // System Admins and HR Admins can view all employees across all departments
            if (User.IsInRole("SystemAdmin") || User.IsInRole("HRAdmin"))
            {
                // Show all employees - no filtering needed
            }
            // Managers can only view their team members
            else if (User.IsInRole("LineManager") && currentUser != null)
            {
                employeesQuery = employeesQuery.Where(e => e.ManagerId == currentUser.EmployeeId);
            }
            // Regular employees can only view themselves
            else if (currentUser != null)
            {
                employeesQuery = employeesQuery.Where(e => e.EmployeeId == currentUser.EmployeeId);
            }
            else
            {
                // If user not found, return empty list
                employeesQuery = employeesQuery.Where(e => false);
            }

            var employees = await employeesQuery
                .OrderBy(e => e.DepartmentId)
                .ThenBy(e => e.FullName)
                .ToListAsync();

            return View(employees);
        }


        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Contract)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }


        // GET: Employees/Create
        // GET: Employees/Create
        public IActionResult Create()
        {
            // Get one contract per type (distinct)
            var distinctContracts = _context.Contracts
                .GroupBy(c => c.ContractType)
                .Select(g => g.First())
                .ToList();

            ViewData["ContractId"] = new SelectList(distinctContracts, "ContractId", "ContractType");
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionTitle");

            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Employee created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractType", employee.ContractId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionTitle", employee.PositionId);


            return View(employee);
        }

        // GET: Employees/Edit/5
        // Only HR Admins can edit any employee profile
        [Authorize(Roles = "HRAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Get one contract per type (distinct) - this shows only 4 options
            var distinctContracts = _context.Contracts
                .GroupBy(c => c.ContractType)
                .Select(g => g.First())
                .ToList();

            ViewData["ContractId"] = new SelectList(distinctContracts, "ContractId", "ContractType", employee.ContractId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionTitle", employee.PositionId);

            return View(employee);
        }

        // POST: Employees/Edit/5
        // Only HR Admins can edit any employee profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HRAdmin")]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Employee updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractType", employee.ContractId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "PositionId", "PositionTitle", employee.PositionId);


            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Contract)
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Employee deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // -------------------- ASSIGN ROLE TO EMPLOYEE (System Admin Only) --------------------
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        public async Task<IActionResult> AssignRole(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            // Get all available roles
            var allRoles = await _context.Roles
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            // Get current roles for the employee
            var currentRoleIds = employee.EmployeeRoles
                .Where(er => er.IsActive == true)
                .Select(er => er.RoleId)
                .ToList();

            ViewBag.Employee = employee;
            ViewBag.AllRoles = allRoles;
            ViewBag.CurrentRoleIds = currentRoleIds;

            return View();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(int employeeId, int[] selectedRoles)
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeRoles)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Deactivate all current roles
                foreach (var existingRole in employee.EmployeeRoles.Where(er => er.IsActive == true))
                {
                    existingRole.IsActive = false;
                }

                // Assign new roles
                if (selectedRoles != null && selectedRoles.Length > 0)
                {
                    foreach (var roleId in selectedRoles)
                    {
                        var existingEmployeeRole = employee.EmployeeRoles
                            .FirstOrDefault(er => er.RoleId == roleId);

                        if (existingEmployeeRole != null)
                        {
                            // Reactivate existing role
                            existingEmployeeRole.IsActive = true;
                            existingEmployeeRole.AssignedDate = DateTime.Now;
                        }
                        else
                        {
                            // Create new role assignment
                            var newEmployeeRole = new EmployeeRole
                            {
                                EmployeeId = employeeId,
                                RoleId = roleId,
                                IsActive = true,
                                AssignedDate = DateTime.Now
                            };
                            _context.EmployeeRoles.Add(newEmployeeRole);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Roles assigned successfully to {employee.FullName}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error assigning roles: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = employeeId });
        }

        // -------------------- MANAGER: VIEW MY TEAM --------------------
        [Authorize(Roles = "LineManager")]
        [HttpGet]
        public async Task<IActionResult> MyTeam()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var manager = await _context.Employees
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (manager == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all employees reporting to this manager
            var teamMembers = await _context.Employees
                .Where(e => e.ManagerId == manager.EmployeeId && e.IsActive == true)
                .Include(e => e.Position)
                .Include(e => e.Department)
                .Include(e => e.Contract)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            ViewBag.ManagerName = manager.FullName;
            ViewBag.ManagerPosition = manager.Position?.PositionTitle;
            ViewBag.ManagerId = manager.EmployeeId;

            return View(teamMembers);
        }

        // -------------------- EMPLOYEE: UPDATE OWN PERSONAL DETAILS AND EMERGENCY CONTACTS --------------------
        [HttpGet]
        public async Task<IActionResult> EditMyProfile()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMyProfile(Employee employee)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (currentEmployee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Ensure employee can only edit their own profile
            if (employee.EmployeeId != currentEmployee.EmployeeId)
            {
                TempData["ErrorMessage"] = "You can only edit your own profile.";
                return RedirectToAction(nameof(Details), new { id = currentEmployee.EmployeeId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Only update allowed fields: personal details and emergency contacts
                    currentEmployee.Phone = employee.Phone;
                    currentEmployee.Address = employee.Address;
                    currentEmployee.EmergencyContactName = employee.EmergencyContactName;
                    currentEmployee.EmergencyContactPhone = employee.EmergencyContactPhone;
                    currentEmployee.Relationship = employee.Relationship;
                    currentEmployee.UpdatedAt = DateTime.Now;

                    _context.Update(currentEmployee);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = currentEmployee.EmployeeId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(employee);
        }

        // -------------------- HR ADMIN: MANAGE PROFILE COMPLETENESS --------------------
        [Authorize(Roles = "HRAdmin")]
        [HttpGet]
        public async Task<IActionResult> ManageProfileCompleteness(int? id)
        {
            if (id == null)
            {
                // Show list of all employees with their completeness status
                var employees = await _context.Employees
                    .Where(e => e.IsActive == true)
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .OrderBy(e => e.FullName)
                    .ToListAsync();

                // Calculate completeness for each employee
                var employeesWithCompleteness = employees.Select(e => new
                {
                    Employee = e,
                    CompletenessPercentage = CalculateProfileCompleteness(e)
                }).ToList();

                ViewBag.EmployeesWithCompleteness = employeesWithCompleteness;
                return View("ManageProfileCompletenessList");
            }
            else
            {
                // Manage completeness for specific employee
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);
                if (employee == null)
                {
                    return NotFound();
                }

                var completenessPercentage = CalculateProfileCompleteness(employee);
                ViewBag.CompletenessPercentage = completenessPercentage;
                return View(employee);
            }
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileCompleteness(int employeeId, bool profileCompletion)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(ManageProfileCompleteness));
            }

            employee.ProfileCompletion = profileCompletion;
            employee.UpdatedAt = DateTime.Now;

            _context.Update(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Profile completeness updated for {employee.FullName}.";
            return RedirectToAction(nameof(ManageProfileCompleteness), new { id = employeeId });
        }

        private int CalculateProfileCompleteness(Employee employee)
        {
            int totalFields = 0;
            int completedFields = 0;

            // Required fields
            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.FirstName)) completedFields++;

            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.LastName)) completedFields++;

            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.Email)) completedFields++;

            totalFields++;
            if (employee.HireDate != default(DateOnly)) completedFields++;

            // Important optional fields
            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.Phone)) completedFields++;

            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.Address)) completedFields++;

            totalFields++;
            if (employee.DateOfBirth.HasValue) completedFields++;

            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.NationalId)) completedFields++;

            totalFields++;
            if (employee.DepartmentId.HasValue) completedFields++;

            totalFields++;
            if (employee.PositionId.HasValue) completedFields++;

            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.EmergencyContactName)) completedFields++;

            totalFields++;
            if (!string.IsNullOrWhiteSpace(employee.EmergencyContactPhone)) completedFields++;

            if (totalFields == 0) return 0;
            return (int)Math.Round((double)completedFields / totalFields * 100);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
