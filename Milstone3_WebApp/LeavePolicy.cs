using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class LeavePolicy
{
    public int PolicyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Purpose { get; set; }

    public TimeOnly? NoticePeriod { get; set; }

    public string? SpecialLeaveType { get; set; }

    public string? EligibilityRules { get; set; }

    public bool? ResetOnNewYear { get; set; }

    public int LeaveId { get; set; }

    public int? EligibilityMonths { get; set; }

    public decimal? AccrualRate { get; set; }

    public decimal? MaxBalance { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public virtual Leave Leave { get; set; } = null!;
}
