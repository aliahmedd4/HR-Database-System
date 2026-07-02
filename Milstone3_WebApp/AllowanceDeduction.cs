using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class AllowanceDeduction
{
    public int AdId { get; set; }

    public int PayrollId { get; set; }

    public int? EmployeeId { get; set; }

    public string? Type { get; set; }

    public string? Currency { get; set; }

    public TimeOnly? Duration { get; set; }

    public DateOnly? Timezone { get; set; }

    public string ItemType { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal? Percentage { get; set; }

    public string? Description { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Payroll Payroll { get; set; } = null!;
}
