using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class LatenessPolicy
{
    public int PolicyId { get; set; }

    public decimal? GracePeriodMins { get; set; }

    public decimal? DeductionRate { get; set; }

    public decimal? PenaltyPerLateMinute { get; set; }

    public int? ThresholdMinutes { get; set; }

    public decimal? MaxPenaltyPerDay { get; set; }

    public virtual PayrollPolicy Policy { get; set; } = null!;
}
