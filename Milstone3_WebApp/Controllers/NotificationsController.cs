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
    public class NotificationsController : Controller
    {
        private readonly HrPayrollSystemContext _context;

        public NotificationsController(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // -------------------- LIST ALL NOTIFICATIONS (Admin View) --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var notifications = await _context.Notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                return View(notifications);
            }
            catch
            {
                // If there's an error, return empty list
                return View(new List<Notification>());
            }
        }

        // -------------------- VIEW MY NOTIFICATIONS (Employee) --------------------
        [Authorize]
        public async Task<IActionResult> MyNotifications()
        {
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

            try
            {
                var myNotifications = await _context.EmployeeNotifications
                    .Include(en => en.Notification)
                    .Where(en => en.EmployeeId == employee.EmployeeId)
                    .OrderByDescending(en => en.Notification.CreatedAt)
                    .Select(en => new
                    {
                        NotificationId = en.NotificationId,
                        Title = en.Notification.Title ?? "No Title",
                        Message = en.Notification.MessageContent ?? "No Message",
                        Priority = en.Notification.Priority ?? "Normal",
                        NotificationType = en.Notification.NotificationType ?? "General",
                        CreatedAt = en.Notification.CreatedAt,
                        IsRead = en.IsRead,
                        EmployeeNotificationId = en.EmployeeNotificationId
                    })
                    .ToListAsync();

                ViewBag.EmployeeName = employee.FullName;
                ViewBag.UnreadCount = myNotifications.Count(n => n.IsRead == false);

                return View(myNotifications);
            }
            catch
            {
                ViewBag.EmployeeName = employee.FullName;
                ViewBag.UnreadCount = 0;
                return View(new List<dynamic>());
            }
        }

        // -------------------- MARK NOTIFICATION AS READ --------------------
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var employeeNotification = await _context.EmployeeNotifications
                .FirstOrDefaultAsync(en => en.EmployeeNotificationId == id);

            if (employeeNotification != null)
            {
                employeeNotification.IsRead = true;
                employeeNotification.DeliveredAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyNotifications));
        }

        // -------------------- MARK ALL AS READ --------------------
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee != null)
            {
                var unreadNotifications = await _context.EmployeeNotifications
                    .Where(en => en.EmployeeId == employee.EmployeeId && en.IsRead == false)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.DeliveredAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "All notifications marked as read.";
            }

            return RedirectToAction(nameof(MyNotifications));
        }

        // -------------------- NOTIFICATION DETAILS --------------------
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.NotificationId == id);

            if (notification == null) return NotFound();
            return View(notification);
        }

        // -------------------- CREATE NOTIFICATION --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Get current user's employee info
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            // For Line Managers, get their team members
            if (User.IsInRole("LineManager") && employee != null)
            {
                var teamMembers = await _context.Employees
                    .Where(e => e.ManagerId == employee.EmployeeId && e.IsActive == true)
                    .Select(e => new { e.EmployeeId, e.FullName, e.Email })
                    .ToListAsync();

                ViewBag.TeamMembers = teamMembers;
            }
            else if (User.IsInRole("HRAdmin") || User.IsInRole("SystemAdmin"))
            {
                // HR Admin can send to all employees
                var allEmployees = await _context.Employees
                    .Where(e => e.IsActive == true)
                    .Select(e => new { e.EmployeeId, e.FullName, e.Email })
                    .ToListAsync();

                ViewBag.AllEmployees = allEmployees;
            }

            return View();
        }

        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string messageContent, string priority, string notificationType, int[] selectedEmployees)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(messageContent))
            {
                TempData["ErrorMessage"] = "Title and message are required.";
                return RedirectToAction(nameof(Create));
            }

            if (selectedEmployees == null || selectedEmployees.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one employee.";
                return RedirectToAction(nameof(Create));
            }

            try
            {
                // Create the notification
                var notification = new Notification
                {
                    Title = title,
                    MessageContent = messageContent,
                    Priority = priority ?? "Normal",
                    NotificationType = notificationType ?? "General",
                    Urgency = priority == "Urgent" ? "Urgent" : (priority == "High" ? "High" : "Normal"),
                    Timestamp = DateTime.Now,
                    ReadStatus = "Unread",
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Assign to selected employees
                foreach (var employeeId in selectedEmployees)
                {
                    var employeeNotification = new EmployeeNotification
                    {
                        NotificationId = notification.NotificationId,
                        EmployeeId = employeeId,
                        IsRead = false,
                        DeliveryStatus = "Delivered",
                        DeliveredAt = DateTime.Now
                    };
                    _context.EmployeeNotifications.Add(employeeNotification);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Notification sent to {selectedEmployees.Length} employee(s) successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating notification: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        // -------------------- EDIT NOTIFICATION --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            return View(notification);
        }

        [Authorize(Roles = "SystemAdmin, HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Notification notification)
        {
            if (id != notification.NotificationId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(notification);
            }

            try
            {
                _context.Update(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Notification updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(notification.NotificationId))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error editing notification: {ex.Message}");
                return View(notification);
            }
        }

        // -------------------- DELETE NOTIFICATION --------------------
        [Authorize(Roles = "SystemAdmin, HRAdmin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.NotificationId == id);

            if (notification == null) return NotFound();
            return View(notification);
        }

        [Authorize(Roles = "SystemAdmin, HRAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                }
                TempData["SuccessMessage"] = "Notification deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting notification: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "SystemAdmin, HRAdmin, LineManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id, int[] selectedEmployees)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                TempData["ErrorMessage"] = "Notification not found.";
                return RedirectToAction(nameof(Index));
            }

            if (selectedEmployees == null || selectedEmployees.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one employee.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                foreach (var employeeId in selectedEmployees)
                {
                    var employeeNotification = new EmployeeNotification
                    {
                        NotificationId = notification.NotificationId,
                        EmployeeId = employeeId,
                        IsRead = false,
                        DeliveryStatus = "Delivered",
                        DeliveredAt = DateTime.Now
                    };
                    _context.EmployeeNotifications.Add(employeeNotification);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Notification sent to {selectedEmployees.Length} employee(s).";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error sending notification: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.NotificationId == id);
        }
    }
}