using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
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

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;
            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);
                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, 100_000, HashAlgorithmName.SHA256, 32);
                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
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
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter your email and password.";
                return View();
            }

            // Find employee by email including their assigned roles
            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync(e => e.Email == email && e.IsActive == true);

            if (employee == null)
            {
                ViewBag.Error = "Email not found or account is inactive.";
                return View();
            }

            // Verify per-user password hash
            if (string.IsNullOrEmpty(employee.PasswordHash) || !VerifyPassword(password, employee.PasswordHash))
            {
                ViewBag.Error = "Invalid password.";
                return View();
            }

            // Auto-detect role: pick the most privileged active role
            var rolePriority = new[] { "SystemAdmin", "HRAdmin", "LineManager", "Employee" };
            var role = rolePriority.FirstOrDefault(r =>
                employee.EmployeeRoles.Any(er => er.IsActive == true && er.Role?.RoleName == r));

            if (role == null)
            {
                ViewBag.Error = "No active role assigned to your account. Contact your administrator.";
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

            TempData["SuccessMessage"] = $"Welcome back, {employee.FullName}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
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
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string phone, string role, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            if (password.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters.";
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

            // Insert with a temporary unique code; replace with the DB-assigned ID after insert
            var newEmployee = new Employee
            {
                EmployeeCode = $"TEMP-{Guid.NewGuid():N}",
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Email = email,
                Phone = phone,
                NationalId = $"TEMPNID-{Guid.NewGuid():N}",
                HireDate = DateOnly.FromDateTime(DateTime.Now),
                IsActive = true,
                ProfileCompletion = false,
                DepartmentId = 1,
                PositionId = 3,
                PayGradeId = 1,
                SalaryTypeId = 1,
                CurrencyId = 1,
                BaseSalary = 50000.00m,
                PasswordHash = HashPassword(password)
            };

            _context.Employees.Add(newEmployee);
            await _context.SaveChangesAsync();

            // Now assign the real code using the DB-generated ID (race-condition-free)
            newEmployee.EmployeeCode = $"EMP{newEmployee.EmployeeId:D3}";
            newEmployee.NationalId = $"NID{newEmployee.EmployeeId:D6}";
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

        // ── First-run setup ──────────────────────────────────────────────
        // GET /Account/Setup — only accessible when NO employee has a password hash.
        // Use this once to create the initial SystemAdmin account, then it locks itself.
        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            bool anyPasswordSet = await _context.Employees
                .AnyAsync(e => e.PasswordHash != null);
            if (anyPasswordSet)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(string email, string password, string confirmPassword)
        {
            bool anyPasswordSet = await _context.Employees
                .AnyAsync(e => e.PasswordHash != null);
            if (anyPasswordSet)
                return RedirectToAction("Login");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }
            if (password.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters.";
                return View();
            }

            // Find or create the SystemAdmin account
            var employee = await _context.Employees
                .Include(e => e.EmployeeRoles)
                .FirstOrDefaultAsync(e => e.Email == email);

            if (employee == null)
            {
                ViewBag.Error = "No employee found with that email. Add the employee to the database first, then run setup.";
                return View();
            }

            employee.PasswordHash = HashPassword(password);
            await _context.SaveChangesAsync();

            // Ensure a SystemAdmin role is assigned
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "SystemAdmin");
            if (adminRole != null)
            {
                bool hasAdminRole = employee.EmployeeRoles
                    .Any(er => er.RoleId == adminRole.RoleId && er.IsActive == true);
                if (!hasAdminRole)
                {
                    _context.EmployeeRoles.Add(new EmployeeRole
                    {
                        EmployeeId = employee.EmployeeId,
                        RoleId    = adminRole.RoleId,
                        IsActive  = true,
                        AssignedDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = $"Setup complete. You can now sign in as {email}.";
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
            string emergencyContactPhone, string relationship, string password)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please fill in all required fields (First Name, Last Name, Email, Role, Password).";
                RepopulateDropdowns();
                return View();
            }

            if (password.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters.";
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

            // Insert with a temporary unique code; replace with the DB-assigned ID after insert
            var newEmployee = new Employee
            {
                EmployeeCode = $"TEMP-{Guid.NewGuid():N}",
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Email = email,
                Phone = phone,
                NationalId = !string.IsNullOrEmpty(nationalId) ? nationalId : $"TEMPNID-{Guid.NewGuid():N}",
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
                CreatedAt = DateTime.Now,
                PasswordHash = HashPassword(password)
            };

            try
            {
                _context.Employees.Add(newEmployee);
                await _context.SaveChangesAsync();

                // Assign the real code using the DB-generated ID (race-condition-free)
                newEmployee.EmployeeCode = $"EMP{newEmployee.EmployeeId:D3}";
                if (string.IsNullOrEmpty(nationalId))
                    newEmployee.NationalId = $"NID{newEmployee.EmployeeId:D6}";
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