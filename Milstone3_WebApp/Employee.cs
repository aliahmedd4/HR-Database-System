using System;
using System.Collections.Generic;

namespace Milstone3_WebApp;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? NationalId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? CountryOfBirth { get; set; }

    public DateOnly HireDate { get; set; }

    public DateOnly? TerminationDate { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public int? ManagerId { get; set; }

    public int? ContractId { get; set; }

    public int? PayGradeId { get; set; }

    public int? SalaryTypeId { get; set; }

    public int? CurrencyId { get; set; }

    public int? TaxFormId { get; set; }

    public decimal? BaseSalary { get; set; }

    public bool? IsActive { get; set; }

    public bool? ProfileCompletion { get; set; }

    public string? ProfileImage { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? Relationship { get; set; }

    public string? Biography { get; set; }

    public string? EmploymentProgress { get; set; }

    public string? AccountStatus { get; set; }

    public string? EmploymentStatus { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllowanceDeduction> AllowanceDeductions { get; set; } = new List<AllowanceDeduction>();

    public virtual ICollection<AttendanceCorrectionRequest> AttendanceCorrectionRequests { get; set; } = new List<AttendanceCorrectionRequest>();

    public virtual ICollection<AttendanceLog> AttendanceLogChangedByNavigations { get; set; } = new List<AttendanceLog>();

    public virtual ICollection<AttendanceLog> AttendanceLogEmployees { get; set; } = new List<AttendanceLog>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Contract? Contract { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<EmployeeNotification> EmployeeNotifications { get; set; } = new List<EmployeeNotification>();

    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();

    public virtual ICollection<EmployeeVerification> EmployeeVerifications { get; set; } = new List<EmployeeVerification>();

    public virtual ICollection<InternshipContract> InternshipContracts { get; set; } = new List<InternshipContract>();

    public virtual ICollection<Employee> InverseManager { get; set; } = new List<Employee>();

    public virtual ICollection<LeaveRequest> LeaveRequestApprovedByNavigations { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<LeaveRequest> LeaveRequestEmployees { get; set; } = new List<LeaveRequest>();

    public virtual Employee? Manager { get; set; }

    public virtual ICollection<ManagerNote> ManagerNoteEmployees { get; set; } = new List<ManagerNote>();

    public virtual ICollection<ManagerNote> ManagerNoteManagers { get; set; } = new List<ManagerNote>();

    public virtual ICollection<Mission> MissionAssignedByNavigations { get; set; } = new List<Mission>();

    public virtual ICollection<Mission> MissionEmployees { get; set; } = new List<Mission>();

    public virtual ICollection<Mission> MissionManagers { get; set; } = new List<Mission>();

    public virtual ICollection<PayrollLog> PayrollLogs { get; set; } = new List<PayrollLog>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual Position? Position { get; set; }

    public virtual ICollection<Reimbursement> Reimbursements { get; set; } = new List<Reimbursement>();

    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
}
