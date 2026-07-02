using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ProbationLeave
{
    public int LeaveId { get; set; }

    public bool? ApplicableDuringProbation { get; set; }

    public DateOnly? EligibilityStartDate { get; set; }

    public TimeOnly? ProbationPeriod { get; set; }

    public virtual Leave Leave { get; set; } = null!;
}
