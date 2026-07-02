using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Mission
{
    public int MissionId { get; set; }

    public string? Destination { get; set; }

    public int? ManagerId { get; set; }

    public int EmployeeId { get; set; }

    public string MissionName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Location { get; set; }

    public string? Status { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Employee? AssignedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee? Manager { get; set; }
}
