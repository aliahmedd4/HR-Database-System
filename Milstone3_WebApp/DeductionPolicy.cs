using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class DeductionPolicy
{
    public int PolicyId { get; set; }

    public string? DeductionReason { get; set; }

    public string? CalculationMode { get; set; }

    public decimal? DeductionAmount { get; set; }

    public decimal? DeductionPercentage { get; set; }

    public bool? AppliesToAll { get; set; }

    public virtual PayrollPolicy Policy { get; set; } = null!;
}
