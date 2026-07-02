using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class FullTimeContract
{
    public int ContractId { get; set; }

    public string? LeaveEntitlement { get; set; }

    public string? InsuranceEligibility { get; set; }

    public int? WeeklyWorkingHours { get; set; }

    public string? Benefits { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
