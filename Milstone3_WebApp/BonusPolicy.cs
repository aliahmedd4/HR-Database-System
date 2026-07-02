using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class BonusPolicy
{
    public int PolicyId { get; set; }

    public string? BonusType { get; set; }

    public decimal? BonusAmount { get; set; }

    public decimal? BonusPercentage { get; set; }

    public string? EligibilityCriteria { get; set; }

    public virtual PayrollPolicy Policy { get; set; } = null!;
}
