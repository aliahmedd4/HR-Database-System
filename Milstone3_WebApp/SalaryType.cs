using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class SalaryType
{
    public int SalarytypeId { get; set; }

    public string Type { get; set; } = null!;

    public string? Description { get; set; }

    public int? PaymentFrequency { get; set; }

    public decimal? Currency { get; set; }

    public virtual ICollection<ContractSalaryType> ContractSalaryTypes { get; set; } = new List<ContractSalaryType>();

    public virtual ICollection<HourlySalaryType> HourlySalaryTypes { get; set; } = new List<HourlySalaryType>();

    public virtual ICollection<MonthlySalaryType> MonthlySalaryTypes { get; set; } = new List<MonthlySalaryType>();
}
