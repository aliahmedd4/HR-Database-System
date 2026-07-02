using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;

namespace Milstone3_WebApp.Services
{
    public class NotificationService
    {
        private readonly HrPayrollSystemContext _context;

        public NotificationService(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // Create notification for contract expiration
        public async Task CreateContractExpirationNotification(int employeeId, DateOnly expirationDate)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var daysUntilExpiration = expirationDate.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;

            var notification = new Notification
            {
                Title = "Contract Expiration Notice",
                MessageContent = $"Your contract will expire on {expirationDate:yyyy-MM-dd}. Please contact HR to renew your contract. ({daysUntilExpiration} days remaining)",
                NotificationType = "ContractExpiration",
                Priority = daysUntilExpiration <= 30 ? "High" : "Normal",
                Urgency = daysUntilExpiration <= 30 ? "Urgent" : "Normal",
                CreatedAt = DateTime.Now,
                ExpiresAt = expirationDate.ToDateTime(TimeOnly.MinValue)
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var employeeNotification = new EmployeeNotification
            {
                NotificationId = notification.NotificationId,
                EmployeeId = employeeId,
                IsRead = false,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.Now
            };

            _context.EmployeeNotifications.Add(employeeNotification);
            await _context.SaveChangesAsync();
        }

        // Create notification for leave approval
        public async Task CreateLeaveApprovalNotification(int employeeId, string status, string leaveType, DateOnly startDate, DateOnly endDate)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var notification = new Notification
            {
                Title = $"Leave Request {status}",
                MessageContent = $"Your leave request for {leaveType} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} has been {status.ToLower()}.",
                NotificationType = "LeaveApproval",
                Priority = status == "Approved" ? "Normal" : "High",
                Urgency = status == "Rejected" ? "Normal" : "Low",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var employeeNotification = new EmployeeNotification
            {
                NotificationId = notification.NotificationId,
                EmployeeId = employeeId,
                IsRead = false,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.Now
            };

            _context.EmployeeNotifications.Add(employeeNotification);
            await _context.SaveChangesAsync();
        }

        // Create notification for shift reassignment
        public async Task CreateShiftReassignmentNotification(int employeeId, string shiftName, DateOnly startDate)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var notification = new Notification
            {
                Title = "Shift Reassignment",
                MessageContent = $"You have been reassigned to {shiftName} shift starting from {startDate:yyyy-MM-dd}. Please check your schedule.",
                NotificationType = "ShiftReassignment",
                Priority = "Normal",
                Urgency = "Normal",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var employeeNotification = new EmployeeNotification
            {
                NotificationId = notification.NotificationId,
                EmployeeId = employeeId,
                IsRead = false,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.Now
            };

            _context.EmployeeNotifications.Add(employeeNotification);
            await _context.SaveChangesAsync();
        }

        // Create notification for mission update
        public async Task CreateMissionUpdateNotification(int employeeId, string missionName, string status, string? message = null)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var notification = new Notification
            {
                Title = $"Mission Update: {missionName}",
                MessageContent = message ?? $"Your mission '{missionName}' status has been updated to {status}.",
                NotificationType = "MissionUpdate",
                Priority = status == "Approved" || status == "Completed" ? "Normal" : "High",
                Urgency = status == "Rejected" ? "Normal" : "Low",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var employeeNotification = new EmployeeNotification
            {
                NotificationId = notification.NotificationId,
                EmployeeId = employeeId,
                IsRead = false,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.Now
            };

            _context.EmployeeNotifications.Add(employeeNotification);
            await _context.SaveChangesAsync();
        }

        // Check for expiring contracts and create notifications
        public async Task CheckAndNotifyExpiringContracts(int daysBeforeExpiration = 30)
        {
            var expirationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(daysBeforeExpiration));
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            // Get expiring contracts by joining with Employees
            var expiringContracts = await _context.Contracts
                .Join(_context.Employees,
                    contract => contract.EmployeeId,
                    employee => employee.EmployeeId,
                    (contract, employee) => new { Contract = contract, Employee = employee })
                .Where(x => x.Contract.EndDate.HasValue && 
                           x.Contract.EndDate.Value <= expirationDate && 
                           x.Contract.EndDate.Value >= today &&
                           x.Employee.IsActive == true)
                .Select(x => x.Contract)
                .ToListAsync();

            foreach (var contract in expiringContracts)
            {
                if (contract.EndDate.HasValue)
                {
                    // Check if notification already exists for this contract expiration
                    var existingNotification = await _context.EmployeeNotifications
                        .Include(en => en.Notification)
                        .Where(en => en.EmployeeId == contract.EmployeeId &&
                                    en.Notification.NotificationType == "ContractExpiration" &&
                                    en.Notification.MessageContent.Contains(contract.EndDate.Value.ToString("yyyy-MM-dd")))
                        .FirstOrDefaultAsync();

                    if (existingNotification == null)
                    {
                        await CreateContractExpirationNotification(contract.EmployeeId, contract.EndDate.Value);
                    }
                }
            }
        }

        // Create notification for contract creation
        public async Task CreateContractCreatedNotification(int employeeId, string contractType, DateOnly startDate, DateOnly? endDate)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var endDateText = endDate.HasValue ? $" until {endDate.Value:yyyy-MM-dd}" : " (indefinite)";
            var notification = new Notification
            {
                Title = "New Contract Created",
                MessageContent = $"A new {contractType} contract has been created for you, starting from {startDate:yyyy-MM-dd}{endDateText}. Please review the contract details.",
                NotificationType = "ContractCreated",
                Priority = "Normal",
                Urgency = "Normal",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var employeeNotification = new EmployeeNotification
            {
                NotificationId = notification.NotificationId,
                EmployeeId = employeeId,
                IsRead = false,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.Now
            };

            _context.EmployeeNotifications.Add(employeeNotification);
            await _context.SaveChangesAsync();
        }

        // Create notification for contract update
        public async Task CreateContractUpdatedNotification(int employeeId, string contractType, DateOnly startDate, DateOnly? endDate)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var endDateText = endDate.HasValue ? $" until {endDate.Value:yyyy-MM-dd}" : " (indefinite)";
            var notification = new Notification
            {
                Title = "Contract Updated",
                MessageContent = $"Your {contractType} contract has been updated. New start date: {startDate:yyyy-MM-dd}{endDateText}. Please review the updated contract details.",
                NotificationType = "ContractUpdated",
                Priority = "Normal",
                Urgency = "Normal",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var employeeNotification = new EmployeeNotification
            {
                NotificationId = notification.NotificationId,
                EmployeeId = employeeId,
                IsRead = false,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.Now
            };

            _context.EmployeeNotifications.Add(employeeNotification);
            await _context.SaveChangesAsync();
        }

        // Create notification for contract renewal
        public async Task CreateContractRenewedNotification(int employeeId, string contractType, DateOnly newStartDate, DateOnly? newEndDate, DateOnly? oldEndDate)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var oldEndDateText = oldEndDate.HasValue ? $" (previously expiring on {oldEndDate.Value:yyyy-MM-dd})" : "";
            var newEndDateText = newEndDate.HasValue ? $" until {newEndDate.Value:yyyy-MM-dd}" : " (indefinite)";
            var notification = new Notification
            {
                Title = "Contract Renewed",
                MessageContent = $"Your {contractType} contract has been renewed{oldEndDateText}. New contract period: {newStartDate:yyyy-MM-dd}{newEndDateText}. Please review the renewed contract details.",
                NotificationType = "ContractRenewed",
                Priority = "Normal",
                Urgency = "Normal",
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var employeeNotification = new EmployeeNotification
            {
                NotificationId = notification.NotificationId,
                EmployeeId = employeeId,
                IsRead = false,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.Now
            };

            _context.EmployeeNotifications.Add(employeeNotification);
            await _context.SaveChangesAsync();
        }

        // Create notification for HR admins when a leave is flagged as irregular
        public async Task NotifyHRAdminIrregularLeave(int leaveRequestId, int employeeId, string employeeName, string leaveType, DateOnly startDate, DateOnly endDate, int managerId, string managerName)
        {
            // Get all HR Admin and System Admin employees by querying through EmployeeRoles
            var hrAdminEmployeeIds = await _context.EmployeeRoles
                .Include(er => er.Role)
                .Include(er => er.Employee)
                .Where(er => er.IsActive == true && 
                            er.Employee.IsActive == true &&
                            (er.Role.RoleName == "HRAdmin" || er.Role.RoleName == "SystemAdmin"))
                .Select(er => er.EmployeeId)
                .Distinct()
                .ToListAsync();

            if (!hrAdminEmployeeIds.Any()) return;

            foreach (var hrAdminId in hrAdminEmployeeIds)
            {
                var notification = new Notification
                {
                    Title = "Irregular Leave Pattern Flagged",
                    MessageContent = $"Manager {managerName} has flagged an irregular leave request. Employee: {employeeName}, Leave Type: {leaveType}, Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}. Leave Request ID: {leaveRequestId}",
                    NotificationType = "IrregularLeave",
                    Priority = "High",
                    Urgency = "High",
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var employeeNotification = new EmployeeNotification
                {
                    NotificationId = notification.NotificationId,
                    EmployeeId = hrAdminId,
                    IsRead = false,
                    DeliveryStatus = "Delivered",
                    DeliveredAt = DateTime.Now
                };

                _context.EmployeeNotifications.Add(employeeNotification);
            }

            await _context.SaveChangesAsync();
        }
    }
}

