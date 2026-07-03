using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using Milstone3_WebApp.Models;
using Milstone3_WebApp.Services;

namespace Milstone3_WebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HrPayrollSystemContext _context;
        private readonly NotificationService _notificationService;

        public HomeController(HrPayrollSystemContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            // Background: notify about expiring contracts
            try { await _notificationService.CheckAndNotifyExpiringContracts(30); } catch { }

            var role      = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var empIdStr  = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(empIdStr, out int empId);
            var today     = DateOnly.FromDateTime(DateTime.Today);
            bool isAdmin  = role is "SystemAdmin" or "HRAdmin";
            bool isMgr    = isAdmin || role == "LineManager";

            // ── KPI stats ──
            if (isMgr)
            {
                ViewBag.TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive == true);

                ViewBag.PresentToday = await _context.Attendances
                    .CountAsync(a => a.AttendanceDate == today && a.Status == "Present");

                ViewBag.OnLeaveToday = await _context.Attendances
                    .CountAsync(a => a.AttendanceDate == today && a.Status == "OnLeave");

                ViewBag.PendingLeaves = await _context.LeaveRequests
                    .CountAsync(lr => lr.Status == "Pending");

                // Missions use "Assigned" as the awaiting-approval state
                // (the schema CHECK constraint does not permit "Pending").
                ViewBag.PendingMissions = await _context.Missions
                    .CountAsync(m => m.Status == "Assigned" || m.Status == "Pending");
            }

            if (isAdmin)
            {
                var thirtyDaysOut = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
                ViewBag.ExpiringContracts = await _context.Contracts
                    .CountAsync(c => c.EndDate <= thirtyDaysOut && c.EndDate >= today && c.CurrentState == "Active");
            }

            if (role == "Employee" && empId > 0)
            {
                var currentYear = DateTime.Now.Year;
                ViewBag.MyLeaveBalance = await _context.LeaveEntitlements
                    .Where(le => le.EmployeeId == empId && le.Year == currentYear)
                    .SumAsync(le => (decimal?)(le.BalanceDays ?? 0)) ?? 0;

                ViewBag.MyMissions = await _context.Missions
                    .CountAsync(m => m.EmployeeId == empId);

                ViewBag.PendingLeaves = await _context.LeaveRequests
                    .CountAsync(lr => lr.EmployeeId == empId && lr.Status == "Pending");
            }

            // ── Unread notifications (topbar badge) ──
            if (empId > 0)
            {
                ViewBag.UnreadNotifications = await _context.EmployeeNotifications
                    .CountAsync(en => en.EmployeeId == empId && en.IsRead == false);
            }

            // ── Recent notifications feed ──
            if (empId > 0)
            {
                var recent = await _context.EmployeeNotifications
                    .Include(en => en.Notification)
                    .Where(en => en.EmployeeId == empId)
                    .OrderByDescending(en => en.Notification.CreatedAt)
                    .Take(6)
                    .Select(en => en.Notification)
                    .ToListAsync();
                ViewBag.RecentNotifications = recent;
            }

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
