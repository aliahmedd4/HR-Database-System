using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Milstone3_WebApp;

namespace Milstone3_WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly HrPayrollSystemContext _context;

        public AccountController(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string role, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Please enter email, password, and select a role.";
                return View();
            }

            // Role-specific passwords
            string expectedPassword = role switch
            {
                "SystemAdmin" => "systemadmin123",
                "HRAdmin" => "admin123",
                "LineManager" => "manager123",
                "Employee" => "employee123",
                _ => ""
            };

            if (password != expectedPassword)
            {
                ViewBag.Error = $"Invalid password for selected role.";
                return View();
            }

            // Find employee by email
            var employee = await _context.Employees
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Email == email && e.IsActive == true);

            if (employee == null)
            {
                ViewBag.Error = "Email not found. Please create an account first.";
                return View();
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employee.FullName),
                new Claim(ClaimTypes.Email, employee.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["SuccessMessage"] = $"Welcome back, {employee.FullName} ({role})!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        // Only System Admins, HR Admins, and Line Managers can create their personal accounts
        [HttpGet]
        [Authorize(Roles = "SystemAdmin,HRAdmin,LineManager")]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        // Only System Admins, HR Admins, and Line Managers can create their personal accounts
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,HRAdmin,LineManager")]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string phone, string role)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }

            // Check if email already exists
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);

            if (existingEmployee != null)
            {
                ViewBag.Error = "This email is already registered. Please login instead.";
                return View();
            }

            // Generate employee code
            var lastEmployee = await _context.Employees
                .OrderByDescending(e => e.EmployeeId)
                .FirstOrDefaultAsync();

            int nextId = (lastEmployee?.EmployeeId ?? 0) + 1;
            string employeeCode = $"EMP{nextId:D3}";

            // Create new employee
            var newEmployee = new Employee
            {
                EmployeeCode = employeeCode,
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Email = email,
                Phone = phone,
                NationalId = $"NID{nextId:D6}", // Add this line - generates unique national_id
                HireDate = DateOnly.FromDateTime(DateTime.Now),
                IsActive = true,
                ProfileCompletion = false,
                DepartmentId = 1,
                PositionId = 3,
                PayGradeId = 1,
                SalaryTypeId = 1,
                CurrencyId = 1,
                BaseSalary = 50000.00m
            };

            _context.Employees.Add(newEmployee);
            await _context.SaveChangesAsync();

            // Assign role - look up role by name
            var roleEntity = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == role || r.RoleName == role.Replace(" ", ""));

            if (roleEntity == null)
            {
                // Fallback to role ID mapping if role name lookup fails
                var roleId = role switch
                {
                    "Employee" => 3,
                    "LineManager" => 2,
                    "HRAdmin" => 1,
                    "SystemAdmin" => 7,
                    "Executive" => 4,
                    "PayrollFinance" => 5,
                    "Recruiter" => 6,
                    _ => 3
                };
                roleEntity = await _context.Roles.FindAsync(roleId);
            }

            if (roleEntity != null)
            {
                var employeeRole = new EmployeeRole
                {
                    EmployeeId = newEmployee.EmployeeId,
                    RoleId = roleEntity.RoleId,
                    IsActive = true,
                    AssignedDate = DateTime.Now
                };

                _context.EmployeeRoles.Add(employeeRole);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Account created successfully! You can now login.";
            return RedirectToAction("Login");
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Account/CreateEmployeeAccount
        // Only System Admins can create accounts for new employees
        [HttpGet]
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult CreateEmployeeAccount()
        {
            ViewData["DepartmentId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["PositionId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Positions, "PositionId", "PositionTitle");
            ViewData["ManagerId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FullName");

            // Get distinct contracts
            var distinctContracts = _context.Contracts
                .GroupBy(c => c.ContractType)
                .Select(g => g.First())
                .ToList();
            ViewData["ContractId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(distinctContracts, "ContractId", "ContractType");

            return View();
        }

        // POST: Account/CreateEmployeeAccount
        // Only System Admins can create accounts for new employees
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> CreateEmployeeAccount(
            string firstName, string lastName, string email, string phone, string role,
            string nationalId, DateOnly? dateOfBirth, string countryOfBirth,
            int? departmentId, int? positionId, int? managerId, int? contractId,
            decimal? baseSalary, string address, string emergencyContactName,
            string emergencyContactPhone, string relationship)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Please fill in all required fields (First Name, Last Name, Email, Role).";
                RepopulateDropdowns();
                return View();
            }

            // Check if email already exists
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);

            if (existingEmployee != null)
            {
                ViewBag.Error = "This email is already registered. Please use a different email.";
                RepopulateDropdowns();
                return View();
            }

            // Generate employee code
            var lastEmployee = await _context.Employees
                .OrderByDescending(e => e.EmployeeId)
                .FirstOrDefaultAsync();

            int nextId = (lastEmployee?.EmployeeId ?? 0) + 1;
            string employeeCode = $"EMP{nextId:D3}";

            // Create new employee with all provided details
            var newEmployee = new Employee
            {
                EmployeeCode = employeeCode,
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Email = email,
                Phone = phone,
                NationalId = nationalId ?? $"NID{nextId:D6}",
                DateOfBirth = dateOfBirth,
                CountryOfBirth = countryOfBirth,
                HireDate = DateOnly.FromDateTime(DateTime.Now),
                DepartmentId = departmentId,
                PositionId = positionId,
                ManagerId = managerId,
                ContractId = contractId,
                BaseSalary = baseSalary ?? 50000.00m,
                Address = address,
                EmergencyContactName = emergencyContactName,
                EmergencyContactPhone = emergencyContactPhone,
                Relationship = relationship,
                IsActive = true,
                ProfileCompletion = false,
                AccountStatus = "Active",
                EmploymentStatus = "Active",
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.Employees.Add(newEmployee);
                await _context.SaveChangesAsync();

                // Assign role - look up role by name
                var roleEntity = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == role || r.RoleName == role.Replace(" ", ""));

                if (roleEntity == null)
                {
                    // Fallback to role ID mapping if role name lookup fails
                    var roleId = role switch
                    {
                        "Employee" => 3,
                        "LineManager" => 2,
                        "HRAdmin" => 1,
                        "SystemAdmin" => 7,
                        "Executive" => 4,
                        "PayrollFinance" => 5,
                        "Recruiter" => 6,
                        _ => 3
                    };
                    roleEntity = await _context.Roles.FindAsync(roleId);
                }

                if (roleEntity != null)
                {
                    var employeeRole = new EmployeeRole
                    {
                        EmployeeId = newEmployee.EmployeeId,
                        RoleId = roleEntity.RoleId,
                        IsActive = true,
                        AssignedDate = DateTime.Now
                    };

                    _context.EmployeeRoles.Add(employeeRole);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Account created successfully for {newEmployee.FullName}! They can now login.";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error creating account: {ex.Message}";
                RepopulateDropdowns();
                return View();
            }
        }

        private void RepopulateDropdowns()
        {
            ViewData["DepartmentId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["PositionId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Positions, "PositionId", "PositionTitle");
            ViewData["ManagerId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FullName");

            var distinctContracts = _context.Contracts
                .GroupBy(c => c.ContractType)
                .Select(g => g.First())
                .ToList();
            ViewData["ContractId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(distinctContracts, "ContractId", "ContractType");
        }
    }
}