using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class HourlySalaryType
{
    public int HourlySalaryTypeId { get; set; }

    public int SalaryTypeId { get; set; }

    public decimal HourlyRate { get; set; }

    public int? MaxMonthlyHours { get; set; }

    public virtual SalaryType SalaryType { get; set; } = null!;
}
