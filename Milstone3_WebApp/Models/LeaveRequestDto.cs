namespace Milstone3_WebApp.Models;

public class LeaveRequestDto
{
    public int LeaveId { get; set; }
    public string? LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? AttachmentPath { get; set; }
    public string? Reason { get; set; }
    public string? Justification { get; set; }
}

