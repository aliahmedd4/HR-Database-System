using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ConsultantContract
{
    public int ContractId { get; set; }

    public string? ProjectScope { get; set; }

    public int? Fees { get; set; }

    public DateTime? PaymentSchedule { get; set; }

    public decimal? HourlyRate { get; set; }

    public int? MaxHoursPerMonth { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
