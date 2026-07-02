using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Currency
{
    public int CurrencyId { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string CurrencyName { get; set; } = null!;

    public decimal? ExchangeRate { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? LastUpdated { get; set; }

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
