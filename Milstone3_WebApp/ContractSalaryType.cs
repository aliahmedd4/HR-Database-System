using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ContractSalaryType
{
    public int ContractSalaryTypeId { get; set; }

    public int SalaryTypeId { get; set; }

    public decimal ContractValue { get; set; }

    public string? InstallmentDetails { get; set; }

    public virtual SalaryType SalaryType { get; set; } = null!;
}
