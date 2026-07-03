using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Milstone3_WebApp;

public partial class HrPayrollSystemContext : DbContext
{
    public HrPayrollSystemContext()
    {
    }

    public HrPayrollSystemContext(DbContextOptions<HrPayrollSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AllowanceDeduction> AllowanceDeductions { get; set; }

    public virtual DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }

    public virtual DbSet<ApprovalWorkflowStep> ApprovalWorkflowSteps { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AttendanceCorrectionRequest> AttendanceCorrectionRequests { get; set; }

    public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; }

    public virtual DbSet<AttendanceSource> AttendanceSources { get; set; }

    public virtual DbSet<BonusPolicy> BonusPolicies { get; set; }

    public virtual DbSet<ConsultantContract> ConsultantContracts { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractSalaryType> ContractSalaryTypes { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<DeductionPolicy> DeductionPolicies { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeHierarchy> EmployeeHierarchies { get; set; }

    public virtual DbSet<EmployeeNotification> EmployeeNotifications { get; set; }

    public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }

    public virtual DbSet<EmployeeSkill> EmployeeSkills { get; set; }

    public virtual DbSet<EmployeeVerification> EmployeeVerifications { get; set; }

    public virtual DbSet<FullTimeContract> FullTimeContracts { get; set; }

    public virtual DbSet<HolidayLeave> HolidayLeaves { get; set; }

    public virtual DbSet<HourlySalaryType> HourlySalaryTypes { get; set; }

    public virtual DbSet<InternshipContract> InternshipContracts { get; set; }

    public virtual DbSet<LatenessPolicy> LatenessPolicies { get; set; }

    public virtual DbSet<Leave> Leaves { get; set; }

    public virtual DbSet<LeaveDocument> LeaveDocuments { get; set; }

    public virtual DbSet<LeaveEntitlement> LeaveEntitlements { get; set; }

    public virtual DbSet<LeavePolicy> LeavePolicies { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<ManagerNote> ManagerNotes { get; set; }

    public virtual DbSet<Mission> Missions { get; set; }

    public virtual DbSet<MonthlySalaryType> MonthlySalaryTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OvertimePolicy> OvertimePolicies { get; set; }

    public virtual DbSet<PartTimeContract> PartTimeContracts { get; set; }

    public virtual DbSet<PayGrade> PayGrades { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<PayrollLog> PayrollLogs { get; set; }

    public virtual DbSet<PayrollPeriod> PayrollPeriods { get; set; }

    public virtual DbSet<PayrollPolicy> PayrollPolicies { get; set; }

    public virtual DbSet<PayrollPolicyId> PayrollPolicyIds { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<ProbationLeave> ProbationLeaves { get; set; }

    public virtual DbSet<Reimbursement> Reimbursements { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<SalaryType> SalaryTypes { get; set; }

    public virtual DbSet<ShiftAssignment> ShiftAssignments { get; set; }

    public virtual DbSet<ShiftCycle> ShiftCycles { get; set; }

    public virtual DbSet<ShiftCycleAssignment> ShiftCycleAssignments { get; set; }

    public virtual DbSet<ShiftSchedule> ShiftSchedules { get; set; }

    public virtual DbSet<SickLeave> SickLeaves { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<TaxForm> TaxForms { get; set; }

    public virtual DbSet<VacationLeave> VacationLeaves { get; set; }

    public virtual DbSet<Verification> Verifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured in Program.cs via dependency injection
        // This method is only called if options are not configured externally
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback connection string (should not be used in production)
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HR_Payroll_System;Trusted_Connection=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");

            entity.HasKey(e => e.NotificationId);

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.NotificationType).HasColumnName("notification_type");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.Urgency).HasColumnName("urgency");
            entity.Property(e => e.ReadStatus).HasColumnName("read_status");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.MessageContent).HasColumnName("message_content");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        });
        modelBuilder.Entity<AllowanceDeduction>(entity =>
        {
            entity.HasKey(e => e.AdId).HasName("PK__Allowanc__CAA4A6274AC3CF40");

            entity.ToTable("AllowanceDeduction");

            entity.Property(e => e.AdId).HasColumnName("ad_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Currency)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("currency");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ItemName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("item_type");
            entity.Property(e => e.PayrollId).HasColumnName("payroll_id");
            entity.Property(e => e.Percentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("percentage");
            entity.Property(e => e.Timezone).HasColumnName("timezone");
            entity.Property(e => e.Type)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Employee).WithMany(p => p.AllowanceDeductions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Allowance__emplo__3418A56D");

            entity.HasOne(d => d.Payroll).WithMany(p => p.AllowanceDeductions)
                .HasForeignKey(d => d.PayrollId)
                .HasConstraintName("FK__Allowance__payro__33248134");
        });

        modelBuilder.Entity<ApprovalWorkflow>(entity =>
        {
            entity.HasKey(e => e.WorkflowId).HasName("PK__Approval__64A76B70C51B7DDD");

            entity.ToTable("ApprovalWorkflow");

            entity.Property(e => e.WorkflowId).HasColumnName("workflow_id");
            entity.Property(e => e.ApproverRole)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("approver_role");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Status)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.ThresholdAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("threshold_amount");
            entity.Property(e => e.WorkflowName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("workflow_name");
            entity.Property(e => e.WorkflowType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("workflow_type");
        });

        modelBuilder.Entity<ApprovalWorkflowStep>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ApprovalWorkflowStep");

            entity.Property(e => e.ActionRequired)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("action_required");
            entity.Property(e => e.ApproverDepartmentId).HasColumnName("approver_department_id");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(true)
                .HasColumnName("is_required");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.StepNumber).HasColumnName("step_number");
            entity.Property(e => e.StepSequence).HasColumnName("step_sequence");
            entity.Property(e => e.WorkflowId).HasColumnName("workflow_id");

            entity.HasOne(d => d.ApproverDepartment).WithMany()
                .HasForeignKey(d => d.ApproverDepartmentId)
                .HasConstraintName("FK__ApprovalW__appro__73FE2058");

            entity.HasOne(d => d.Role).WithMany()
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__ApprovalW__role___7309FC1F");

            entity.HasOne(d => d.Workflow).WithMany()
                .HasForeignKey(d => d.WorkflowId)
                .HasConstraintName("FK__ApprovalW__workf__7215D7E6");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__20D6A968F37E8914");

            entity.ToTable("Attendance");

            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate }, "UQ__Attendan__0C139771B4BC3EC4").IsUnique();

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EntryTime)
                .HasColumnType("datetime")
                .HasColumnName("entry_time");
            entity.Property(e => e.ExceptionId).HasColumnName("exception_id");
            entity.Property(e => e.ExitTime)
                .HasColumnType("datetime")
                .HasColumnName("exit_time");
            entity.Property(e => e.HoursWorked)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("hours_worked");
            entity.Property(e => e.IsLeaveException)
                .HasDefaultValue(false)
                .HasColumnName("is_leave_exception");
            entity.Property(e => e.LatenessMinutes)
                .HasDefaultValue(0)
                .HasColumnName("lateness_minutes");
            entity.Property(e => e.LeaveRequestId).HasColumnName("leave_request_id");
            entity.Property(e => e.LoginMethod)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("login_method");
            entity.Property(e => e.LogoutMethod)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("logout_method");
            entity.Property(e => e.OvertimeHours)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("overtime_hours");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.SourceId)
                .HasMaxLength(100)
                .HasColumnName("source_id");
            entity.Property(e => e.SourceType)
                .HasMaxLength(50)
                .HasColumnName("source_type");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Present")
                .HasColumnName("status");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__emplo__44841760");

            entity.HasOne(d => d.Shift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK__Attendanc__shift__45783B99");
        });

        modelBuilder.Entity<AttendanceCorrectionRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Attendan__18D3B90F57F58DA7");

            entity.ToTable("AttendanceCorrectionRequest");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CorrectionType)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("correction_type");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("reason");
            entity.Property(e => e.RecordedBy)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("recorded_by");
            entity.Property(e => e.RequestedCheckIn)
                .HasColumnType("datetime")
                .HasColumnName("requested_check_in");
            entity.Property(e => e.RequestedCheckOut)
                .HasColumnType("datetime")
                .HasColumnName("requested_check_out");
            entity.Property(e => e.ReviewNotes)
                .HasMaxLength(500)
                .HasColumnName("review_notes");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Employee).WithMany(p => p.AttendanceCorrectionRequests)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__emplo__50E9EE45");
        });

        modelBuilder.Entity<AttendanceLog>(entity =>
        {
            entity.HasKey(e => e.AttendanceLogId).HasName("PK__Attendan__DB38FB09FC3A2CDE");

            entity.ToTable("AttendanceLog");

            entity.Property(e => e.AttendanceLogId).HasColumnName("attendance_log_id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("action_type");
            entity.Property(e => e.Actor)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("actor");
            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.NewValue)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("new_value");
            entity.Property(e => e.OldValue)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("old_value");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("reason");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");

            entity.HasOne(d => d.Attendance).WithMany(p => p.AttendanceLogs)
                .HasForeignKey(d => d.AttendanceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Attendanc__atten__4A3CF0B6");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.AttendanceLogChangedByNavigations)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK__Attendanc__chang__4C253928");

            entity.HasOne(d => d.Employee).WithMany(p => p.AttendanceLogEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__emplo__4B3114EF");
        });

        modelBuilder.Entity<AttendanceSource>(entity =>
        {
            entity.HasKey(e => e.AttendaceId).HasName("PK__Attendan__5EC46CC22225FFD1");

            entity.ToTable("AttendanceSource");

            entity.HasIndex(e => new { e.SourceType, e.SourceIdentifier }, "UQ__Attendan__3AF9F0A312401569").IsUnique();

            entity.Property(e => e.AttendaceId).HasColumnName("attendace_id");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 8)")
                .HasColumnName("latitude");
            entity.Property(e => e.LocationName)
                .HasMaxLength(200)
                .HasColumnName("location_name");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(11, 8)")
                .HasColumnName("longitude");
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("recorded_at");
            entity.Property(e => e.SourceIdentifier)
                .HasMaxLength(100)
                .HasColumnName("source_identifier");
            entity.Property(e => e.SourceType)
                .HasMaxLength(50)
                .HasColumnName("source_type");

            entity.HasOne(d => d.Device).WithMany(p => p.AttendanceSources)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__Attendanc__devic__5796EBD4");
        });

        modelBuilder.Entity<BonusPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__BonusPol__47DA3F036C1F2AE1");

            entity.ToTable("BonusPolicy");

            entity.Property(e => e.PolicyId)
                .ValueGeneratedNever()
                .HasColumnName("policy_id");
            entity.Property(e => e.BonusAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("bonus_amount");
            entity.Property(e => e.BonusPercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("bonus_percentage");
            entity.Property(e => e.BonusType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("bonus_type");
            entity.Property(e => e.EligibilityCriteria)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("eligibility_criteria");

            entity.HasOne(d => d.Policy).WithOne(p => p.BonusPolicy)
                .HasForeignKey<BonusPolicy>(d => d.PolicyId)
                .HasConstraintName("FK__BonusPoli__polic__4913C253");
        });

        modelBuilder.Entity<ConsultantContract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Consulta__F8D6642345BDC52B");

            entity.ToTable("ConsultantContract");

            entity.Property(e => e.ContractId)
                .ValueGeneratedNever()
                .HasColumnName("contract_id");
            entity.Property(e => e.Fees).HasColumnName("fees");
            entity.Property(e => e.HourlyRate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("hourly_rate");
            entity.Property(e => e.MaxHoursPerMonth).HasColumnName("max_hours_per_month");
            entity.Property(e => e.PaymentSchedule)
                .HasColumnType("datetime")
                .HasColumnName("payment_schedule");
            entity.Property(e => e.ProjectScope)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("project_scope");

            entity.HasOne(d => d.Contract).WithOne(p => p.ConsultantContract)
                .HasForeignKey<ConsultantContract>(d => d.ContractId)
                .HasConstraintName("FK__Consultan__contr__0C3FBE3D");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__F8D66423780893AB");

            entity.ToTable("Contract");

            entity.HasIndex(e => e.EmployeeId, "idx_contract_employee");

            entity.HasIndex(e => e.CurrentState, "idx_contract_status");

            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.ContractType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("contract_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentState)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Active")
                .HasColumnName("current_state");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Type)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<ContractSalaryType>(entity =>
        {
            entity.HasKey(e => e.ContractSalaryTypeId).HasName("PK__Contract__23D88403098FEA1C");

            entity.ToTable("ContractSalaryType");

            entity.Property(e => e.ContractSalaryTypeId).HasColumnName("contract_salary_type_id");
            entity.Property(e => e.ContractValue)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("contract_value");
            entity.Property(e => e.InstallmentDetails)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("installment_details");
            entity.Property(e => e.SalaryTypeId).HasColumnName("salary_type_id");

            entity.HasOne(d => d.SalaryType).WithMany(p => p.ContractSalaryTypes)
                .HasForeignKey(d => d.SalaryTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ContractS__salar__7ED0B486");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyId).HasName("PK__Currency__C7F543D385A027D9");

            entity.ToTable("Currency");

            entity.HasIndex(e => e.CurrencyCode, "UQ__Currency__6008D0BA54BEC4F3").IsUnique();

            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("currency_code");
            entity.Property(e => e.CurrencyName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("currency_name");
            entity.Property(e => e.ExchangeRate)
                .HasDefaultValue(1.0m)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("exchange_rate");
        });

        modelBuilder.Entity<DeductionPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Deductio__47DA3F03BDC8DFFC");

            entity.ToTable("DeductionPolicy");

            entity.Property(e => e.PolicyId)
                .ValueGeneratedNever()
                .HasColumnName("policy_id");
            entity.Property(e => e.AppliesToAll)
                .HasDefaultValue(false)
                .HasColumnName("applies_to_all");
            entity.Property(e => e.CalculationMode)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("calculation_mode");
            entity.Property(e => e.DeductionAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("deduction_amount");
            entity.Property(e => e.DeductionPercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("deduction_percentage");
            entity.Property(e => e.DeductionReason)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("deduction_reason");

            entity.HasOne(d => d.Policy).WithOne(p => p.DeductionPolicy)
                .HasForeignKey<DeductionPolicy>(d => d.PolicyId)
                .HasConstraintName("FK__Deduction__polic__4ECC9BA9");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C22324228A8059F7");

            entity.ToTable("Department");

            entity.HasIndex(e => e.DepartmentName, "UQ__Departme__226ED157CAB720DD").IsUnique();

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentHeadId).HasColumnName("department_head_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("department_name");
            entity.Property(e => e.Purpose)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("purpose");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__Device__3B085D8BA42A1EED");

            entity.ToTable("Device");

            entity.HasIndex(e => e.DeviceIdentifier, "UQ__Device__F91932EE25D51786").IsUnique();

            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceIdentifier)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("device_identifier");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("device_name");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("device_type");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("longitude");
            entity.Property(e => e.TerminalId).HasColumnName("terminal_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.Devices)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Device__employee__1939C6A8");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__C52E0BA84943801B");

            entity.ToTable("Employee");

            entity.HasIndex(e => e.NationalId, "UQ__Employee__9560E95D2DC94F3E").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Employee__AB6E6164DA904273").IsUnique();

            entity.HasIndex(e => e.EmployeeCode, "UQ__Employee__B0AA734580E214D0").IsUnique();

            entity.HasIndex(e => e.IsActive, "idx_employee_active");

            entity.HasIndex(e => e.DepartmentId, "idx_employee_department");

            entity.HasIndex(e => e.ManagerId, "idx_employee_manager");

            entity.HasIndex(e => e.PositionId, "idx_employee_position");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.AccountStatus)
                .HasMaxLength(50)
                .HasColumnName("account_status");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.BaseSalary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("base_salary");
            entity.Property(e => e.Biography).HasColumnName("biography");
            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.CountryOfBirth)
                .HasMaxLength(100)
                .HasColumnName("country_of_birth");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmergencyContactName)
                .HasMaxLength(200)
                .HasColumnName("emergency_contact_name");
            entity.Property(e => e.EmergencyContactPhone)
                .HasMaxLength(20)
                .HasColumnName("emergency_contact_phone");
            entity.Property(e => e.EmployeeCode)
                .HasMaxLength(50)
                .HasColumnName("employee_code");
            entity.Property(e => e.EmploymentProgress)
                .HasMaxLength(100)
                .HasColumnName("employment_progress");
            entity.Property(e => e.EmploymentStatus)
                .HasMaxLength(50)
                .HasColumnName("employment_status");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.NationalId)
                .HasMaxLength(50)
                .HasColumnName("national_id");
            entity.Property(e => e.PayGradeId).HasColumnName("pay_grade_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PositionId).HasColumnName("position_id");
            entity.Property(e => e.ProfileCompletion)
                .HasDefaultValue(false)
                .HasColumnName("profile_completion");
            entity.Property(e => e.ProfileImage)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("profile_image");
            entity.Property(e => e.Relationship)
                .HasMaxLength(50)
                .HasColumnName("relationship");
            entity.Property(e => e.SalaryTypeId).HasColumnName("salary_type_id");
            entity.Property(e => e.TaxFormId).HasColumnName("tax_form_id");
            entity.Property(e => e.TerminationDate).HasColumnName("termination_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");

            entity.HasOne(d => d.Contract).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK__Employee__contra__00A32308");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Employee__depart__7EBADA96");

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_Employee_Manager");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("FK__Employee__positi__7FAEFECF");
        });

        modelBuilder.Entity<EmployeeHierarchy>(entity =>
        {
            entity
                .HasKey(eh => new { eh.EmployeeId, eh.ManagerId });
                

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.HierarchyLevel).HasColumnName("hierarchy_level");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Employee).WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EmployeeH__emplo__67984973");

            entity.HasOne(d => d.Manager).WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EmployeeH__manag__688C6DAC");
        });

        modelBuilder.Entity<EmployeeNotification>(entity =>
        {
            entity.HasKey(e => e.EmployeeNotificationId).HasName("PK__Employee__1632191C2B49BDB8");

            entity.ToTable("Employee_Notification");

            entity.Property(e => e.EmployeeNotificationId).HasColumnName("employee_notification_id");
            entity.Property(e => e.DeliveredAt)
                .HasColumnType("datetime")
                .HasColumnName("delivered_at");
            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("delivery_status");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeNotifications)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Employee___emplo__63C7B88F");

            entity.HasOne(d => d.Notification).WithMany(p => p.EmployeeNotifications)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("FK__Employee___notif__62D39456");
        });

        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.HasKey(e => e.EmployeeRoleId).HasName("PK__Employee__AF99BE5B6E6A4755");

            entity.ToTable("Employee_Role");

            entity.HasIndex(e => new { e.EmployeeId, e.RoleId }, "UQ__Employee__124E9DF5F566711A").IsUnique();

            entity.Property(e => e.EmployeeRoleId).HasColumnName("employee_role_id");
            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("assigned_date");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeRoles)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Employee___emplo__7C095674");

            entity.HasOne(d => d.Role).WithMany(p => p.EmployeeRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employee___role___7CFD7AAD");
        });

        modelBuilder.Entity<EmployeeSkill>(entity =>
        {
            entity.HasKey(e => e.EmployeeSkillId).HasName("PK__Employee__36C2C1F67A5DA0EB");

            entity.ToTable("Employee_Skill");

            entity.HasIndex(e => new { e.EmployeeId, e.SkillId }, "UQ__Employee__4A95A39E6A8F25B8").IsUnique();

            entity.Property(e => e.EmployeeSkillId).HasColumnName("employee_skill_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ProficiencyLevel)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("proficiency_level");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.VerifiedDate).HasColumnName("verified_date");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeSkills)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Employee___emplo__18A59522");

            entity.HasOne(d => d.Skill).WithMany(p => p.EmployeeSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employee___skill__1999B95B");
        });

        modelBuilder.Entity<EmployeeVerification>(entity =>
        {
            entity.HasKey(e => e.EmployeeVerificationId).HasName("PK__Employee__3C777DD9786B2476");

            entity.ToTable("Employee_Verification");

            entity.Property(e => e.EmployeeVerificationId).HasColumnName("employee_verification_id");
            entity.Property(e => e.DocumentPath)
                .HasMaxLength(500)
                .HasColumnName("document_path");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Valid")
                .HasColumnName("status");
            entity.Property(e => e.VerificationDate).HasColumnName("verification_date");
            entity.Property(e => e.VerificationId).HasColumnName("verification_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeVerifications)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Employee___emplo__213ADB23");

            entity.HasOne(d => d.Verification).WithMany(p => p.EmployeeVerifications)
                .HasForeignKey(d => d.VerificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employee___verif__222EFF5C");
        });

        modelBuilder.Entity<FullTimeContract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__FullTime__F8D664234F190AFF");

            entity.ToTable("FullTimeContract");

            entity.Property(e => e.ContractId)
                .ValueGeneratedNever()
                .HasColumnName("contract_id");
            entity.Property(e => e.Benefits)
                .HasMaxLength(500)
                .HasColumnName("benefits");
            entity.Property(e => e.InsuranceEligibility)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("insurance_eligibility");
            entity.Property(e => e.LeaveEntitlement)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("leave_entitlement");
            entity.Property(e => e.WeeklyWorkingHours).HasColumnName("weekly_working_hours");

            entity.HasOne(d => d.Contract).WithOne(p => p.FullTimeContract)
                .HasForeignKey<FullTimeContract>(d => d.ContractId)
                .HasConstraintName("FK__FullTimeC__contr__03AA783C");
        });

        modelBuilder.Entity<HolidayLeave>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("PK__HolidayL__743350BC3935C861");

            entity.ToTable("HolidayLeave");

            entity.Property(e => e.LeaveId)
                .ValueGeneratedNever()
                .HasColumnName("leave_id");
            entity.Property(e => e.HolidayDate).HasColumnName("holiday_date");
            entity.Property(e => e.HolidayName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("holiday_name");
            entity.Property(e => e.IsNationalHoliday)
                .HasDefaultValue(true)
                .HasColumnName("is_national_holiday");
            entity.Property(e => e.OfficialRecognition)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("official_recognition");
            entity.Property(e => e.RegionalScope)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("regional_scope");

            entity.HasOne(d => d.Leave).WithOne(p => p.HolidayLeave)
                .HasForeignKey<HolidayLeave>(d => d.LeaveId)
                .HasConstraintName("FK__HolidayLe__leave__75274EBB");
        });

        modelBuilder.Entity<HourlySalaryType>(entity =>
        {
            entity.HasKey(e => e.HourlySalaryTypeId).HasName("PK__HourlySa__C2BED55982EDC018");

            entity.ToTable("HourlySalaryType");

            entity.Property(e => e.HourlySalaryTypeId).HasColumnName("hourly_salary_type_id");
            entity.Property(e => e.HourlyRate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("hourly_rate");
            entity.Property(e => e.MaxMonthlyHours).HasColumnName("max_monthly_hours");
            entity.Property(e => e.SalaryTypeId).HasColumnName("salary_type_id");

            entity.HasOne(d => d.SalaryType).WithMany(p => p.HourlySalaryTypes)
                .HasForeignKey(d => d.SalaryTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HourlySal__salar__4A5CEC47");
        });

        modelBuilder.Entity<InternshipContract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Internsh__F8D6642369FF9641");

            entity.ToTable("InternshipContract");

            entity.Property(e => e.ContractId)
                .ValueGeneratedNever()
                .HasColumnName("contract_id");
            entity.Property(e => e.Evaluation)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("evaluation");
            entity.Property(e => e.LearningObjectives)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("learning_objectives");
            entity.Property(e => e.Mentoring)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("mentoring");
            entity.Property(e => e.StipendRelated)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("stipend_related");
            entity.Property(e => e.SupervisorId).HasColumnName("supervisor_id");

            entity.HasOne(d => d.Contract).WithOne(p => p.InternshipContract)
                .HasForeignKey<InternshipContract>(d => d.ContractId)
                .HasConstraintName("FK__Internshi__contr__10104F21");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.InternshipContracts)
                .HasForeignKey(d => d.SupervisorId)
                .HasConstraintName("FK__Internshi__super__1104735A");
        });

        modelBuilder.Entity<LatenessPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Lateness__47DA3F039DA66CC3");

            entity.ToTable("LatenessPolicy");

            entity.Property(e => e.PolicyId)
                .ValueGeneratedNever()
                .HasColumnName("policy_id");
            entity.Property(e => e.DeductionRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("deduction_rate");
            entity.Property(e => e.GracePeriodMins)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("grace_period_mins");
            entity.Property(e => e.MaxPenaltyPerDay)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("max_penalty_per_day");
            entity.Property(e => e.PenaltyPerLateMinute)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("penalty_per_late_minute");
            entity.Property(e => e.ThresholdMinutes)
                .HasDefaultValue(0)
                .HasColumnName("threshold_minutes");

            entity.HasOne(d => d.Policy).WithOne(p => p.LatenessPolicy)
                .HasForeignKey<LatenessPolicy>(d => d.PolicyId)
                .HasConstraintName("FK__LatenessP__polic__435AE8FD");
        });

        modelBuilder.Entity<Leave>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("PK__Leave__743350BCD699E8F1");

            entity.ToTable("Leave");

            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPaid)
                .HasDefaultValue(true)
                .HasColumnName("is_paid");
            entity.Property(e => e.LeaveDescription)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("leave_description");
            entity.Property(e => e.LeaveType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("leave_type");
            entity.Property(e => e.MaxDaysPerYear).HasColumnName("max_days_per_year");
            entity.Property(e => e.RequiresApproval)
                .HasDefaultValue(true)
                .HasColumnName("requires_approval");
        });

        modelBuilder.Entity<LeaveDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__LeaveDoc__9666E8AC63C508B8");

            entity.ToTable("LeaveDocument");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("document_type");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("file_path");
            entity.Property(e => e.LeaveRequestId).HasColumnName("leave_request_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.LeaveRequest).WithMany(p => p.LeaveDocuments)
                .HasForeignKey(d => d.LeaveRequestId)
                .HasConstraintName("FK__LeaveDocu__leave__18708AF8");
        });

        modelBuilder.Entity<LeaveEntitlement>(entity =>
        {
            entity.HasKey(e => e.EntitlementId).HasName("PK_LeaveEntitlement");

            entity.ToTable("LeaveEntitlement");

            entity.HasIndex(e => new { e.EmployeeId, e.LeaveId, e.Year }, "UQ__LeaveEnt__99EDA4817B3F356C").IsUnique();

            entity.Property(e => e.EntitlementId)
                .ValueGeneratedOnAdd()
                .HasColumnName("entitlement_id");
            entity.Property(e => e.AllocatedDays)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("allocated_days");
            entity.Property(e => e.BalanceDays)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("balance_days");
            entity.Property(e => e.CarryForwardDays)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("carry_forward_days");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Entitlement)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("entitlement");
            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.UsedDays)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("used_days");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Employee).WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveEnti__emplo__083A232F");

            entity.HasOne(d => d.Leave).WithMany()
                .HasForeignKey(d => d.LeaveId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveEnti__leave__092E4768");
        });

        modelBuilder.Entity<LeavePolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__LeavePol__47DA3F030185B6B3");

            entity.ToTable("LeavePolicy");

            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.AccrualRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("accrual_rate");
            entity.Property(e => e.EffectiveDate)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("effective_date");
            entity.Property(e => e.EligibilityMonths)
                .HasDefaultValue(0)
                .HasColumnName("eligibility_months");
            entity.Property(e => e.EligibilityRules)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("eligibility_rules");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.MaxBalance)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("max_balance");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.NoticePeriod).HasColumnName("notice_period");
            entity.Property(e => e.Purpose)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("purpose");
            entity.Property(e => e.ResetOnNewYear).HasColumnName("reset_on_new_year");
            entity.Property(e => e.SpecialLeaveType)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("special_leave_type");

            entity.HasOne(d => d.Leave).WithMany(p => p.LeavePolicies)
                .HasForeignKey(d => d.LeaveId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeavePoli__leave__7DBC94BC");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__LeaveReq__18D3B90F38B88CDF");

            entity.ToTable("LeaveRequest");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.ApprovalTiming).HasColumnName("approval_timing");
            entity.Property(e => e.ApprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.DurationDays)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("duration_days");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsIrregular)
                .HasDefaultValue(false)
                .HasColumnName("is_irregular");
            entity.Property(e => e.Justification)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("justification");
            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.LinkedShiftId).HasColumnName("linked_shift_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("reason");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("submitted_at");
            entity.Property(e => e.TotalDays)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("total_days");
            entity.Property(e => e.OverrideReason)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("override_reason");
            entity.Property(e => e.OverriddenAt)
                .HasColumnType("datetime")
                .HasColumnName("overridden_at");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LeaveRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__LeaveRequ__appro__12B7B1A2");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRequestEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveRequ__emplo__10CF6930");

            entity.HasOne(d => d.Leave).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.LeaveId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveRequ__leave__11C38D69");

            entity.HasOne(d => d.LinkedShift).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.LinkedShiftId)
                .HasConstraintName("FK__LeaveRequ__linke__13ABD5DB");
        });

        modelBuilder.Entity<ManagerNote>(entity =>
        {
            entity.HasKey(e => e.NoteId).HasName("PK__ManagerN__CEDD0FA4AC1B87CC");

            entity.Property(e => e.NoteId).HasColumnName("note_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.NoteContent)
                .HasMaxLength(2000)
                .IsUnicode(false)
                .HasColumnName("note_content");
            entity.Property(e => e.NoteType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("note_type");

            entity.HasOne(d => d.Employee).WithMany(p => p.ManagerNoteEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ManagerNo__emplo__78C2D575");

            entity.HasOne(d => d.Manager).WithMany(p => p.ManagerNoteManagers)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ManagerNo__manag__79B6F9AE");
        });

        modelBuilder.Entity<Mission>(entity =>
        {
            entity.HasKey(e => e.MissionId).HasName("PK__Mission__B5419AB2C7FE1BF9");

            entity.ToTable("Mission");

            entity.Property(e => e.MissionId).HasColumnName("mission_id");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.Destination)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("destination");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("location");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.MissionName)
                .HasMaxLength(200)
                .HasColumnName("mission_name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Assigned")
                .HasColumnName("status");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.MissionAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK__Mission__assigne__0063F73D");

            entity.HasOne(d => d.Employee).WithMany(p => p.MissionEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Mission__employe__7F6FD304");

            entity.HasOne(d => d.Manager).WithMany(p => p.MissionManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Mission__manager__7E7BAECB");
        });

        modelBuilder.Entity<MonthlySalaryType>(entity =>
        {
            entity.HasKey(e => e.MonthlySalaryTypeId).HasName("PK__MonthlyS__1133C26C8A06110F");

            entity.ToTable("MonthlySalaryType");

            entity.Property(e => e.MonthlySalaryTypeId).HasColumnName("monthly_salary_type_id");
            entity.Property(e => e.ContributionScheme)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("contribution_scheme");
            entity.Property(e => e.SalaryTypeId).HasColumnName("salary_type_id");
            entity.Property(e => e.TaxRule)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tax_rule");

            entity.HasOne(d => d.SalaryType).WithMany(p => p.MonthlySalaryTypes)
                .HasForeignKey(d => d.SalaryTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MonthlySa__salar__7BF447DB");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FB2892D4D");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.MessageContent)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("message_content");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("notification_type");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Normal")
                .HasColumnName("priority");
            entity.Property(e => e.ReadStatus)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("read_status");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.Urgency)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("urgency");
        });

        modelBuilder.Entity<OvertimePolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Overtime__47DA3F038CDB9531");

            entity.ToTable("OvertimePolicy");

            entity.Property(e => e.PolicyId)
                .ValueGeneratedNever()
                .HasColumnName("policy_id");
            entity.Property(e => e.MaxHoursPerMonth)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("max_hours_per_month");
            entity.Property(e => e.MinHoursForOvertime)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("min_hours_for_overtime");
            entity.Property(e => e.WeekdayRateMultiplier)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weekday_rate_multiplier");
            entity.Property(e => e.WeekdendRateMultiplier)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weekdend_rate_multiplier");

            entity.HasOne(d => d.Policy).WithOne(p => p.OvertimePolicy)
                .HasForeignKey<OvertimePolicy>(d => d.PolicyId)
                .HasConstraintName("FK__OvertimeP__polic__3CADEB6E");
        });

        modelBuilder.Entity<PartTimeContract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__PartTime__F8D6642338686C70");

            entity.ToTable("PartTimeContract");

            entity.Property(e => e.ContractId)
                .ValueGeneratedNever()
                .HasColumnName("contract_id");
            entity.Property(e => e.HourlyRate).HasColumnName("hourly_rate");
            entity.Property(e => e.Schedule)
                .HasMaxLength(200)
                .HasColumnName("schedule");
            entity.Property(e => e.WorkingHours).HasColumnName("working_hours");

            entity.HasOne(d => d.Contract).WithOne(p => p.PartTimeContract)
                .HasForeignKey<PartTimeContract>(d => d.ContractId)
                .HasConstraintName("FK__PartTimeC__contr__077B0920");
        });

        modelBuilder.Entity<PayGrade>(entity =>
        {
            entity.HasKey(e => e.PayGradeId).HasName("PK__PayGrade__C8AD0DED043A1441");

            entity.ToTable("PayGrade");

            entity.HasIndex(e => e.GradeName, "UQ__PayGrade__3CA226E13C56E77D").IsUnique();

            entity.Property(e => e.PayGradeId).HasColumnName("pay_grade_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GradeName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("grade_name");
            entity.Property(e => e.MaxSalary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("max_salary");
            entity.Property(e => e.MinSalary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("min_salary");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PK__Payroll__D99FC944B51233FD");

            entity.ToTable("Payroll");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodId }, "UQ__Payroll__971C354D8DDC631D").IsUnique();

            entity.Property(e => e.PayrollId).HasColumnName("payroll_id");
            entity.Property(e => e.ActualPay)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("actual_pay");
            entity.Property(e => e.Adjustments)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("adjustments");
            entity.Property(e => e.BaseAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("base_amount");
            entity.Property(e => e.Contributions)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("contributions");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.GrossSalary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("gross_salary");
            entity.Property(e => e.HoursWorked)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("hours_worked");
            entity.Property(e => e.NetSalary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("net_salary");
            entity.Property(e => e.OvertimeHours)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("overtime_hours");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodId).HasColumnName("period_id");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Draft")
                .HasColumnName("status");
            entity.Property(e => e.Taxes)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("taxes");
            entity.Property(e => e.TotalAllowances)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_allowances");
            entity.Property(e => e.TotalDeductions)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_deductions");

            entity.HasOne(d => d.Currency).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payroll__currenc__2E5FCC17");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payroll__employe__2D6BA7DE");
        });

        modelBuilder.Entity<PayrollLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Payroll___9E2397E0BFB16CEC");

            entity.ToTable("Payroll_Log");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("action_type");
            entity.Property(e => e.Actor)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("actor");
            entity.Property(e => e.ChangeReason)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("change_reason");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("changed_date");
            entity.Property(e => e.ModificationType)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("modification_type");
            entity.Property(e => e.NewValue)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("new_value");
            entity.Property(e => e.OldValue)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("old_value");
            entity.Property(e => e.PayrollId).HasColumnName("payroll_id");
            entity.Property(e => e.PayrollLogId).HasColumnName("payroll_log_id");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.PayrollLogs)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK__Payroll_L__chang__5A3E4E55");

            entity.HasOne(d => d.Payroll).WithMany(p => p.PayrollLogs)
                .HasForeignKey(d => d.PayrollId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Payroll_L__payro__594A2A1C");
        });

        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.HasKey(e => e.PayrollPeriodId).HasName("PK__PayrollP__CD8483A2B2B5A04D");

            entity.ToTable("PayrollPeriod");

            entity.Property(e => e.PayrollPeriodId).HasColumnName("payroll_period_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.PayDate).HasColumnName("pay_date");
            entity.Property(e => e.PayrollId).HasColumnName("payroll_id");
            entity.Property(e => e.PeriodName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("period_name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
        });

        modelBuilder.Entity<PayrollPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__PayrollP__47DA3F03E5CCBFE5");

            entity.ToTable("PayrollPolicy");

            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.EffectiveDate)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("effective_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PolicyName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("policy_name");
            entity.Property(e => e.PolicyType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("policy_type");
            entity.Property(e => e.Type)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<PayrollPolicyId>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PayrollPolicy_ID");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.PayrollId).HasColumnName("payroll_id");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");

            entity.HasOne(d => d.Department).WithMany()
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__PayrollPo__depar__539150C6");

            entity.HasOne(d => d.Employee).WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__PayrollPo__emplo__529D2C8D");

            entity.HasOne(d => d.Payroll).WithMany()
                .HasForeignKey(d => d.PayrollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PayrollPo__payro__51A90854");

            entity.HasOne(d => d.Policy).WithMany()
                .HasForeignKey(d => d.PolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PayrollPo__polic__50B4E41B");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__Position__99A0E7A4BFB3DE9D");

            entity.ToTable("Position");

            entity.HasIndex(e => e.PositionTitle, "UQ__Position__AD01610063EEE97B").IsUnique();

            entity.Property(e => e.PositionId).HasColumnName("position_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PositionTitle)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("position_title");
            entity.Property(e => e.Responsibilities)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("responsibilities");
            entity.Property(e => e.Status)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("status");
        });

        modelBuilder.Entity<ProbationLeave>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("PK__Probatio__743350BC95FEB3D3");

            entity.ToTable("ProbationLeave");

            entity.Property(e => e.LeaveId)
                .ValueGeneratedNever()
                .HasColumnName("leave_id");
            entity.Property(e => e.ApplicableDuringProbation)
                .HasDefaultValue(true)
                .HasColumnName("applicable_during_probation");
            entity.Property(e => e.EligibilityStartDate).HasColumnName("eligibility_start_date");
            entity.Property(e => e.ProbationPeriod).HasColumnName("probation_period");

            entity.HasOne(d => d.Leave).WithOne(p => p.ProbationLeave)
                .HasForeignKey<ProbationLeave>(d => d.LeaveId)
                .HasConstraintName("FK__Probation__leave__7156BDD7");
        });

        modelBuilder.Entity<Reimbursement>(entity =>
        {
            entity.HasKey(e => e.ReimbursementId).HasName("PK__Reimburs__F6C26984DBFA553C");

            entity.ToTable("Reimbursement");

            entity.Property(e => e.ReimbursementId).HasColumnName("reimbursement_id");
            entity.Property(e => e.ApprovalDate).HasColumnName("approval_date");
            entity.Property(e => e.ClaimType)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("claim_type");
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("current_status");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Type)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Employee).WithMany(p => p.Reimbursements)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reimburse__emplo__04348821");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CC5AF9A1FC");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__783254B1ABBEC20C").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Purpose)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("purpose");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__RolePerm__E5331AFAF71D4CF9");

            entity.ToTable("RolePermission");

            entity.HasIndex(e => new { e.RoleId, e.PermissionName }, "UQ__RolePerm__5E156A97E4AFFD98").IsUnique();

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.AllowedAction)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("allowed_action");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("permission_name");
            entity.Property(e => e.PermissionValue)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("permission_value");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__RolePermi__role___00CE0B91");
        });

        modelBuilder.Entity<SalaryType>(entity =>
        {
            entity.HasKey(e => e.SalarytypeId).HasName("PK__SalaryTy__BA186D0E0B6855DD");

            entity.ToTable("SalaryType");

            entity.HasIndex(e => e.Type, "UQ__SalaryTy__E3F85248C4ABF654").IsUnique();

            entity.Property(e => e.SalarytypeId).HasColumnName("salarytype_id");
            entity.Property(e => e.Currency)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("currency");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.PaymentFrequency).HasColumnName("payment_frequency");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<ShiftAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__ShiftAss__DA8918142888CD30");

            entity.ToTable("ShiftAssignment");

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.AssignmentType)
                .HasMaxLength(50)
                .HasColumnName("assignment_type");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("status");

            entity.HasOne(d => d.Department).WithMany(p => p.ShiftAssignments)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__ShiftAssi__depar__344DAF97");

            entity.HasOne(d => d.Employee).WithMany(p => p.ShiftAssignments)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__ShiftAssi__emplo__33598B5E");

            entity.HasOne(d => d.Shift).WithMany(p => p.ShiftAssignments)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShiftAssi__shift__3541D3D0");
        });

        modelBuilder.Entity<ShiftCycle>(entity =>
        {
            entity.HasKey(e => e.CycleId).HasName("PK__ShiftCyc__5D955881E0924308");

            entity.ToTable("ShiftCycle");

            entity.Property(e => e.CycleId).HasColumnName("cycle_id");
            entity.Property(e => e.CycleDays).HasColumnName("cycle_days");
            entity.Property(e => e.CycleName)
                .HasMaxLength(100)
                .HasColumnName("cycle_name");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<ShiftCycleAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__ShiftCyc__DA89181433991F1F");

            entity.ToTable("ShiftCycleAssignment");

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.CycleId).HasColumnName("cycle_id");
            entity.Property(e => e.DaySequence).HasColumnName("day_sequence");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");

            entity.HasOne(d => d.Cycle).WithMany(p => p.ShiftCycleAssignments)
                .HasForeignKey(d => d.CycleId)
                .HasConstraintName("FK__ShiftCycl__cycle__5F380D9C");

            entity.HasOne(d => d.Shift).WithMany(p => p.ShiftCycleAssignments)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShiftCycl__shift__602C31D5");
        });

        modelBuilder.Entity<ShiftSchedule>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__ShiftSch__7B267220D1996EC8");

            entity.ToTable("ShiftSchedule");

            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.BreakDuration)
                .HasDefaultValue(0)
                .HasColumnName("break_duration");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.GracePeriodMinutes)
                .HasDefaultValue(0)
                .HasColumnName("grace_period_minutes");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ShiftDate).HasColumnName("shift_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Active")
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<SickLeave>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("PK__SickLeav__743350BC4AAC8559");

            entity.ToTable("SickLeave");

            entity.Property(e => e.LeaveId)
                .ValueGeneratedNever()
                .HasColumnName("leave_id");
            entity.Property(e => e.MedicalCertRequired)
                .HasDefaultValue(false)
                .HasColumnName("medical_cert_required");
            entity.Property(e => e.PhysicianId)
                .ValueGeneratedOnAdd()
                .HasColumnName("physician_id");

            entity.HasOne(d => d.Leave).WithOne(p => p.SickLeave)
                .HasForeignKey<SickLeave>(d => d.LeaveId)
                .HasConstraintName("FK__SickLeave__leave__6D862CF3");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Skill__FBBA83794FAD985D");

            entity.ToTable("Skill");

            entity.HasIndex(e => e.SkillName, "UQ__Skill__73C038ADB5121DC7").IsUnique();

            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.SkillName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("skill_name");
        });

        modelBuilder.Entity<TaxForm>(entity =>
        {
            entity.HasKey(e => e.TaxformId).HasName("PK__TaxForm__0C7E590CA25ED687");

            entity.ToTable("TaxForm");

            entity.HasIndex(e => e.FormName, "UQ__TaxForm__7790D91278A80CC3").IsUnique();

            entity.Property(e => e.TaxformId).HasColumnName("taxform_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.FormContent)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("form_content");
            entity.Property(e => e.FormName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("form_name");
            entity.Property(e => e.Jurisdiction)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("jurisdiction");
            entity.Property(e => e.ValidityPeriod).HasColumnName("validity_period");
        });

        modelBuilder.Entity<VacationLeave>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("PK__Vacation__743350BCC5B7731C");

            entity.ToTable("VacationLeave");

            entity.Property(e => e.LeaveId)
                .ValueGeneratedNever()
                .HasColumnName("leave_id");
            entity.Property(e => e.ApprovingManager)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("approving_manager");
            entity.Property(e => e.CarryOverDays).HasColumnName("carry_over_days");

            entity.HasOne(d => d.Leave).WithOne(p => p.VacationLeave)
                .HasForeignKey<VacationLeave>(d => d.LeaveId)
                .HasConstraintName("FK__VacationL__leave__69B59C0F");
        });

        modelBuilder.Entity<Verification>(entity =>
        {
            entity.HasKey(e => e.VerificationId).HasName("PK__Verifica__24F179695F1DC911");

            entity.ToTable("Verification");

            entity.HasIndex(e => e.VerificationType, "UQ__Verifica__B8E05C90B5D96CF1").IsUnique();

            entity.Property(e => e.VerificationId).HasColumnName("verification_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.ExpiryPeriod).HasColumnName("expiry_period");
            entity.Property(e => e.IssueDate).HasColumnName("issue_date");
            entity.Property(e => e.Issuer)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("issuer");
            entity.Property(e => e.VerificationType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("verification_type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
