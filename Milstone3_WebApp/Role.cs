using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Purpose { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
