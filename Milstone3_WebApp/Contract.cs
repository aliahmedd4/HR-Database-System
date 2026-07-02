using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Contract
{
    public int ContractId { get; set; }

    public int EmployeeId { get; set; }

    public string ContractType { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Type { get; set; }

    public string? CurrentState { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ConsultantContract? ConsultantContract { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual FullTimeContract? FullTimeContract { get; set; }

    public virtual InternshipContract? InternshipContract { get; set; }

    public virtual PartTimeContract? PartTimeContract { get; set; }
}
