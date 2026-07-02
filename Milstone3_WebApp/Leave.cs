using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Leave
{
    public int LeaveId { get; set; }

    public string LeaveType { get; set; } = null!;

    public string LeaveDescription { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsPaid { get; set; }

    public int? MaxDaysPerYear { get; set; }

    public bool? RequiresApproval { get; set; }

    public bool? IsActive { get; set; }

    public virtual HolidayLeave? HolidayLeave { get; set; }

    public virtual ICollection<LeavePolicy> LeavePolicies { get; set; } = new List<LeavePolicy>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual ProbationLeave? ProbationLeave { get; set; }

    public virtual SickLeave? SickLeave { get; set; }

    public virtual VacationLeave? VacationLeave { get; set; }
}
