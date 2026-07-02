using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class SickLeave
{
    public int LeaveId { get; set; }

    public bool? MedicalCertRequired { get; set; }

    public int PhysicianId { get; set; }

    public virtual Leave Leave { get; set; } = null!;
}
