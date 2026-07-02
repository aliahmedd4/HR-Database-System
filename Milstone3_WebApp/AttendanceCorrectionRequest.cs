using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class AttendanceCorrectionRequest
{
    public int RequestId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime? RequestedCheckIn { get; set; }

    public DateTime? RequestedCheckOut { get; set; }

    public string? Reason { get; set; }

    public string? CorrectionType { get; set; }

    public string? Status { get; set; }

    public string? RecordedBy { get; set; }

    public DateOnly? Date { get; set; }

    public string? ReviewNotes { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
