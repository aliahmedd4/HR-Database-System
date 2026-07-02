using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class LeaveDocument
{
    public int DocumentId { get; set; }

    public int LeaveRequestId { get; set; }

    public string? DocumentType { get; set; }

    public string? FilePath { get; set; }

    public DateOnly? UploadedAt { get; set; }

    public virtual LeaveRequest LeaveRequest { get; set; } = null!;
}
