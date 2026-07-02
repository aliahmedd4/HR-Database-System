using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class OvertimePolicy
{
    public int PolicyId { get; set; }

    public decimal? WeekdayRateMultiplier { get; set; }

    public decimal? WeekdendRateMultiplier { get; set; }

    public decimal? MinHoursForOvertime { get; set; }

    public decimal? MaxHoursPerMonth { get; set; }

    public virtual PayrollPolicy Policy { get; set; } = null!;
}
