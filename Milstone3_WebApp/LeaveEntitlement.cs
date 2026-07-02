using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class LeaveEntitlement
{
    public int EntitlementId { get; set; }

    public string? Entitlement { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveId { get; set; }

    public int Year { get; set; }

    public decimal? AllocatedDays { get; set; }

    public decimal? UsedDays { get; set; }

    public decimal? BalanceDays { get; set; }

    public decimal? CarryForwardDays { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Leave Leave { get; set; } = null!;
}
