using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public int? ShiftId { get; set; }

    public DateTime? EntryTime { get; set; }

    public DateTime? ExitTime { get; set; }

    public TimeOnly? Duration { get; set; }

    public string? LoginMethod { get; set; }

    public string? LogoutMethod { get; set; }

    public int? ExceptionId { get; set; }

    public string? Status { get; set; }

    public decimal? HoursWorked { get; set; }

    public decimal? OvertimeHours { get; set; }

    public int? LatenessMinutes { get; set; }

    public string? SourceType { get; set; }

    public string? SourceId { get; set; }

    public int? LeaveRequestId { get; set; }

    public bool? IsLeaveException { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual ShiftSchedule? Shift { get; set; }
}
