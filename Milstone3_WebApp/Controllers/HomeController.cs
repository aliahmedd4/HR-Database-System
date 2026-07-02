using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using Milstone3_WebApp.Models;
using Milstone3_WebApp.Services;

namespace Milstone3_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HrPayrollSystemContext _context;
        private readonly NotificationService _notificationService;

        public HomeController(HrPayrollSystemContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // -------------------- DASHBOARD --------------------
        public async Task<IActionResult> Index()
        {
            // Check for expiring contracts and send notifications (runs on dashboard load)
            try
            {
                await _notificationService.CheckAndNotifyExpiringContracts(30);
            }
            catch
            {
                // Silently fail - don't break dashboard if notification check fails
            }

            var recentNotifications = new List<Notification>();
            return View(recentNotifications);
        }

        // -------------------- PRIVACY --------------------
        public IActionResult Privacy()
        {
            return View();
        }

        // -------------------- ERROR --------------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
