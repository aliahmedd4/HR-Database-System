using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using System.Linq;
using System.Threading.Tasks;

namespace Milstone3_WebApp.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly HrPayrollSystemContext _context;

        public AnalyticsController(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // -------------------- DEPARTMENT-WISE EMPLOYEE STATISTICS --------------------
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> DepartmentStats()
        {
            // Materialize employees first to avoid EF Core translation issues with collection subqueries
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.IsActive == true)
                .ToListAsync();

            var stats = employees
                .GroupBy(e => new {
                    e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : "Unassigned"
                })
                .Select(g => new
                {
                    DepartmentId = g.Key.DepartmentId,
                    DepartmentName = g.Key.DepartmentName,
                    EmployeeCount = g.Count(),
                    ActiveEmployees = g.Count(e => e.IsActive == true),
                    AverageSalary = g.Average(e => e.BaseSalary ?? 0),
                    Positions = g.Select(e => e.Position != null ? e.Position.PositionTitle : "N/A")
                                 .Distinct()
                                 .ToList()
                })
                .OrderByDescending(s => s.EmployeeCount)
                .ToList();

            ViewBag.TotalEmployees = employees.Count;
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();

            return View(stats);
        }

        // -------------------- COMPLIANCE REPORT --------------------
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> ComplianceReport(string? searchTerm, string? reportType)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.FullName.Contains(searchTerm) ||
                    e.CountryOfBirth != null && e.CountryOfBirth.Contains(searchTerm) ||
                    e.Department != null && e.Department.DepartmentName.Contains(searchTerm) ||
                    e.Email.Contains(searchTerm));
            }

            var report = await query
                .Select(e => new
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Email = e.Email,
                    Nationality = e.CountryOfBirth ?? "Not Specified",
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : "Unassigned",
                    Position = e.Position != null ? e.Position.PositionTitle : "N/A",
                    HireDate = e.HireDate,
                    EmploymentStatus = e.EmploymentStatus ?? "Active"
                })
                .ToListAsync();

            // Group by nationality for diversity report
            var diversityStats = report
                .GroupBy(e => e.Nationality)
                .Select(g => new
                {
                    Nationality = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / report.Count * 100
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            ViewBag.ReportType = reportType ?? "Compliance";
            ViewBag.SearchTerm = searchTerm;
            ViewBag.DiversityStats = diversityStats;
            ViewBag.TotalEmployees = report.Count;

            return View(report);
        }

        // -------------------- DIVERSITY REPORT --------------------
        [Authorize(Roles = "HRAdmin,SystemAdmin")]
        public async Task<IActionResult> DiversityReport(string? searchTerm, string? filterBy)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Where(e => e.IsActive == true)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.FullName.Contains(searchTerm) ||
                    (e.CountryOfBirth != null && e.CountryOfBirth.Contains(searchTerm)) ||
                    (e.Department != null && e.Department.DepartmentName.Contains(searchTerm)) ||
                    e.Email.Contains(searchTerm));
            }

            // Apply additional filter if provided
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(searchTerm))
            {
                switch (filterBy.ToLower())
                {
                    case "country":
                        query = query.Where(e => e.CountryOfBirth != null && e.CountryOfBirth.Contains(searchTerm));
                        break;
                    case "department":
                        query = query.Where(e => e.Department != null && e.Department.DepartmentName.Contains(searchTerm));
                        break;
                    case "name":
                        query = query.Where(e => e.FullName.Contains(searchTerm));
                        break;
                }
            }

            var employees = await query.ToListAsync();

            var diversityByCountry = employees
                .Where(e => !string.IsNullOrEmpty(e.CountryOfBirth))
                .GroupBy(e => e.CountryOfBirth)
                .Select(g => new
                {
                    Country = g.Key,
                    Count = g.Count(),
                    Percentage = employees.Count > 0 ? (double)g.Count() / employees.Count * 100 : 0,
                    Departments = g.Select(e => e.Department != null ? e.Department.DepartmentName : "Unassigned").Distinct().ToList()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var diversityByDepartment = employees
                .GroupBy(e => e.Department != null ? e.Department.DepartmentName : "Unassigned")
                .Select(g => new
                {
                    Department = g.Key,
                    TotalEmployees = g.Count(),
                    Countries = g.Where(e => !string.IsNullOrEmpty(e.CountryOfBirth))
                                 .Select(e => e.CountryOfBirth)
                                 .Distinct()
                                 .Count()
                })
                .OrderByDescending(g => g.TotalEmployees)
                .ToList();

            ViewBag.DiversityByCountry = diversityByCountry;
            ViewBag.DiversityByDepartment = diversityByDepartment;
            ViewBag.TotalEmployees = employees.Count;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.FilterBy = filterBy;

            return View();
        }

        // -------------------- EMPLOYEE PROFILE ACCESS --------------------
        [Authorize]
        public async Task<IActionResult> Profile(int? id)
        {
            // If no ID provided, show current user's profile
            if (id == null)
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var currentEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == userEmail);

                if (currentEmployee == null)
                {
                    return NotFound("Employee profile not found.");
                }

                id = currentEmployee.EmployeeId;
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .Include(e => e.Contract)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            // Check if user has permission to view this profile
            var userEmail2 = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentUser = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail2);

            // Users can only view their own profile unless they're HR Admin or System Admin
            if (currentUser != null &&
                currentUser.EmployeeId != employee.EmployeeId &&
                !User.IsInRole("HRAdmin") &&
                !User.IsInRole("SystemAdmin"))
            {
                return Forbid();
            }

            return View(employee);
        }
    }
}
