/* Corrective seed — constraint-compliant values for tables that
   failed in the first pass (shifts, leaves, attendance, missions). */
USE HR_Payroll_System;
GO

DECLARE @today DATE = CAST(GETDATE() AS DATE);
DECLARE @yr    INT  = YEAR(GETDATE());

DECLARE @eng INT = (SELECT department_id FROM Department WHERE department_name = 'Engineering');
DECLARE @ops INT = (SELECT department_id FROM Department WHERE department_name = 'Operations');

/* ---- SHIFTS (type in Normal/Rotational/Overnight/Split/Mission; end>start) ---- */
IF NOT EXISTS (SELECT 1 FROM ShiftSchedule WHERE name = 'Morning Shift')
    INSERT INTO ShiftSchedule (name, type, start_time, end_time, break_duration, status, is_active, grace_period_minutes, created_at) VALUES
        ('Morning Shift', 'Normal', '09:00', '17:00', 60, 'Active', 1, 15, GETDATE()),
        ('Evening Shift', 'Normal', '14:00', '22:00', 45, 'Active', 1, 15, GETDATE()),
        ('Split Shift',   'Split',  '08:00', '16:00', 30, 'Active', 1, 10, GETDATE());

DECLARE @morning INT = (SELECT shift_id FROM ShiftSchedule WHERE name = 'Morning Shift');
DECLARE @evening INT = (SELECT shift_id FROM ShiftSchedule WHERE name = 'Evening Shift');

/* ---- SHIFT ASSIGNMENTS (assignment_type Employee|Department; XOR emp/dept) ---- */
IF NOT EXISTS (SELECT 1 FROM ShiftAssignment)
    INSERT INTO ShiftAssignment (employee_id, department_id, shift_id, assignment_type, start_date, end_date, is_active, status) VALUES
        (1,    NULL, @morning, 'Employee',   @today, NULL, 1, 'Active'),
        (2,    NULL, @morning, 'Employee',   @today, NULL, 1, 'Active'),
        (3,    NULL, @morning, 'Employee',   @today, NULL, 1, 'Active'),
        (6,    NULL, @evening, 'Employee',   @today, NULL, 1, 'Active'),
        (7,    NULL, @evening, 'Employee',   @today, NULL, 1, 'Active'),
        (10,   NULL, @morning, 'Employee',   @today, NULL, 1, 'Active'),
        (NULL, @ops, @evening, 'Department', @today, NULL, 1, 'Active');

/* ---- LEAVE TYPES (allowed: Vacation,Sick,Medical,Holiday,Probation,Special) ---- */
IF NOT EXISTS (SELECT 1 FROM Leave)
    INSERT INTO Leave (leave_type, leave_description, description, is_paid, max_days_per_year, requires_approval, is_active) VALUES
        ('Vacation',  'Annual paid vacation leave', 'Annual leave',    1, 21, 1, 1),
        ('Sick',      'Paid sick leave',            'Sick leave',      1, 14, 1, 1),
        ('Medical',   'Extended medical leave',     'Medical leave',   1, 30, 1, 1),
        ('Holiday',   'Public holiday leave',       'Holiday leave',   1, 10, 0, 1),
        ('Probation', 'Probation-period leave',     'Probation leave', 0,  5, 1, 1);

DECLARE @vac  INT = (SELECT leave_id FROM Leave WHERE leave_type = 'Vacation');
DECLARE @sick INT = (SELECT leave_id FROM Leave WHERE leave_type = 'Sick');

/* ---- LEAVE ENTITLEMENTS (Vacation, Sick, Medical per employee) ---- */
IF NOT EXISTS (SELECT 1 FROM LeaveEntitlement)
    INSERT INTO LeaveEntitlement (entitlement, employee_id, leave_id, year, allocated_days, used_days, balance_days, carry_forward_days)
    SELECT NULL, e.employee_id, l.leave_id, @yr, l.max_days_per_year, 0, l.max_days_per_year, 0
    FROM Employee e CROSS JOIN Leave l
    WHERE l.leave_type IN ('Vacation', 'Sick', 'Medical');

UPDATE LeaveEntitlement SET used_days = 2, balance_days = allocated_days - 2 WHERE employee_id = 3 AND leave_id = @sick AND year = @yr;
UPDATE LeaveEntitlement SET used_days = 5, balance_days = allocated_days - 5 WHERE employee_id = 1 AND leave_id = @vac  AND year = @yr;

/* ---- LEAVE REQUESTS ---- */
IF NOT EXISTS (SELECT 1 FROM LeaveRequest)
    INSERT INTO LeaveRequest (employee_id, leave_id, start_date, end_date, total_days, reason, status, submitted_at, approved_by, approved_at, is_irregular) VALUES
        (1,  @vac,  DATEADD(DAY, 5,@today),  DATEADD(DAY, 9,@today), 5,  'Family trip',           'Pending',  GETDATE(),                  NULL, NULL,                       0),
        (3,  @sick, DATEADD(DAY,-3,@today),  DATEADD(DAY,-2,@today), 2,  'Flu recovery',          'Approved', DATEADD(DAY,-4,GETDATE()),  4,    DATEADD(DAY,-3,GETDATE()),  0),
        (6,  @vac,  DATEADD(DAY,10,@today),  DATEADD(DAY,20,@today), 11, 'Extended family leave', 'Pending',  GETDATE(),                  NULL, NULL,                       1),
        (7,  @vac,  DATEADD(DAY,-10,@today), DATEADD(DAY,-8,@today), 3,  'Personal matters',      'Rejected', DATEADD(DAY,-12,GETDATE()), 8,    DATEADD(DAY,-11,GETDATE()), 0),
        (10, @sick, DATEADD(DAY, 2,@today),  DATEADD(DAY, 3,@today), 2,  'Medical appointment',   'Pending',  GETDATE(),                  NULL, NULL,                       0);

/* ---- ATTENDANCE (status in Present/Late/Absent/OnLeave...; source_type in Terminal/Leave/Manual/Offline/GPS) ---- */
IF NOT EXISTS (SELECT 1 FROM Attendance)
    INSERT INTO Attendance (employee_id, attendance_date, shift_id, entry_time, exit_time, hours_worked, overtime_hours, lateness_minutes, status, source_type, created_at) VALUES
        (1,  @today,                @morning, DATEADD(HOUR,9,  CAST(@today AS DATETIME)),                 DATEADD(HOUR,17,CAST(@today AS DATETIME)),                 8, 0, 0,  'Present', 'Terminal', GETDATE()),
        (2,  @today,                @morning, DATEADD(HOUR,9,  CAST(@today AS DATETIME)),                 DATEADD(HOUR,18,CAST(@today AS DATETIME)),                 9, 1, 0,  'Present', 'Terminal', GETDATE()),
        (3,  @today,                @morning, DATEADD(MINUTE,565,CAST(@today AS DATETIME)),               DATEADD(HOUR,17,CAST(@today AS DATETIME)),                 7, 0, 25, 'Late',    'Terminal', GETDATE()),
        (10, @today,                @morning, DATEADD(HOUR,9,  CAST(@today AS DATETIME)),                 DATEADD(HOUR,17,CAST(@today AS DATETIME)),                 8, 0, 0,  'Present', 'Terminal', GETDATE()),
        (1,  DATEADD(DAY,-1,@today), @morning, DATEADD(HOUR,9, CAST(DATEADD(DAY,-1,@today) AS DATETIME)), DATEADD(HOUR,17,CAST(DATEADD(DAY,-1,@today) AS DATETIME)), 8, 0, 0,  'Present', 'Terminal', GETDATE()),
        (2,  DATEADD(DAY,-1,@today), @morning, DATEADD(HOUR,9, CAST(DATEADD(DAY,-1,@today) AS DATETIME)), DATEADD(HOUR,17,CAST(DATEADD(DAY,-1,@today) AS DATETIME)), 8, 0, 0,  'Present', 'Terminal', GETDATE()),
        (3,  DATEADD(DAY,-2,@today), NULL,     NULL,                                                      NULL,                                                      0, 0, 0,  'OnLeave', 'Leave',    GETDATE()),
        (6,  DATEADD(DAY,-1,@today), @evening, DATEADD(HOUR,14,CAST(DATEADD(DAY,-1,@today) AS DATETIME)), DATEADD(HOUR,22,CAST(DATEADD(DAY,-1,@today) AS DATETIME)), 8, 0, 0,  'Present', 'Terminal', GETDATE()),
        (7,  DATEADD(DAY,-1,@today), NULL,     NULL,                                                      NULL,                                                      0, 0, 0,  'Absent',  'Manual',   GETDATE());

/* ---- MISSIONS (status in Assigned/InProgress/Completed/Cancelled) ---- */
IF NOT EXISTS (SELECT 1 FROM Mission)
    INSERT INTO Mission (destination, manager_id, employee_id, mission_name, start_date, end_date, location, status, assigned_by, completed_at) VALUES
        ('Dubai',    2, 1,  'Client onboarding', DATEADD(DAY, 7,@today), DATEADD(DAY,10,@today), 'UAE',   'Assigned',   4, NULL),
        ('Riyadh',   8, 7,  'Sales conference',  DATEADD(DAY,-15,@today),DATEADD(DAY,-12,@today),'KSA',   'Completed',  4, DATEADD(DAY,-11,GETDATE())),
        ('Cairo HQ', 4, 3,  'Product training',  DATEADD(DAY, 3,@today), DATEADD(DAY, 4,@today), 'Egypt', 'Assigned',   4, NULL),
        ('London',   2, 10, 'QA audit',          DATEADD(DAY,20,@today), DATEADD(DAY,25,@today), 'UK',    'InProgress', 4, NULL);

PRINT 'Corrective seed complete.';
GO
