using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class EmployeeNotification
{
    public int EmployeeNotificationId { get; set; }

    public int NotificationId { get; set; }

    public int EmployeeId { get; set; }

    public bool? IsRead { get; set; }

    public string? DeliveryStatus { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Notification Notification { get; set; } = null!;
}
