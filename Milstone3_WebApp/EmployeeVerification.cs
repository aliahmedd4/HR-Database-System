using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class EmployeeVerification
{
    public int EmployeeVerificationId { get; set; }

    public int EmployeeId { get; set; }

    public int VerificationId { get; set; }

    public DateOnly? VerificationDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? DocumentPath { get; set; }

    public string? Status { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Verification Verification { get; set; } = null!;
}
