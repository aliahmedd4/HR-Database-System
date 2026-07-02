using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Reimbursement
{
    public int ReimbursementId { get; set; }

    public int EmployeeId { get; set; }

    public string? Type { get; set; }

    public string? ClaimType { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public string? CurrentStatus { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
