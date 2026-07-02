namespace Milstone3_WebApp.Models;

public class LeaveBalanceDto
{
    public int LeaveId { get; set; }
    public decimal Allocated_Days { get; set; }
    public decimal Balance_Days { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public int UsedDays { get; set; }

    public decimal RemainingDays => TotalDays - UsedDays;
}

