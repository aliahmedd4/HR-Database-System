

    CREATE DATABASE HR_Payroll_System;

GO







CREATE TABLE Role (
    role_id INT PRIMARY KEY IDENTITY(1,1),
    role_name VARCHAR(100) NOT NULL UNIQUE,
    purpose VARCHAR(500),
    created_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Department(
    department_id INT PRIMARY KEY IDENTITY(1,1),
    department_name VARCHAR(100) NOT NULL UNIQUE,
    purpose VARCHAR(500),
     department_head_id INT,
    created_at DATETIME DEFAULT GETDATE()
);
GO
CREATE TABLE Contract(
    contract_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    contract_type VARCHAR(50) NOT NULL CHECK (contract_type IN ('FullTime', 'PartTime', 'Consultant', 'Internship')),
    start_date DATE NOT NULL,
    end_date DATE,
    type VARCHAR(200),
    current_state VARCHAR(50) DEFAULT 'Active' CHECK (current_state IN ('Active', 'Expired', 'Terminated', 'Renewed')),
    created_at DATETIME DEFAULT GETDATE(),

    CHECK (end_date IS NULL OR end_date >= start_date)
);
GO

CREATE TABLE Position (
    position_id INT PRIMARY KEY IDENTITY(1,1),
    position_title VARCHAR(100) NOT NULL UNIQUE,
    responsibilities VARCHAR(500),
    status VARCHAR(500),
    created_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE PayGrade (
    pay_grade_id INT PRIMARY KEY IDENTITY(1,1),
    grade_name VARCHAR(50) NOT NULL UNIQUE,
    min_salary DECIMAL(10,2) NOT NULL CHECK (min_salary >= 0),
    max_salary DECIMAL(10,2) NOT NULL CHECK (max_salary >= 0),
    CHECK (max_salary >= min_salary),
    created_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Currency (
    currency_id INT PRIMARY KEY IDENTITY(1,1),
    currency_code VARCHAR(3) NOT NULL UNIQUE,
    currency_name VARCHAR(100) NOT NULL,
    exchange_rate DECIMAL(10,4) DEFAULT 1.0 CHECK (exchange_rate > 0),
    CreatedDate DATE,
    LastUpdated DATE,

);
GO

CREATE TABLE SalaryType (
    salarytype_id INT PRIMARY KEY IDENTITY(1,1),
    type VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(500),
    payment_frequency INT,
    currency DECIMAL(10,4),

);
GO
CREATE TABLE HourlySalaryType (
    hourly_salary_type_id INT PRIMARY KEY IDENTITY(1,1),
    salary_type_id INT NOT NULL,
    hourly_rate DECIMAL(10,2) NOT NULL,
    max_monthly_hours INT CHECK (max_monthly_hours > 0),
    FOREIGN KEY (salary_type_id) REFERENCES SalaryType(salarytype_id)
);
GO
CREATE TABLE MonthlySalaryType (
    monthly_salary_type_id INT PRIMARY KEY IDENTITY(1,1),
    salary_type_id INT NOT NULL,
    tax_rule VARCHAR(100),
    contribution_scheme VARCHAR(100),
    FOREIGN KEY (salary_type_id) REFERENCES SalaryType(salarytype_id)
);
GO
CREATE TABLE ContractSalaryType (
    contract_salary_type_id INT PRIMARY KEY IDENTITY(1,1),
    salary_type_id INT NOT NULL,
    contract_value DECIMAL(12,2) NOT NULL,
    installment_details VARCHAR(255),
    FOREIGN KEY (salary_type_id) REFERENCES SalaryType(salarytype_id)
);
GO

CREATE TABLE TaxForm (
    taxform_id INT PRIMARY KEY IDENTITY(1,1),
    form_name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500),
    jurisdiction VARCHAR(300),
     validity_period DATE,
     form_content VARCHAR(500),

);
GO


CREATE TABLE Employee (
    employee_id INT PRIMARY KEY IDENTITY(1,1),
    employee_code NVARCHAR(50) NOT NULL UNIQUE,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    full_name NVARCHAR(200) NOT NULL,
    email NVARCHAR(255) NOT NULL UNIQUE,
    phone NVARCHAR(20),
    national_id NVARCHAR(50) UNIQUE,
    date_of_birth DATE,
    country_of_birth NVARCHAR(100),
    hire_date DATE NOT NULL,
    termination_date DATE,
    department_id INT,
    position_id INT,
    manager_id INT,
    contract_id INT,
    pay_grade_id INT,   
    salary_type_id INT,
    currency_id INT,
    tax_form_id INT,
    base_salary DECIMAL(10,2) CHECK (base_salary >= 0),
    is_active BIT DEFAULT 1,
    profile_completion BIT DEFAULT 0,
    profile_image VARCHAR(255),
    emergency_contact_name NVARCHAR(200),
    emergency_contact_phone NVARCHAR(20),
    relationship NVARCHAR(50),
    biography NVARCHAR(MAX),
    employment_progress NVARCHAR(100),
    account_status NVARCHAR(50),
    employment_status NVARCHAR(50),
    address NVARCHAR(500),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (department_id) REFERENCES Department(department_id),
    FOREIGN KEY (position_id) REFERENCES Position(position_id),
    
    
    FOREIGN KEY (contract_id) REFERENCES Contract(contract_id),
    
    CHECK (termination_date IS NULL OR termination_date >= hire_date)
);
GO





CREATE TABLE Employee_Role (
    employee_role_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    role_id INT NOT NULL,
    assigned_date DATETIME DEFAULT GETDATE(),
    is_active BIT DEFAULT 1,
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES Role(role_id),
    UNIQUE (employee_id, role_id)
);
GO

CREATE TABLE RolePermission (
    permission_id INT PRIMARY KEY IDENTITY(1,1),
    role_id INT NOT NULL,
    permission_name VARCHAR(100) NOT NULL,
    permission_value VARCHAR(50),
    allowed_action VARCHAR(500),
    FOREIGN KEY (role_id) REFERENCES Role(role_id) ON DELETE CASCADE,
    UNIQUE (role_id, permission_name)
);
GO





CREATE TABLE FullTimeContract (
    contract_id INT PRIMARY KEY,
    leave_entitlement VARCHAR(200),
     insurance_eligibility VARCHAR(200),
    weekly_working_hours INT,
    benefits NVARCHAR(500),
    FOREIGN KEY (contract_id) REFERENCES Contract(contract_id) ON DELETE CASCADE
);
GO

CREATE TABLE PartTimeContract (
    contract_id INT PRIMARY KEY,
    working_hours INT CHECK (working_hours > 0 AND working_hours < 40),
    schedule NVARCHAR(200),
    hourly_rate INT,
    FOREIGN KEY (contract_id) REFERENCES Contract(contract_id) ON DELETE CASCADE
);
GO


CREATE TABLE ConsultantContract (
    contract_id INT PRIMARY KEY,
    project_scope VARCHAR(200),
    fees INT,
    payment_schedule DATETIME,
    hourly_rate DECIMAL(10,2) CHECK (hourly_rate >= 0),
    max_hours_per_month INT CHECK (max_hours_per_month > 0),
    FOREIGN KEY (contract_id) REFERENCES Contract(contract_id) ON DELETE CASCADE
);
GO


CREATE TABLE InternshipContract (
    contract_id INT PRIMARY KEY,
    supervisor_id INT,
    stipend_related DECIMAL(10,2) CHECK (stipend_related >= 0),
    learning_objectives VARCHAR(500),
    evaluation VARCHAR(200),
     mentoring VARCHAR(200),
    FOREIGN KEY (contract_id) REFERENCES Contract(contract_id) ON DELETE CASCADE,
    FOREIGN KEY (supervisor_id) REFERENCES Employee(employee_id)
);
GO


CREATE TABLE Skill (
    skill_id INT PRIMARY KEY IDENTITY(1,1),
    skill_name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500)
);
GO

CREATE TABLE Employee_Skill (
    employee_skill_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    skill_id INT NOT NULL,
    proficiency_level VARCHAR(50) CHECK (proficiency_level IN ('Beginner', 'Intermediate', 'Advanced', 'Expert')),
    verified_date DATE,
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id) ON DELETE CASCADE,
    FOREIGN KEY (skill_id) REFERENCES Skill(skill_id),
    UNIQUE (employee_id, skill_id)
);
GO

CREATE TABLE Verification (
    verification_id INT PRIMARY KEY IDENTITY(1,1),
    verification_type VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500),
    issuer VARCHAR(200),
     issue_date DATE,
     expiry_period DATE,


);
GO

CREATE TABLE Employee_Verification (
    employee_verification_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    verification_id INT NOT NULL,
    verification_date DATE,
    expiry_date DATE,
    document_path NVARCHAR(500),
    status NVARCHAR(50) DEFAULT 'Valid' CHECK (status IN ('Valid', 'Expired', 'Pending')),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id) ON DELETE CASCADE,
    FOREIGN KEY (verification_id) REFERENCES Verification(verification_id),
    CHECK (expiry_date IS NULL OR expiry_date >= verification_date)
);
GO

CREATE TABLE ShiftSchedule (
    shift_id INT PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) CHECK (type IN ('Normal', 'Split', 'Overnight', 'Mission', 'Rotational')),
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    break_duration INT DEFAULT 0 CHECK (break_duration >= 0),
    shift_date DATE,
    status VARCHAR(50) DEFAULT 'Active' CHECK (status IN ('Active', 'Inactive', 'Expired')),
    is_active BIT DEFAULT 1,
    grace_period_minutes INT DEFAULT 0 CHECK (grace_period_minutes >= 0),
    created_at DATETIME DEFAULT GETDATE(),
    CHECK (end_time > start_time)
);
GO


CREATE TABLE ShiftAssignment (
    assignment_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT,
    department_id INT,
    shift_id INT NOT NULL,
    assignment_type NVARCHAR(50) CHECK (assignment_type IN ('Employee', 'Department')),
    start_date DATE NOT NULL,
    end_date DATE,
    is_active BIT DEFAULT 1,
    status VARCHAR(100),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (department_id) REFERENCES Department(department_id),
    FOREIGN KEY (shift_id) REFERENCES ShiftSchedule(shift_id),
    CHECK (end_date IS NULL OR end_date >= start_date),
    CHECK ((employee_id IS NOT NULL AND department_id IS NULL) OR (employee_id IS NULL AND department_id IS NOT NULL))
);
GO

CREATE TABLE Attendance (
    attendance_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    attendance_date DATE NOT NULL,
    shift_id INT,
    entry_time DATETIME,
    exit_time DATETIME,
    duration TIME,
    login_method VARCHAR(50),
    logout_method VARCHAR(50),
    exception_id INT,
    status NVARCHAR(50) DEFAULT 'Present' CHECK (status IN ('Present', 'Absent', 'Late', 'EarlyLeave', 'OnTime', 'OnLeave')),
    hours_worked DECIMAL(5,2) CHECK (hours_worked >= 0),
    overtime_hours DECIMAL(5,2) DEFAULT 0 CHECK (overtime_hours >= 0),
    lateness_minutes INT DEFAULT 0 CHECK (lateness_minutes >= 0),
    source_type NVARCHAR(50) CHECK (source_type IN ('GPS', 'Terminal', 'Manual', 'Offline', 'Leave')),
    source_id NVARCHAR(100),
    leave_request_id INT,
    is_leave_exception BIT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (shift_id) REFERENCES ShiftSchedule(shift_id),
    UNIQUE (employee_id, attendance_date),
    
);
GO

CREATE TABLE AttendanceLog (
    attendance_log_id INT PRIMARY KEY IDENTITY(1,1),
    attendance_id INT,
    employee_id INT NOT NULL,
    action_type VARCHAR(50) CHECK (action_type IN ('Create', 'Update', 'Delete', 'Correction')),
    old_value VARCHAR(500),
    new_value VARCHAR(500),
    changed_by INT,
    reason VARCHAR(500),
    actor VARCHAR(100),
    time_stamp TIME,
    changed_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (attendance_id) REFERENCES Attendance(attendance_id) ON DELETE CASCADE,
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (changed_by) REFERENCES Employee(employee_id),
    
);
GO

CREATE TABLE AttendanceCorrectionRequest (
    request_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    attendance_date DATE NOT NULL,
    requested_check_in DATETIME,
    requested_check_out DATETIME,
    reason VARCHAR(500),
    correction_type VARCHAR(200),
    status VARCHAR(50) DEFAULT 'Pending' CHECK (status IN ('Pending', 'Approved', 'Rejected')),
    recorded_by VARCHAR(200),
    date DATE,
    review_notes NVARCHAR(500),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    
);
GO

CREATE TABLE AttendanceSource (
    attendace_id INT PRIMARY KEY IDENTITY(1,1),
    device_id INT ,
    source_type VARCHAR(50) NOT NULL CHECK (source_type IN ('GPS', 'Terminal')),
    source_identifier VARCHAR(100) NOT NULL,
    location_name VARCHAR(200),
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    is_active BIT DEFAULT 1,
    recorded_at DATETIME DEFAULT GETDATE(),
    UNIQUE (source_type, source_identifier),
    
    FOREIGN KEY (device_id) REFERENCES Device(device_id),
    

);
GO

CREATE TABLE ShiftCycle (
    cycle_id INT PRIMARY KEY IDENTITY(1,1),
    cycle_name NVARCHAR(100) NOT NULL,
    cycle_days INT NOT NULL CHECK (cycle_days > 0),
    description NVARCHAR(500),
    is_active BIT DEFAULT 1
);
GO

CREATE TABLE ShiftCycleAssignment (
    assignment_id INT PRIMARY KEY IDENTITY(1,1),
    cycle_id INT NOT NULL,
    shift_id INT NOT NULL,
    order_number INT,
    day_sequence INT NOT NULL CHECK (day_sequence > 0),
    FOREIGN KEY (cycle_id) REFERENCES ShiftCycle(cycle_id) ON DELETE CASCADE,
    FOREIGN KEY (shift_id) REFERENCES ShiftSchedule(shift_id)
);
GO


CREATE TABLE Leave (
    leave_id INT PRIMARY KEY IDENTITY(1,1),
    leave_type VARCHAR(50) NOT NULL CHECK (leave_type IN ('Vacation', 'Sick', 'Probation', 'Holiday', 'Medical', 'Special')),
    leave_description VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    is_paid BIT DEFAULT 1,
    max_days_per_year INT CHECK (max_days_per_year >= 0),
    requires_approval BIT DEFAULT 1,
    is_active BIT DEFAULT 1
);
GO


CREATE TABLE VacationLeave (
    leave_id INT PRIMARY KEY,
   carry_over_days INT,
    approving_manager VARCHAR(200),
    FOREIGN KEY (leave_id) REFERENCES Leave(leave_id) ON DELETE CASCADE
);
GO

CREATE TABLE SickLeave (
    leave_id INT PRIMARY KEY,
    medical_cert_required BIT DEFAULT 0,
   physician_id INT IDENTITY,
    FOREIGN KEY (leave_id) REFERENCES Leave(leave_id) ON DELETE CASCADE
);
GO

CREATE TABLE ProbationLeave (
    leave_id INT PRIMARY KEY,
    applicable_during_probation BIT DEFAULT 1,
    eligibility_start_date DATE,
    probation_period TIME,
    FOREIGN KEY (leave_id) REFERENCES Leave(leave_id) ON DELETE CASCADE
);
GO

CREATE TABLE HolidayLeave (
    leave_id INT PRIMARY KEY,
    holiday_name VARCHAR (200),
    official_recognition VARCHAR(200),
    regional_scope VARCHAR(200),
    is_national_holiday BIT DEFAULT 1,
    holiday_date DATE,
    FOREIGN KEY (leave_id) REFERENCES Leave(leave_id) ON DELETE CASCADE
);
GO

CREATE TABLE LeavePolicy (
    policy_id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    purpose VARCHAR(200),
    notice_period TIME,
    special_leave_type VARCHAR(200),
    eligibility_rules VARCHAR(500),
    reset_on_new_year BIT,
    leave_id INT NOT NULL,
    eligibility_months INT DEFAULT 0 CHECK (eligibility_months >= 0),
    accrual_rate DECIMAL(5,2) CHECK (accrual_rate >= 0 AND accrual_rate <= 100),
    max_balance DECIMAL(5,2) CHECK (max_balance >= 0),
    is_active BIT DEFAULT 1,
    effective_date DATE DEFAULT CAST(GETDATE() AS DATE),
    FOREIGN KEY (leave_id) REFERENCES Leave(leave_id)
);
GO

CREATE TABLE LeaveEntitlement (
    entitlement VARCHAR(200),
    employee_id INT NOT NULL,
    leave_id INT NOT NULL,
    year INT NOT NULL,
    allocated_days DECIMAL(5,2) DEFAULT 0 CHECK (allocated_days >= 0),
    used_days DECIMAL(5,2) DEFAULT 0 CHECK (used_days >= 0),
    balance_days DECIMAL(5,2) DEFAULT 0 CHECK (balance_days >= 0),
    carry_forward_days DECIMAL(5,2) DEFAULT 0 CHECK (carry_forward_days >= 0),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (leave_id ) REFERENCES Leave(leave_id),
    UNIQUE (employee_id, leave_id, year),
    CHECK (used_days <= allocated_days + carry_forward_days)
);
GO

CREATE TABLE LeaveRequest (
    request_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    leave_id INT NOT NULL,
    justification VARCHAR(200),
    duration_days DECIMAL(5,2) CHECK (duration_days > 0), 
    approval_timing DATE,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_days DECIMAL(5,2) NOT NULL CHECK (total_days > 0),
    reason VARCHAR(500),
    status VARCHAR(50),
    submitted_at DATETIME DEFAULT GETDATE(),
    approved_by INT,
    approved_at DATETIME,
    rejection_reason NVARCHAR(500),
    is_irregular BIT DEFAULT 0,
    linked_shift_id INT,
    override_reason VARCHAR(200),     
    overridden_at DATETIME,               
    document_verified BIT DEFAULT 0,      
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (leave_id) REFERENCES Leave(leave_id),
    FOREIGN KEY (approved_by) REFERENCES Employee(employee_id),
    FOREIGN KEY (linked_shift_id) REFERENCES ShiftSchedule(shift_id),
    CHECK (end_date >= start_date)
);
GO



CREATE TABLE LeaveDocument (
    document_id INT PRIMARY KEY IDENTITY(1,1),
    leave_request_id INT NOT NULL,
    document_type VARCHAR(50),
    file_path VARCHAR(500),
    uploaded_at DATE DEFAULT GETDATE(),
    FOREIGN KEY (leave_request_id) REFERENCES LeaveRequest(request_id) ON DELETE CASCADE
);
GO




CREATE TABLE Payroll (
    payroll_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    base_salary DECIMAL(10,2) NOT NULL,
    total_allowances DECIMAL(10,2),
    total_deductions DECIMAL(10,2),
    gross_salary DECIMAL(10,2),
    net_salary DECIMAL(10,2),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id)
);





GO

CREATE TABLE AllowanceDeduction (
    ad_id INT PRIMARY KEY IDENTITY(1,1),
    payroll_id INT NOT NULL,
    employee_id INT,
    type VARCHAR (200),
    currency VARCHAR(200),
    duration TIME,
    timezone DATE,
    item_type VARCHAR(50) NOT NULL CHECK (item_type IN ('Allowance', 'Deduction')),
    item_name VARCHAR(100) NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    percentage DECIMAL(5,2) CHECK (percentage >= 0 AND percentage <= 100),
    description VARCHAR(500),
    FOREIGN KEY (payroll_id) REFERENCES Payroll(payroll_id) ON DELETE CASCADE,
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id) ON DELETE CASCADE,
     


);
GO

CREATE TABLE PayrollPolicy (
    policy_id INT PRIMARY KEY IDENTITY(1,1),
    type VARCHAR(200),
    description VARCHAR(200),
    policy_name VARCHAR(100) NOT NULL,
    policy_type VARCHAR(50) ,
    is_active BIT DEFAULT 1,
    effective_date DATE DEFAULT CAST(GETDATE() AS DATE),

    
);
GO

CREATE TABLE OvertimePolicy (
    policy_id INT PRIMARY KEY,
    weekday_rate_multiplier DECIMAL(5,2),
     weekend_rate_multiplier DECIMAL(5,2),  
    min_hours_for_overtime DECIMAL(5,2) DEFAULT 0 CHECK (min_hours_for_overtime >= 0),
    max_hours_per_month DECIMAL(5,2),
    FOREIGN KEY (policy_id) REFERENCES PayrollPolicy(policy_id) ON DELETE CASCADE
);
GO

CREATE TABLE LatenessPolicy (
    policy_id INT PRIMARY KEY,
    grace_period_mins DECIMAL(5,2),
    deduction_rate DECIMAL (5,2),
    penalty_per_late_minute DECIMAL(10,2) DEFAULT 0 CHECK (penalty_per_late_minute >= 0),
    threshold_minutes INT DEFAULT 0 CHECK (threshold_minutes >= 0),
    max_penalty_per_day DECIMAL(10,2),
    FOREIGN KEY (policy_id) REFERENCES PayrollPolicy(policy_id) ON DELETE CASCADE
);
GO

CREATE TABLE BonusPolicy (
    policy_id INT PRIMARY KEY,
    bonus_type VARCHAR(50) CHECK (bonus_type IN ('Performance', 'Attendance', 'Signing', 'Annual')),
    bonus_amount DECIMAL(10,2) CHECK (bonus_amount >= 0),
    bonus_percentage DECIMAL(5,2) CHECK (bonus_percentage >= 0 AND bonus_percentage <= 100),
    eligibility_criteria VARCHAR(500),
    FOREIGN KEY (policy_id) REFERENCES PayrollPolicy(policy_id) ON DELETE CASCADE
);
GO

CREATE TABLE DeductionPolicy (
    policy_id INT PRIMARY KEY,
    deduction_reason VARCHAR(50),
    calculation_mode VARCHAR(200),
    deduction_amount DECIMAL(10,2) CHECK (deduction_amount >= 0),
    deduction_percentage DECIMAL(5,2) CHECK (deduction_percentage >= 0 AND deduction_percentage <= 100),
    applies_to_all BIT DEFAULT 0,
    FOREIGN KEY (policy_id) REFERENCES PayrollPolicy(policy_id) ON DELETE CASCADE
);
GO


CREATE TABLE PayrollPolicy_ID (
   payroll_id INT NOT NULL,
    policy_id INT NOT NULL,
    employee_id INT,
    department_id INT,
    FOREIGN KEY (policy_id) REFERENCES PayrollPolicy(policy_id),
    FOREIGN KEY (payroll_id) REFERENCES Payroll(payroll_id),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (department_id) REFERENCES Department(department_id),
    CHECK ((employee_id IS NOT NULL AND department_id IS NULL) OR (employee_id IS NULL AND department_id IS NOT NULL))
);
GO

CREATE TABLE Payroll_Log (
    log_id INT PRIMARY KEY IDENTITY(1,1),
    payroll_log_id INT,
    payroll_id INT,
    actor VARCHAR(200),
    modification_type VARCHAR(200),
    action_type VARCHAR(50) CHECK (action_type IN ('Create', 'Update', 'Finalize', 'Adjust')),
    old_value VARCHAR(500),
    new_value VARCHAR(500),
    changed_by INT,
    change_reason VARCHAR(500),
    changed_date DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (payroll_id) REFERENCES Payroll(payroll_id) ON DELETE CASCADE,
    FOREIGN KEY (changed_by) REFERENCES Employee(employee_id)
);
GO


CREATE TABLE Notification (
    notification_id INT PRIMARY KEY IDENTITY(1,1),
    notification_type VARCHAR(50) NOT NULL,
    timestamp TIME,
    urgency VARCHAR (200),
    read_status VARCHAR(200),
    title VARCHAR(200) NOT NULL,
    message_content VARCHAR(1000) NOT NULL,
    priority VARCHAR(20) DEFAULT 'Normal' CHECK (priority IN ('Low', 'Normal', 'High', 'Urgent')),
    created_at DATETIME DEFAULT GETDATE(),
    expires_at DATETIME
);
GO


CREATE TABLE Employee_Notification (
    employee_notification_id INT PRIMARY KEY IDENTITY(1,1),
    notification_id INT NOT NULL,
    employee_id INT NOT NULL,
    is_read BIT DEFAULT 0,
    delivery_status VARCHAR(200),
    delivered_at DATETIME,
    FOREIGN KEY (notification_id) REFERENCES Notification(notification_id) ON DELETE CASCADE,
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id) ON DELETE CASCADE
);
GO


CREATE TABLE EmployeeHierarchy (
    hierarchy_level INT,
    employee_id INT NOT NULL,
    manager_id INT NOT NULL,
    level INT NOT NULL CHECK (level >= 0),
    start_date DATE NOT NULL,
    end_date DATE,
    is_active BIT DEFAULT 1,
   
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (manager_id) REFERENCES Employee(employee_id),
    CHECK (end_date IS NULL OR end_date >= start_date)
);
GO

CREATE TABLE Device (
    device_id INT PRIMARY KEY IDENTITY(1,1),
    device_name VARCHAR(100) NOT NULL,
    terminal_id INT,
    latitude DECIMAL(10,2),
    longitude DECIMAL(10,2),
    employee_id INT NOT NULL,
    device_type VARCHAR(50) CHECK (device_type IN ('GPS', 'Terminal', 'Mobile')),
    device_identifier VARCHAR(100) NOT NULL UNIQUE,
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),

);
GO

CREATE TABLE ApprovalWorkflow (
    workflow_id INT PRIMARY KEY IDENTITY(1,1),
    workflow_name VARCHAR(100) NOT NULL,
    workflow_type VARCHAR(50),
    threshold_amount DECIMAL(10,2),
    approver_role VARCHAR(200),
    created_by VARCHAR(200),
    status VARCHAR(200),
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE ApprovalWorkflowStep (
    workflow_id INT NOT NULL,
    step_number INT,
    action_required VARCHAR(200),
    step_sequence INT NOT NULL CHECK (step_sequence > 0),
    role_id INT,
    approver_department_id INT,
    is_required BIT DEFAULT 1,
    FOREIGN KEY (workflow_id) REFERENCES ApprovalWorkflow(workflow_id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES Role(role_id),
    FOREIGN KEY (approver_department_id) REFERENCES Department(department_id)
);
GO


CREATE TABLE ManagerNotes (
    note_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    manager_id INT NOT NULL,
    note_type VARCHAR(50) CHECK (note_type IN ('Performance', 'Attendance', 'General', 'Disciplinary')),
    note_content VARCHAR(2000) NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (manager_id) REFERENCES Employee(employee_id)
);
GO


CREATE TABLE Mission (
    mission_id INT PRIMARY KEY IDENTITY(1,1),
    destination VARCHAR(200),
    manager_id INT,
    employee_id INT NOT NULL,
    mission_name NVARCHAR(200) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    location NVARCHAR(200),
    status NVARCHAR(50) DEFAULT 'Assigned' CHECK (status IN ('Assigned', 'InProgress', 'Completed', 'Cancelled')),
    assigned_by INT,
    completed_at DATETIME,
    FOREIGN KEY (manager_id) REFERENCES Employee(employee_id),
     FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
    FOREIGN KEY (assigned_by) REFERENCES Employee(employee_id),
    CHECK (end_date IS NULL OR end_date >= start_date)
);
GO
CREATE TABLE Reimbursement (
    reimbursement_id INT PRIMARY KEY IDENTITY(1,1),
    employee_id INT NOT NULL,
    type VARCHAR(200),
    claim_type VARCHAR(200),
    approval_date DATE,
    current_status VARCHAR(50),
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id),
   
);
GO

SELECT contract_type, COUNT(*) AS count
FROM Contract
GROUP BY contract_type
HAVING COUNT(*) > 1;

DELETE FROM Contract
WHERE contract_id NOT IN (
    SELECT MIN(contract_id)
    FROM Contract
    GROUP BY contract_type


    );



