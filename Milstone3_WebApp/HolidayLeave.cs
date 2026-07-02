using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class HolidayLeave
{
    public int LeaveId { get; set; }

    public string? HolidayName { get; set; }

    public string? OfficialRecognition { get; set; }

    public string? RegionalScope { get; set; }

    public bool? IsNationalHoliday { get; set; }

    public DateOnly? HolidayDate { get; set; }

    public virtual Leave Leave { get; set; } = null!;
}
