using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ShiftCycleAssignment
{
    public int AssignmentId { get; set; }

    public int CycleId { get; set; }

    public int ShiftId { get; set; }

    public int? OrderNumber { get; set; }

    public int DaySequence { get; set; }

    public virtual ShiftCycle Cycle { get; set; } = null!;

    public virtual ShiftSchedule Shift { get; set; } = null!;
}
