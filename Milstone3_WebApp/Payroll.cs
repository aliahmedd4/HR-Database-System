using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Payroll
{
    public int PayrollId { get; set; }

    public int EmployeeId { get; set; }

    public decimal? Taxes { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public int PeriodId { get; set; }

    public string? Adjustments { get; set; }

    public string? Contributions { get; set; }

    public decimal? ActualPay { get; set; }

    public decimal? NetSalary { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public decimal BaseAmount { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal? TotalAllowances { get; set; }

    public decimal? TotalDeductions { get; set; }

    public int CurrencyId { get; set; }

    public decimal? HoursWorked { get; set; }

    public decimal? OvertimeHours { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AllowanceDeduction> AllowanceDeductions { get; set; } = new List<AllowanceDeduction>();

    public virtual Currency Currency { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<PayrollLog> PayrollLogs { get; set; } = new List<PayrollLog>();
}
