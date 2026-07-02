using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Device
{
    public int DeviceId { get; set; }

    public string DeviceName { get; set; } = null!;

    public int? TerminalId { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int EmployeeId { get; set; }

    public string? DeviceType { get; set; }

    public string DeviceIdentifier { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AttendanceSource> AttendanceSources { get; set; } = new List<AttendanceSource>();

    public virtual Employee Employee { get; set; } = null!;
}
