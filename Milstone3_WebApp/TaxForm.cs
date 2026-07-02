using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class TaxForm
{
    public int TaxformId { get; set; }

    public string FormName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Jurisdiction { get; set; }

    public DateOnly? ValidityPeriod { get; set; }

    public string? FormContent { get; set; }
}
