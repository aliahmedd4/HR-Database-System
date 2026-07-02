using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class PayrollLog
{
    public int LogId { get; set; }

    public int? PayrollLogId { get; set; }

    public int? PayrollId { get; set; }

    public string? Actor { get; set; }

    public string? ModificationType { get; set; }

    public string? ActionType { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ChangedBy { get; set; }

    public string? ChangeReason { get; set; }

    public DateTime? ChangedDate { get; set; }

    public virtual Employee? ChangedByNavigation { get; set; }

    public virtual Payroll? Payroll { get; set; }
}
