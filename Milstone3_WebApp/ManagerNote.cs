using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class ManagerNote
{
    public int NoteId { get; set; }

    public int EmployeeId { get; set; }

    public int ManagerId { get; set; }

    public string? NoteType { get; set; }

    public string NoteContent { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee Manager { get; set; } = null!;
}
