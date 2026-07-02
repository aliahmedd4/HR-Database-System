using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class PayGrade
{
    public int PayGradeId { get; set; }

    public string GradeName { get; set; } = null!;

    public decimal MinSalary { get; set; }

    public decimal MaxSalary { get; set; }

    public DateTime? CreatedAt { get; set; }
}
