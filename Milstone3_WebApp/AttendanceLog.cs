using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class AttendanceLog
{
    public int AttendanceLogId { get; set; }

    public int? AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public string? ActionType { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ChangedBy { get; set; }

    public string? Reason { get; set; }

    public string? Actor { get; set; }

    public TimeOnly? TimeStamp { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual Attendance? Attendance { get; set; }

    public virtual Employee? ChangedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
