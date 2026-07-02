using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class PartTimeContract
{
    public int ContractId { get; set; }

    public int? WorkingHours { get; set; }

    public string? Schedule { get; set; }

    public int? HourlyRate { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
