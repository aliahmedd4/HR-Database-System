using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class AttendanceSource
{
    public int AttendaceId { get; set; }

    public int? DeviceId { get; set; }

    public string SourceType { get; set; } = null!;

    public string SourceIdentifier { get; set; } = null!;

    public string? LocationName { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? RecordedAt { get; set; }

    public virtual Device? Device { get; set; }
}
