using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionTitle { get; set; } = null!;

    public string? Responsibilities { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
