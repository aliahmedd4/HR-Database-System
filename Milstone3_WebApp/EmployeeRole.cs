using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class EmployeeRole
{
    public int EmployeeRoleId { get; set; }

    public int EmployeeId { get; set; }

    public int RoleId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
