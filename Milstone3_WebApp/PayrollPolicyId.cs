using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class PayrollPolicyId
{
    public int PayrollId { get; set; }

    public int PolicyId { get; set; }

    public int? EmployeeId { get; set; }

    public int? DepartmentId { get; set; }

    public virtual Department? Department { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Payroll Payroll { get; set; } = null!;

    public virtual PayrollPolicy Policy { get; set; } = null!;
}
