using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Verification
{
    public int VerificationId { get; set; }

    public string VerificationType { get; set; } = null!;

    public string? Description { get; set; }

    public string? Issuer { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryPeriod { get; set; }

    public virtual ICollection<EmployeeVerification> EmployeeVerifications { get; set; } = new List<EmployeeVerification>();
}
