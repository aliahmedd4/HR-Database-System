using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class MonthlySalaryType
{
    public int MonthlySalaryTypeId { get; set; }

    public int SalaryTypeId { get; set; }

    public string? TaxRule { get; set; }

    public string? ContributionScheme { get; set; }

    public virtual SalaryType SalaryType { get; set; } = null!;
}
