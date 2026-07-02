using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class VacationLeave
{
    public int LeaveId { get; set; }

    public int? CarryOverDays { get; set; }

    public string? ApprovingManager { get; set; }

    public virtual Leave Leave { get; set; } = null!;
}
