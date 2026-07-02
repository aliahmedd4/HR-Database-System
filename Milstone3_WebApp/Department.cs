using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? Purpose { get; set; }

    public int? DepartmentHeadId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
}
