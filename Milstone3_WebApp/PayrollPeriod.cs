using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class PayrollPeriod
{
    public int PayrollPeriodId { get; set; }

    public int PayrollId { get; set; }

    public string PeriodName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateOnly PayDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}
