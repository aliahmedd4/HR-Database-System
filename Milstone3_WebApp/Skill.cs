using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Skill
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
}
