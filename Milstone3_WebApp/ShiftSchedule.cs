using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ShiftSchedule
{
    public int ShiftId { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int? BreakDuration { get; set; }

    public DateOnly? ShiftDate { get; set; }

    public string? Status { get; set; }

    public bool? IsActive { get; set; }

    public int? GracePeriodMinutes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();

    public virtual ICollection<ShiftCycleAssignment> ShiftCycleAssignments { get; set; } = new List<ShiftCycleAssignment>();
}
