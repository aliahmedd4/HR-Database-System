using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class InternshipContract
{
    public int ContractId { get; set; }

    public int? SupervisorId { get; set; }

    public decimal? StipendRelated { get; set; }

    public string? LearningObjectives { get; set; }

    public string? Evaluation { get; set; }

    public string? Mentoring { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Employee? Supervisor { get; set; }
}
