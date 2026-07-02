using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class EmployeeSkill
{
    public int EmployeeSkillId { get; set; }

    public int EmployeeId { get; set; }

    public int SkillId { get; set; }

    public string? ProficiencyLevel { get; set; }

    public DateOnly? VerifiedDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
