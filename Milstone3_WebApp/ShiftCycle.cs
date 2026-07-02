using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ShiftCycle
{
    public int CycleId { get; set; }

    public string CycleName { get; set; } = null!;

    public int CycleDays { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ShiftCycleAssignment> ShiftCycleAssignments { get; set; } = new List<ShiftCycleAssignment>();
}
