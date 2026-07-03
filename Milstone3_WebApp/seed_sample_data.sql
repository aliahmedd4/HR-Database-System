/* ================================================================
   HRMS — Sample data seed
   Re-runnable: each section guarded by IF NOT EXISTS.
   Run AFTER table.sql, add_password_hash_column.sql, data.sql.
   ================================================================ */
USE HR_Payroll_System;
GO

/* ---- 0. Align Payroll table with the EF model (adds missing cols) ---- */
IF COL_LENGTH('Payroll','currency_id')    IS NULL ALTER TABLE Payroll ADD currency_id   INT           NULL;
IF COL_LENGTH('Payroll','period_id')      IS NULL ALTER TABLE Payroll ADD period_id     INT           NULL;
IF COL_LENGTH('Payroll','period_start')   IS NULL ALTER TABLE Payroll ADD period_start  DATE          NULL;
IF COL_LENGTH('Payroll','period_end')     IS NULL ALTER TABLE Payroll ADD period_end    DATE          NULL;
IF COL_LENGTH('Payroll','base_amount')    IS NULL ALTER TABLE Payroll ADD base_amount   DECIMAL(10,2) NULL;
IF COL_LENGTH('Payroll','taxes')          IS NULL ALTER TABLE Payroll ADD taxes         DECIMAL(10,4) NULL;
IF COL_LENGTH('Payroll','actual_pay')     IS NULL ALTER TABLE Payroll ADD actual_pay    DECIMAL(10,2) NULL;
IF COL_LENGTH('Payroll','payment_date')   IS NULL ALTER TABLE Payroll ADD payment_date  DATE          NULL;
IF COL_LENGTH('Payroll','hours_worked')   IS NULL ALTER TABLE Payroll ADD hours_worked  DECIMAL(5,2)  NULL;
IF COL_LENGTH('Payroll','overtime_hours') IS NULL ALTER TABLE Payroll ADD overtime_hours DECIMAL(5,2) NULL;
IF COL_LENGTH('Payroll','status')         IS NULL ALTER TABLE Payroll ADD status        VARCHAR(50)   NULL;
IF COL_LENGTH('Payroll','adjustments')    IS NULL ALTER TABLE Payroll ADD adjustments   VARCHAR(200)  NULL;
IF COL_LENGTH('Payroll','contributions')  IS NULL ALTER TABLE Payroll ADD contributions VARCHAR(200)  NULL;
GO

/* ================================================================
   Main data batch (columns above now recognized)
   ================================================================ */
DECLARE @today DATE = CAST(GETDATE() AS DATE);
DECLARE @yr    INT  = YEAR(GETDATE());

/* ---- 1. CURRENCY ---- */
IF NOT EXISTS (SELECT 1 FROM Currency WHERE currency_code = 'EGP')
    INSERT INTO Currency (currency_code, currency_name, exchange_rate, CreatedDate, LastUpdated)
    VALUES ('EGP', 'Egyptian Pound', 1.0, GETDATE(), GETDATE());
DECLARE @cur INT = (SELECT TOP 1 currency_id FROM Currency WHERE currency_code = 'EGP');

/* ---- 2. ORGANIZATION: Departments ---- */
IF NOT EXISTS (SELECT 1 FROM Department)
    INSERT INTO Department (department_name, purpose, created_at) VALUES
        ('Engineering',      'Software development and QA',            GETDATE()),
        ('Human Resources',  'People operations and recruitment',     GETDATE()),
        ('Finance',          'Payroll, accounting and analysis',      GETDATE()),
        ('Operations',       'Sales, marketing and daily operations', GETDATE());

DECLARE @eng INT = (SELECT department_id FROM Department WHERE department_name = 'Engineering');
DECLARE @hr  INT = (SELECT department_id FROM Department WHERE department_name = 'Human Resources');
DECLARE @fin INT = (SELECT department_id FROM Department WHERE department_name = 'Finance');
DECLARE @ops INT = (SELECT department_id FROM Department WHERE department_name = 'Operations');

/* Assign employees to departments + reporting managers
   Heads: 2 Mohamed(Eng), 4 Fatima(HR), 5 Omar(Fin), 8 Mona(Ops) */
UPDATE Employee SET department_id = @eng, manager_id = 2    WHERE employee_id IN (1, 3, 10);
UPDATE Employee SET department_id = @eng, manager_id = NULL WHERE employee_id = 2;
UPDATE Employee SET department_id = @hr,  manager_id = NULL WHERE employee_id = 4;
UPDATE Employee SET department_id = @fin, manager_id = 5    WHERE employee_id = 9;
UPDATE Employee SET department_id = @fin, manager_id = NULL WHERE employee_id = 5;
UPDATE Employee SET department_id = @ops, manager_id = 8    WHERE employee_id IN (6, 7);
UPDATE Employee SET department_id = @ops, manager_id = NULL WHERE employee_id = 8;

UPDATE Department SET department_head_id = 2 WHERE department_id = @eng;
UPDATE Department SET department_head_id = 4 WHERE department_id = @hr;
UPDATE Department SET department_head_id = 5 WHERE department_id = @fin;
UPDATE Department SET department_head_id = 8 WHERE department_id = @ops;

/* ---- 3. CONTRACTS (two expiring within 30 days) ---- */
IF NOT EXISTS (SELECT 1 FROM Contract)
BEGIN
    INSERT INTO Contract (employee_id, contract_type, start_date, end_date, type, current_state, created_at) VALUES
        (1,  'FullTime',   '2023-01-15', '2026-01-14',              'Permanent',  'Active', GETDATE()),
        (2,  'FullTime',   '2022-06-01', '2026-05-31',              'Permanent',  'Active', GETDATE()),
        (3,  'FullTime',   '2023-03-10', DATEADD(DAY,20,@today),    'Fixed-term', 'Active', GETDATE()),
        (4,  'FullTime',   '2021-09-01', '2026-08-31',              'Permanent',  'Active', GETDATE()),
        (5,  'FullTime',   '2023-05-20', '2026-05-19',              'Permanent',  'Active', GETDATE()),
        (6,  'PartTime',   '2023-07-15', DATEADD(DAY,12,@today),    'Fixed-term', 'Active', GETDATE()),
        (7,  'FullTime',   '2022-11-01', '2026-10-31',              'Permanent',  'Active', GETDATE()),
        (8,  'FullTime',   '2021-12-10', '2026-12-09',              'Permanent',  'Active', GETDATE()),
        (9,  'Internship', '2023-09-01', '2026-08-31',              'Fixed-term', 'Active', GETDATE()),
        (10, 'FullTime',   '2023-02-14', '2026-02-13',              'Permanent',  'Active', GETDATE());

    UPDATE e SET e.contract_id = c.contract_id
    FROM Employee e JOIN Contract c ON c.employee_id = e.employee_id;
END

/* ---- 4. SHIFTS ---- */
IF NOT EXISTS (SELECT 1 FROM ShiftSchedule)
    INSERT INTO ShiftSchedule (name, type, start_time, end_time, break_duration, status, is_active, grace_period_minutes, created_at) VALUES
        ('Morning Shift', 'Standard',   '09:00', '17:00', 60, 'Active', 1, 15, GETDATE()),
        ('Evening Shift', 'Standard',   '14:00', '22:00', 45, 'Active', 1, 15, GETDATE()),
        ('Night Shift',   'Rotational', '22:00', '06:00', 60, 'Active', 1, 10, GETDATE()),
        ('Split Shift',   'Split',      '08:00', '12:00',  0, 'Active', 1, 10, GETDATE());

DECLARE @morning INT = (SELECT shift_id FROM ShiftSchedule WHERE name = 'Morning Shift');
DECLARE @evening INT = (SELECT shift_id FROM ShiftSchedule WHERE name = 'Evening Shift');
DECLARE @night   INT = (SELECT shift_id FROM ShiftSchedule WHERE name = 'Night Shift');

/* ---- 5. SHIFT ASSIGNMENTS ---- */
IF NOT EXISTS (SELECT 1 FROM ShiftAssignment)
    INSERT INTO ShiftAssignment (employee_id, department_id, shift_id, assignment_type, start_date, end_date, is_active, status) VALUES
        (1,  NULL, @morning, 'Individual', @today, NULL, 1, 'Active'),
        (2,  NULL, @morning, 'Individual', @today, NULL, 1, 'Active'),
        (3,  NULL, @morning, 'Individual', @today, NULL, 1, 'Active'),
        (6,  NULL, @evening, 'Individual', @today, NULL, 1, 'Active'),
        (7,  NULL, @evening, 'Individual', @today, NULL, 1, 'Active'),
        (9,  NULL, @night,   'Individual', @today, NULL, 1, 'Active'),
        (10, NULL, @morning, 'Individual', @today, NULL, 1, 'Active'),
        (NULL, @ops, @evening, 'Department', @today, NULL, 1, 'Active');

/* ---- 6. LEAVE TYPES ---- */
IF NOT EXISTS (SELECT 1 FROM Leave)
    INSERT INTO Leave (leave_type, leave_description, description, is_paid, max_days_per_year, requires_approval, is_active) VALUES
        ('Vacation',  'Annual paid vacation leave', 'Annual leave',    1, 21, 1, 1),
        ('Sick',      'Paid sick leave',            'Sick leave',      1, 14, 1, 1),
        ('Personal',  'Personal time off',          'Personal leave',  0,  5, 1, 1),
        ('Maternity', 'Maternity leave',            'Maternity leave', 1, 90, 1, 1),
        ('Emergency', 'Emergency leave',            'Emergency leave', 1,  3, 1, 1);

DECLARE @vac  INT = (SELECT leave_id FROM Leave WHERE leave_type = 'Vacation');
DECLARE @sick INT = (SELECT leave_id FROM Leave WHERE leave_type = 'Sick');

/* ---- 7. LEAVE ENTITLEMENTS (Vacation, Sick, Personal per employee) ---- */
IF NOT EXISTS (SELECT 1 FROM LeaveEntitlement)
    INSERT INTO LeaveEntitlement (entitlement, employee_id, leave_id, year, allocated_days, used_days, balance_days, carry_forward_days)
    SELECT NULL, e.employee_id, l.leave_id, @yr, l.max_days_per_year, 0, l.max_days_per_year, 0
    FROM Employee e CROSS JOIN Leave l
    WHERE l.leave_type IN ('Vacation', 'Sick', 'Personal');

/* Reflect a couple of used balances for realism */
UPDATE LeaveEntitlement SET used_days = 2, balance_days = allocated_days - 2 WHERE employee_id = 3 AND leave_id = @sick AND year = @yr;
UPDATE LeaveEntitlement SET used_days = 5, balance_days = allocated_days - 5 WHERE employee_id = 1 AND leave_id = @vac  AND year = @yr;

/* ---- 8. LEAVE REQUESTS (mixed statuses) ---- */
IF NOT EXISTS (SELECT 1 FROM LeaveRequest)
    INSERT INTO LeaveRequest (employee_id, leave_id, start_date, end_date, total_days, reason, status, submitted_at, approved_by, approved_at, is_irregular) VALUES
        (1,  @vac,  DATEADD(DAY, 5,@today),  DATEADD(DAY, 9,@today), 5,  'Family trip',          'Pending',  GETDATE(),                  NULL, NULL,                        0),
        (3,  @sick, DATEADD(DAY,-3,@today),  DATEADD(DAY,-2,@today), 2,  'Flu recovery',         'Approved', DATEADD(DAY,-4,GETDATE()),  4,    DATEADD(DAY,-3,GETDATE()),   0),
        (6,  @vac,  DATEADD(DAY,10,@today),  DATEADD(DAY,20,@today), 11, 'Extended family leave','Pending',  GETDATE(),                  NULL, NULL,                        1),
        (7,  @vac,  DATEADD(DAY,-10,@today), DATEADD(DAY,-8,@today), 3,  'Personal matters',     'Rejected', DATEADD(DAY,-12,GETDATE()), 8,    DATEADD(DAY,-11,GETDATE()),  0),
        (10, @sick, DATEADD(DAY, 2,@today),  DATEADD(DAY, 3,@today), 2,  'Medical appointment',  'Pending',  GETDATE(),                  NULL, NULL,                        0);

/* ---- 9. ATTENDANCE (recent days, mixed statuses) ---- */
IF NOT EXISTS (SELECT 1 FROM Attendance)
    INSERT INTO Attendance (employee_id, attendance_date, shift_id, entry_time, exit_time, hours_worked, overtime_hours, lateness_minutes, status, source_type, created_at) VALUES
        (1,  @today,               @morning, DATEADD(HOUR,9,  CAST(@today AS DATETIME)),               DATEADD(HOUR,17,CAST(@today AS DATETIME)),               8, 0, 0,  'Present', 'Web', GETDATE()),
        (2,  @today,               @morning, DATEADD(HOUR,9,  CAST(@today AS DATETIME)),               DATEADD(HOUR,18,CAST(@today AS DATETIME)),               9, 1, 0,  'Present', 'Web', GETDATE()),
        (3,  @today,               @morning, DATEADD(MINUTE,565,CAST(@today AS DATETIME)),             DATEADD(HOUR,17,CAST(@today AS DATETIME)),               7, 0, 25, 'Present', 'Web', GETDATE()),
        (10, @today,               @morning, DATEADD(HOUR,9,  CAST(@today AS DATETIME)),               DATEADD(HOUR,17,CAST(@today AS DATETIME)),               8, 0, 0,  'Present', 'Web', GETDATE()),
        (1,  DATEADD(DAY,-1,@today),@morning, DATEADD(HOUR,9, CAST(DATEADD(DAY,-1,@today) AS DATETIME)),DATEADD(HOUR,17,CAST(DATEADD(DAY,-1,@today) AS DATETIME)),8, 0, 0,  'Present', 'Web', GETDATE()),
        (2,  DATEADD(DAY,-1,@today),@morning, DATEADD(HOUR,9, CAST(DATEADD(DAY,-1,@today) AS DATETIME)),DATEADD(HOUR,17,CAST(DATEADD(DAY,-1,@today) AS DATETIME)),8, 0, 0,  'Present', 'Web', GETDATE()),
        (3,  DATEADD(DAY,-2,@today),NULL,     NULL,                                                    NULL,                                                    0, 0, 0,  'OnLeave', 'Leave', GETDATE()),
        (6,  DATEADD(DAY,-1,@today),@evening, DATEADD(HOUR,14,CAST(DATEADD(DAY,-1,@today) AS DATETIME)),DATEADD(HOUR,22,CAST(DATEADD(DAY,-1,@today) AS DATETIME)),8, 0, 0,  'Present', 'Web', GETDATE()),
        (7,  DATEADD(DAY,-1,@today),NULL,     NULL,                                                    NULL,                                                    0, 0, 0,  'Absent',  'Web', GETDATE());

/* ---- 10. ATTENDANCE CORRECTION REQUESTS (mixed statuses) ---- */
IF NOT EXISTS (SELECT 1 FROM AttendanceCorrectionRequest)
    INSERT INTO AttendanceCorrectionRequest (employee_id, attendance_date, requested_check_in, requested_check_out, reason, correction_type, status, recorded_by, [date], review_notes) VALUES
        (1,  DATEADD(DAY,-1,@today), DATEADD(HOUR,9, CAST(DATEADD(DAY,-1,@today) AS DATETIME)), DATEADD(HOUR,17,CAST(DATEADD(DAY,-1,@today) AS DATETIME)), 'Forgot to check in', 'Missing Check-in',  'Pending',  'Self', @today, NULL),
        (2,  DATEADD(DAY,-2,@today), DATEADD(HOUR,9, CAST(DATEADD(DAY,-2,@today) AS DATETIME)), DATEADD(HOUR,18,CAST(DATEADD(DAY,-2,@today) AS DATETIME)), 'Biometric error',    'Wrong Time',        'Approved', 'Self', @today, 'Verified with logs'),
        (10, DATEADD(DAY,-3,@today), DATEADD(HOUR,9, CAST(DATEADD(DAY,-3,@today) AS DATETIME)), DATEADD(HOUR,16,CAST(DATEADD(DAY,-3,@today) AS DATETIME)), 'Left early',         'Missing Check-out', 'Rejected', 'Self', @today, 'No prior approval');

/* ---- 11. MISSIONS (mixed statuses) ---- */
IF NOT EXISTS (SELECT 1 FROM Mission)
    INSERT INTO Mission (destination, manager_id, employee_id, mission_name, start_date, end_date, location, status, assigned_by, completed_at) VALUES
        ('Dubai',    2, 1,  'Client onboarding', DATEADD(DAY, 7,@today), DATEADD(DAY,10,@today), 'UAE',   'Assigned',  4, NULL),
        ('Riyadh',   8, 7,  'Sales conference',  DATEADD(DAY,-15,@today),DATEADD(DAY,-12,@today),'KSA',   'Completed', 4, DATEADD(DAY,-11,GETDATE())),
        ('Cairo HQ', 4, 3,  'Product training',  DATEADD(DAY, 3,@today), DATEADD(DAY, 4,@today), 'Egypt', 'Pending',   4, NULL),
        ('London',   2, 10, 'QA audit',          DATEADD(DAY,20,@today), DATEADD(DAY,25,@today), 'UK',    'Approved',  4, NULL);

/* ---- 12. NOTIFICATIONS + delivery ---- */
IF NOT EXISTS (SELECT 1 FROM Notification)
BEGIN
    INSERT INTO Notification (notification_type, urgency, read_status, title, message_content, priority, created_at) VALUES
        ('General',  'Low',    'Unread', 'Welcome to HRMS',        'Your account is now active. Explore your dashboard.',                'Low',    GETDATE()),
        ('Contract', 'High',   'Unread', 'Contract expiring soon', 'Your employment contract expires within 30 days. Contact HR to renew.','High', GETDATE()),
        ('Leave',    'Normal', 'Unread', 'Leave request approved', 'Your sick leave request has been approved.',                          'Normal', GETDATE()),
        ('Mission',  'Normal', 'Unread', 'New mission assigned',   'You have been assigned to Client onboarding in Dubai.',               'Normal', GETDATE());

    DECLARE @nWelcome  INT = (SELECT notification_id FROM Notification WHERE title = 'Welcome to HRMS');
    DECLARE @nContract INT = (SELECT notification_id FROM Notification WHERE title = 'Contract expiring soon');
    DECLARE @nLeave    INT = (SELECT notification_id FROM Notification WHERE title = 'Leave request approved');
    DECLARE @nMission  INT = (SELECT notification_id FROM Notification WHERE title = 'New mission assigned');

    /* Welcome → everyone */
    INSERT INTO Employee_Notification (notification_id, employee_id, is_read, delivery_status, delivered_at)
    SELECT @nWelcome, employee_id, 0, 'Delivered', GETDATE() FROM Employee;

    /* Targeted deliveries */
    INSERT INTO Employee_Notification (notification_id, employee_id, is_read, delivery_status, delivered_at) VALUES
        (@nContract, 3, 0, 'Delivered', GETDATE()),
        (@nContract, 6, 0, 'Delivered', GETDATE()),
        (@nLeave,    3, 1, 'Delivered', GETDATE()),
        (@nMission,  1, 0, 'Delivered', GETDATE());
END

/* ---- 13. PAYROLL (one period per employee) ---- */
IF NOT EXISTS (SELECT 1 FROM Payroll)
    INSERT INTO Payroll (employee_id, base_salary, base_amount, total_allowances, total_deductions, taxes,
                         gross_salary, net_salary, actual_pay, currency_id, period_id,
                         period_start, period_end, payment_date, hours_worked, overtime_hours, status, created_at)
    SELECT
        e.employee_id,
        e.base_salary,
        e.base_salary,
        ROUND(e.base_salary * 0.10, 2)                                              AS total_allowances,
        ROUND(e.base_salary * 0.05, 2)                                              AS total_deductions,
        ROUND(e.base_salary * 0.08, 4)                                              AS taxes,
        ROUND(e.base_salary * 1.10, 2)                                              AS gross_salary,
        ROUND(e.base_salary * 1.10 - e.base_salary * 0.05 - e.base_salary * 0.08, 2) AS net_salary,
        ROUND(e.base_salary * 1.10 - e.base_salary * 0.05 - e.base_salary * 0.08, 2) AS actual_pay,
        @cur, 1,
        DATEFROMPARTS(@yr, MONTH(GETDATE()), 1),
        EOMONTH(GETDATE()),
        EOMONTH(GETDATE()),
        160, 5, 'Paid', GETDATE()
    FROM Employee e;

/* ---- 14. ORG HIERARCHY (reporting structure) ---- */
IF NOT EXISTS (SELECT 1 FROM EmployeeHierarchy)
    INSERT INTO EmployeeHierarchy (hierarchy_level, employee_id, manager_id, level, start_date, is_active) VALUES
        (1, 1,  2, 2, @today, 1),
        (1, 3,  2, 2, @today, 1),
        (1, 10, 2, 2, @today, 1),
        (1, 9,  5, 2, @today, 1),
        (1, 6,  8, 2, @today, 1),
        (1, 7,  8, 2, @today, 1);

PRINT 'Sample data seed complete.';
GO
