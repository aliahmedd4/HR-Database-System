namespace Milstone3_WebApp.Models;

public class PendingLeaveDto
{
    public int LeaveId { get; set; }
    public int RequestId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public bool IsIrregular { get; set; }
}

