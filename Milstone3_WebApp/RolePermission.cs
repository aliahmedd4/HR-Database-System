using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class RolePermission
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public string PermissionName { get; set; } = null!;

    public string? PermissionValue { get; set; }

    public string? AllowedAction { get; set; }

    public virtual Role Role { get; set; } = null!;
}
