using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class PayrollPolicy
{
    public int PolicyId { get; set; }

    public string? Type { get; set; }

    public string? Description { get; set; }

    public string PolicyName { get; set; } = null!;

    public string? PolicyType { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public virtual BonusPolicy? BonusPolicy { get; set; }

    public virtual DeductionPolicy? DeductionPolicy { get; set; }

    public virtual LatenessPolicy? LatenessPolicy { get; set; }

    public virtual OvertimePolicy? OvertimePolicy { get; set; }
}
