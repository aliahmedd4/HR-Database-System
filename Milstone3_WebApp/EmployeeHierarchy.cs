using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class EmployeeHierarchy
{
    public int? HierarchyLevel { get; set; }

    public int EmployeeId { get; set; }

    public int ManagerId { get; set; }

    public int Level { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee Manager { get; set; } = null!;
}
