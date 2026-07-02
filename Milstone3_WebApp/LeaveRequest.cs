using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class LeaveRequest
{
    public int RequestId { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveId { get; set; }

    public string? Justification { get; set; }

    public decimal? DurationDays { get; set; }

    public DateOnly? ApprovalTiming { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal TotalDays { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public bool? IsIrregular { get; set; }

    public int? LinkedShiftId { get; set; }

    public string? OverrideReason { get; set; }

    public DateTime? OverriddenAt { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Leave Leave { get; set; } = null!;

    public virtual ICollection<LeaveDocument> LeaveDocuments { get; set; } = new List<LeaveDocument>();

    public virtual ShiftSchedule? LinkedShift { get; set; }
}
