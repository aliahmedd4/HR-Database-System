using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ApprovalWorkflowStep
{
    public int WorkflowId { get; set; }

    public int? StepNumber { get; set; }

    public string? ActionRequired { get; set; }

    public int StepSequence { get; set; }

    public int? RoleId { get; set; }

    public int? ApproverDepartmentId { get; set; }

    public bool? IsRequired { get; set; }

    public virtual Department? ApproverDepartment { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ApprovalWorkflow Workflow { get; set; } = null!;
}
