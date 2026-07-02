using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ApprovalWorkflow
{
    public int WorkflowId { get; set; }

    public string WorkflowName { get; set; } = null!;

    public string? WorkflowType { get; set; }

    public decimal? ThresholdAmount { get; set; }

    public string? ApproverRole { get; set; }

    public string? CreatedBy { get; set; }

    public string? Status { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
