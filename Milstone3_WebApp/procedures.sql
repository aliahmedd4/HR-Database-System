
USE HR_Payroll_System;
GO

ALTER PROCEDURE ViewEmployeeInfo
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.employee_id,
        e.employee_code,
        e.first_name,
        e.last_name,
        e.email,
        e.phone,
        e.national_id,
        e.date_of_birth,
        e.country_of_birth,
        e.hire_date,
        e.termination_date,
        d.department_name,
        
        m.first_name + ' ' + m.last_name AS manager_name,
        
        
        c.currency_code,
        e.base_salary,
        e.is_active,
        e.profile_completion,
        e.emergency_contact_name,
        e.emergency_contact_phone,
        e.relationship,
        e.biography,
        e.employment_progress,
        e.account_status,
        e.employment_status,
        e.address
    FROM Employee e
    LEFT JOIN Department d ON e.department_id = d.department_id
    LEFT JOIN Position p ON e.position_id = p.position_id
    LEFT JOIN Employee m ON e.manager_id = m.employee_id
    

    LEFT JOIN Currency c ON e.currency_id = c.currency_id
    WHERE e.employee_id = @EmployeeID;
END;
GO




ALTER PROCEDURE AddEmployee
    @FullName VARCHAR(200),
    @NationalID VARCHAR(50),
    @DateOfBirth DATE,
    @CountryOfBirth VARCHAR(100),
    @Phone VARCHAR(50),
    @Email VARCHAR(100),
    @Address VARCHAR(255),
    @EmergencyContactName VARCHAR(100),
    @EmergencyContactPhone VARCHAR(50),
    @Relationship VARCHAR(50),
    @Biography VARCHAR(MAX),
    @EmploymentProgress VARCHAR(100),
    @AccountStatus VARCHAR(50),
    @EmploymentStatus VARCHAR(50),
    @HireDate DATE,
    @IsActive BIT,
    @ProfileCompletion INT,
    @DepartmentID INT,
    @PositionID INT,
    @ManagerID INT,
    @ContractID INT,
    @TaxFormID INT,
    @SalaryTypeID INT,
    @PayGradeID INT, -- renamed to match column
    @EmployeeID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @FirstName VARCHAR(100), @LastName VARCHAR(100);
        SET @FirstName = LEFT(@FullName, CHARINDEX(' ', @FullName + ' ') - 1);
        SET @LastName = LTRIM(RIGHT(@FullName, LEN(@FullName) - LEN(@FirstName)));

        INSERT INTO Employee (
            first_name, last_name, national_id, date_of_birth, country_of_birth,
            phone, email, address, emergency_contact_name, emergency_contact_phone,
            relationship, biography, employment_progress, account_status, employment_status,
            hire_date, is_active, profile_completion, department_id, position_id,
            manager_id, contract_id, tax_form_id, salary_type_id, pay_grade_id
        )
        VALUES (
            @FirstName, @LastName, @NationalID, @DateOfBirth, @CountryOfBirth,
            @Phone, @Email, @Address, @EmergencyContactName, @EmergencyContactPhone,
            @Relationship, @Biography, @EmploymentProgress, @AccountStatus, @EmploymentStatus,
            @HireDate, @IsActive, @ProfileCompletion, @DepartmentID, @PositionID,
            @ManagerID, @ContractID, @TaxFormID, @SalaryTypeID, @PayGradeID
        );

        SET @EmployeeID = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SELECT @EmployeeID AS EmployeeID, 'Employee added successfully.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


Alter PROCEDURE UpdateEmployeeInfo
    @EmployeeID INT,
    @Email VARCHAR(100),
    @Phone VARCHAR(20),
    @Address VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
            THROW 50001, 'Employee not found', 1;

        
        UPDATE Employee
        SET 
            email = @Email,
            phone = @Phone,
            address = @Address,
            updated_at = GETDATE()
        WHERE employee_id = @EmployeeID;

        COMMIT TRANSACTION;

        SELECT 'Employee information updated successfully.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;


GO







ALTER PROCEDURE AssignRole
    @EmployeeID INT,
    @RoleID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if employee exists
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        THROW 50001, 'Employee not found', 1;

    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM Role WHERE role_id = @RoleID)
        THROW 50002, 'Role not found', 1;

    -- Check if role already assigned
    IF EXISTS (SELECT 1 FROM Employee_Role WHERE employee_id = @EmployeeID AND role_id = @RoleID)
    BEGIN
        SELECT 'Role already assigned' AS message;
    END
    ELSE
    BEGIN
        INSERT INTO Employee_Role (employee_id, role_id)
        VALUES (@EmployeeID, @RoleID);

        SELECT 'Role assigned successfully' AS message;
    END
END;
GO




Alter PROCEDURE GetDepartmentEmployeeStats
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        
        SELECT d.department_id,
               d.department_name,
               COUNT(e.employee_id) AS employee_count
        FROM Department d
        LEFT JOIN Employee e 
            ON d.department_id = e.department_id
        GROUP BY d.department_id, d.department_name
        ORDER BY d.department_name;
    END TRY
    BEGIN CATCH
        
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        
        THROW;
    END CATCH
END;
GO



Alter PROCEDURE ReassignManager
    @EmployeeID INT,
    @NewManagerID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        THROW 50001, 'Employee not found', 1;

    IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @NewManagerID)
        THROW 50002, 'New manager not found', 1;

    IF @EmployeeID = @NewManagerID
        THROW 50003, 'Employee cannot be their own manager', 1;

    UPDATE Employee
    SET manager_id = @NewManagerID,
        updated_at = GETDATE()
    WHERE employee_id = @EmployeeID;

    SELECT 'Manager reassigned successfully' AS message;
END;
GO



Alter PROCEDURE ReassignHierarchy
    @EmployeeID INT,
    @NewDepartmentID INT,
    @NewManagerID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF NOT EXISTS (SELECT 1 FROM Department WHERE department_id = @NewDepartmentID)
        BEGIN
            RAISERROR('Department does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @NewManagerID)
        BEGIN
            RAISERROR('Manager does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        UPDATE Employee
        SET department_id = @NewDepartmentID,
            manager_id = @NewManagerID
        WHERE employee_id = @EmployeeID;

        COMMIT TRANSACTION;

        PRINT 'Employee reassigned successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END; 
GO






Alter PROCEDURE NotifyStructureChange
    @AffectedEmployees VARCHAR(500),
    @Message VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NotificationID INT;

    -- Insert notification using correct column name: message_content
    INSERT INTO Notification (
        notification_type, title, message_content, priority
    )
    VALUES (
        'Hierarchy Change', 'Organization Structure Updated', @Message, 'Normal'
    );

    SET @NotificationID = SCOPE_IDENTITY();

    -- Notify all active employees (or filter using @AffectedEmployees if needed)
    INSERT INTO Employee_Notification (notification_id, employee_id)
    SELECT @NotificationID, employee_id
    FROM Employee
    WHERE is_active = 1;

    SELECT @NotificationID AS notification_id, 'Notifications sent successfully' AS message;
END;
GO




Alter PROCEDURE ViewOrgHierarchy
    @RootEmployeeID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH HierarchyCTE AS (
        -- Anchor member
        SELECT 
            e.employee_id,
            e.employee_code,
            e.first_name + ' ' + e.last_name AS employee_name,
            e.manager_id,
            CAST(e.first_name + ' ' + e.last_name AS NVARCHAR(MAX)) AS hierarchy_path,
            0 AS level
        FROM Employee e
        WHERE (@RootEmployeeID IS NULL AND e.manager_id IS NULL)
              OR e.employee_id = @RootEmployeeID

        UNION ALL

        -- Recursive member
        SELECT 
            e.employee_id,
            e.employee_code,
            e.first_name + ' ' + e.last_name AS employee_name,
            e.manager_id,
            CAST(h.hierarchy_path + ' > ' + e.first_name + ' ' + e.last_name AS NVARCHAR(MAX)) AS hierarchy_path,
            h.level + 1
        FROM Employee e
        INNER JOIN HierarchyCTE h ON e.manager_id = h.employee_id
        WHERE e.is_active = 1
    )
    SELECT 
        employee_id,
        employee_code,
        employee_name,
        manager_id,
        hierarchy_path,
        level
    FROM HierarchyCTE
    ORDER BY level, employee_name;
END;
GO



Alter PROCEDURE AssignShiftToEmployee
    @EmployeeID INT,
    @ShiftID INT,
    @StartDate DATE,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        THROW 50001, 'Employee not found', 1;
    
    IF NOT EXISTS (SELECT 1 FROM ShiftSchedule WHERE shift_id = @ShiftID)
        THROW 50002, 'Shift not found', 1;
    
    IF @EndDate IS NOT NULL AND @EndDate < @StartDate
        THROW 50003, 'End date must be after start date', 1;
    
    UPDATE ShiftAssignment
    SET is_active = 0, end_date = DATEADD(DAY, -1, @StartDate)
    WHERE employee_id = @EmployeeID AND is_active = 1;
    
    INSERT INTO ShiftAssignment (employee_id, shift_id, assignment_type, start_date, end_date, is_active)
    VALUES (@EmployeeID, @ShiftID, 'Employee', @StartDate, @EndDate, 1);
    
    SELECT 'Shift assigned successfully' AS message;
END;
GO


ALTER PROCEDURE UpdateShiftStatus
    @ShiftAssignmentID INT,
    @Status VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ShiftID INT;
    DECLARE @IsActive BIT;

    IF @Status = 'Active'
        SET @IsActive = 1;
    ELSE IF @Status = 'Inactive'
        SET @IsActive = 0;
    ELSE
        THROW 50002, 'Invalid status value. Use ''Active'' or ''Inactive''.', 1;

    SELECT @ShiftID = shift_id
    FROM ShiftAssignment
    WHERE assignment_id = @ShiftAssignmentID; -- FIXED

    IF @ShiftID IS NULL
        THROW 50001, 'Shift assignment not found', 1;

    UPDATE ShiftSchedule
    SET is_active = @IsActive
    WHERE shift_id = @ShiftID;

    SELECT 'Shift status updated successfully' AS message;
END;
GO


ALTER PROCEDURE AssignShiftToDepartment
    @DepartmentID INT,
    @ShiftID INT,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate department
    IF NOT EXISTS (SELECT 1 FROM Department WHERE department_id = @DepartmentID)
        THROW 50001, 'Department not found', 1;

    -- Validate shift
    IF NOT EXISTS (SELECT 1 FROM ShiftSchedule WHERE shift_id = @ShiftID)
        THROW 50002, 'Shift not found', 1;

    -- Deactivate any current active shift for this department
    UPDATE ShiftAssignment
    SET is_active = 0,
        end_date = DATEADD(DAY, -1, @StartDate)
    WHERE department_id = @DepartmentID AND is_active = 1;

    -- Assign new shift
    INSERT INTO ShiftAssignment (
        department_id, shift_id, assignment_type, start_date, end_date, is_active
    )
    VALUES (
        @DepartmentID, @ShiftID, 'Department', @StartDate, @EndDate, 1
    );

    SELECT 'Shift assigned to department successfully' AS message;
END;
GO



ALTER PROCEDURE AssignCustomShift
    @EmployeeID INT,
    @ShiftName VARCHAR(50),
    @ShiftType VARCHAR(50),
    @StartTime TIME,
    @EndTime TIME,
    @StartDate DATE,
    @EndDate DATE = NULL,
    @BreakDurationMinutes INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate employee existence
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        THROW 50001, 'Employee not found', 1;

    -- Validate time range
    IF @EndTime <= @StartTime
        THROW 50002, 'End time must be after start time', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Create new shift
        INSERT INTO ShiftSchedule (
            name, type, start_time, end_time, break_duration, is_active
        )
        VALUES (
            @ShiftName, @ShiftType, @StartTime, @EndTime, @BreakDurationMinutes, 1
        );

        DECLARE @NewShiftID INT = SCOPE_IDENTITY();

        -- Assign shift to employee
        EXEC AssignShiftToEmployee @EmployeeID, @NewShiftID, @StartDate, @EndDate;

        COMMIT TRANSACTION;

        SELECT @NewShiftID AS shift_id, 'Custom shift created and assigned successfully' AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO




ALTER PROCEDURE ConfigureSplitShift
    @ShiftName VARCHAR(50),
    @FirstSlotStart TIME,
    @FirstSlotEnd TIME,
    @SecondSlotStart TIME,
    @SecondSlotEnd TIME,
    @BreakDurationMinutes INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ShiftID INT;

    -- Find the shift ID by name
    SELECT @ShiftID = shift_id
    FROM ShiftSchedule
    WHERE name = @ShiftName; -- FIXED

    IF @ShiftID IS NULL
        THROW 50001, 'Shift not found', 1;

    -- Update the shift to configure split timing
    UPDATE ShiftSchedule
    SET 
        type = 'Split', -- FIXED
        start_time = @FirstSlotStart,
        end_time = @SecondSlotEnd,
        break_duration = @BreakDurationMinutes -- FIXED
    WHERE shift_id = @ShiftID;

    SELECT 'Split shift configured successfully' AS message;
END;
GO




ALTER PROCEDURE EnableFirstInLastOut
    @Enabled BIT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        CASE 
            WHEN @Enabled = 1 THEN 'First In Last Out enabled' 
            ELSE 'First In Last Out disabled' 
        END AS status;
END;
GO



ALTER PROCEDURE TagAttendanceSource
    @SourceType VARCHAR(50),
    @SourceIdentifier VARCHAR(100),
    @LocationName VARCHAR(200) = NULL,
    @Latitude DECIMAL(10,8) = NULL,
    @Longitude DECIMAL(11,8) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @SourceType NOT IN ('GPS', 'Terminal')
        THROW 50001, 'Invalid source type. Must be GPS or Terminal', 1;
    
    IF EXISTS (SELECT 1 FROM AttendanceSource WHERE source_type = @SourceType AND source_identifier = @SourceIdentifier)
    BEGIN
        UPDATE AttendanceSource
        SET 
            location_name = ISNULL(@LocationName, location_name),
            latitude = ISNULL(@Latitude, latitude),
            longitude = ISNULL(@Longitude, longitude),
            is_active = 1
        WHERE source_type = @SourceType AND source_identifier = @SourceIdentifier;
    END
    ELSE
    BEGIN
        INSERT INTO AttendanceSource (source_type, source_identifier, location_name, latitude, longitude, is_active)
        VALUES (@SourceType, @SourceIdentifier, @LocationName, @Latitude, @Longitude, 1);
    END
    
    SELECT 'Attendance source tagged successfully' AS message;
END;
GO


ALTER PROCEDURE SyncOfflineAttendance
    @EmployeeID INT,
    @AttendanceDate DATE,
    @CheckInTime DATETIME,
    @CheckOutTime DATETIME = NULL,
    @SourceType VARCHAR(50) = 'Offline',
    @SourceID VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate employee
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        THROW 50001, 'Employee not found', 1;

    -- Get shift ID
    DECLARE @ShiftID INT = (
        SELECT shift_id
        FROM ShiftAssignment
        WHERE employee_id = @EmployeeID
          AND @AttendanceDate BETWEEN start_date AND ISNULL(end_date, '9999-12-31')
          AND is_active = 1
    );

    -- Calculate hours worked
    DECLARE @HoursWorked DECIMAL(5,2) = DATEDIFF(MINUTE, @CheckInTime, ISNULL(@CheckOutTime, @CheckInTime)) / 60.0;

    -- Set attendance status
    DECLARE @Status NVARCHAR(50) = 'Present';

    -- Update or insert attendance
    IF EXISTS (
        SELECT 1 FROM Attendance
        WHERE employee_id = @EmployeeID AND attendance_date = @AttendanceDate
    )
    BEGIN
        UPDATE Attendance
        SET 
            entry_time = @CheckInTime,
            exit_time = @CheckOutTime,
            hours_worked = @HoursWorked,
            status = @Status,
            source_type = @SourceType,
            source_id = @SourceID
        WHERE employee_id = @EmployeeID AND attendance_date = @AttendanceDate;
    END
    ELSE
    BEGIN
        INSERT INTO Attendance (
            employee_id, attendance_date, shift_id, entry_time, exit_time,
            status, hours_worked, source_type, source_id
        )
        VALUES (
            @EmployeeID, @AttendanceDate, @ShiftID, @CheckInTime, @CheckOutTime,
            @Status, @HoursWorked, @SourceType, @SourceID
        );
    END

    SELECT 'Offline attendance synced successfully' AS message;
END;
GO

ALTER PROCEDURE LogAttendanceEdit
    @AttendanceID INT,
    @EditedBy INT,
    @OldValue DATETIME,
    @NewValue DATETIME,
    @EditTimestamp DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF NOT EXISTS (SELECT 1 FROM Attendance WHERE attendance_id = @AttendanceID)
        BEGIN
            RAISERROR('Attendance record does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EditedBy)

        BEGIN
            RAISERROR('Editor (employee) does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        INSERT INTO AttendanceEditLog (
            attendance_id,
            edited_by,
            old_value,
            new_value,
            edit_timestamp
        )
        VALUES (
            @AttendanceID,
            @EditedBy,
            @OldValue,
            @NewValue,
            @EditTimestamp
        );

        COMMIT TRANSACTION;

        PRINT 'Attendance edit logged successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO




ALTER PROCEDURE ApplyHolidayOverrides
    @HolidayID INT,
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate holiday
        IF NOT EXISTS (SELECT 1 FROM Holiday WHERE holiday_id = @HolidayID)
        BEGIN
            RAISERROR('Holiday does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate employee
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Apply holiday override
        UPDATE ShiftAssignment
        SET status = 'HolidayOverride'
        WHERE employee_id = @EmployeeID
          AND start_date <= (SELECT holiday_date FROM Holiday WHERE holiday_id = @HolidayID)
          AND end_date >= (SELECT holiday_date FROM Holiday WHERE holiday_id = @HolidayID);

        COMMIT TRANSACTION;

        PRINT 'Holiday override applied successfully to employee shifts.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO


ALTER PROCEDURE ManageUserAccounts
    @UserID INT,
    @Role VARCHAR(50),
    @Action VARCHAR(20)  -- 'Create', 'Update', 'Delete'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF @Action NOT IN ('Create', 'Update', 'Delete')
        BEGIN
            RAISERROR('Invalid action. Allowed values: Create, Update, Delete.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @Action = 'Create'
        BEGIN
            IF EXISTS (SELECT 1 FROM PayrollUser WHERE user_id = @UserID)
            BEGIN
                RAISERROR('User already exists.', 16, 1);
                ROLLBACK TRANSACTION;

                RETURN;
            END

            INSERT INTO PayrollUser(user_id, role, created_at)
            VALUES(@UserID, @Role, GETDATE());

            PRINT 'User account created successfully.';
        END

       
        ELSE IF @Action = 'Update'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM PayrollUser WHERE user_id = @UserID)
            BEGIN
                RAISERROR('User does not exist.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END

            UPDATE PayrollUser
            SET role = @Role,
                updated_at = GETDATE()
            WHERE user_id = @UserID;

            PRINT 'User role updated successfully.';
        END



        
        ELSE IF @Action = 'Delete'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM PayrollUser WHERE user_id = @UserID)
            BEGIN
                RAISERROR('User does not exist.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END

            DELETE FROM PayrollUser
            WHERE user_id = @UserID;

            PRINT 'User account deleted successfully.';
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO



ALTER PROCEDURE CreateContract
    @EmployeeID INT,
    @Type VARCHAR(50),
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @EndDate < @StartDate
        BEGIN
            RAISERROR('End date cannot be before start date.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        INSERT INTO Contract(employee_id, contract_type, start_date, end_date, created_at)
        VALUES(@EmployeeID, @Type, @StartDate, @EndDate, GETDATE());

        COMMIT TRANSACTION;

        PRINT 'Employment contract created successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

ALTER PROCEDURE RenewContract
    @ContractID INT,
    @NewEndDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate contract existence
    IF NOT EXISTS (SELECT 1 FROM Contract WHERE contract_id = @ContractID)
        THROW 50001, 'Contract not found', 1;
    
    DECLARE @StartDate DATE = (SELECT start_date FROM Contract WHERE contract_id = @ContractID);
    
    -- Validate new end date
    IF @NewEndDate <= @StartDate
        THROW 50002, 'New end date must be after start date', 1;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Mark current contract as renewed
        UPDATE Contract 
        SET current_state = 'Renewed', end_date = @NewEndDate 
        WHERE contract_id = @ContractID;
        
        -- Retrieve employee and contract type
        DECLARE @EmployeeID INT = (SELECT employee_id FROM Contract WHERE contract_id = @ContractID);
        DECLARE @ContractType VARCHAR(50) = (SELECT contract_type FROM Contract WHERE contract_id = @ContractID);
        DECLARE @NewContractID INT;
        
        -- Create new contract
        INSERT INTO Contract (employee_id, contract_type, start_date, end_date, current_state)
        VALUES (@EmployeeID, @ContractType, DATEADD(DAY, 1, @NewEndDate), NULL, 'Active');
        
        SET @NewContractID = SCOPE_IDENTITY();
        
        -- Copy contract-specific details
        IF @ContractType = 'FullTime'
        BEGIN
            INSERT INTO FullTimeContract (contract_id, leave_entitlement, insurance_eligibility, weekly_working_hours, benefits)
            SELECT @NewContractID, leave_entitlement, insurance_eligibility, weekly_working_hours, benefits
            FROM FullTimeContract WHERE contract_id = @ContractID;
        END
        ELSE IF @ContractType = 'PartTime'
        BEGIN
            INSERT INTO PartTimeContract (contract_id, working_hours, schedule, hourly_rate)
            SELECT @NewContractID, working_hours, schedule, hourly_rate
            FROM PartTimeContract WHERE contract_id = @ContractID;
        END
        ELSE IF @ContractType = 'Consultant'
        BEGIN
            INSERT INTO ConsultantContract (contract_id, project_scope, fees, payment_schedule, hourly_rate, max_hours_per_month)
            SELECT @NewContractID, project_scope, fees, payment_schedule, hourly_rate, max_hours_per_month
            FROM ConsultantContract WHERE contract_id = @ContractID;
        END
        ELSE IF @ContractType = 'Internship'
        BEGIN
            INSERT INTO InternshipContract (contract_id, supervisor_id, stipend_related, learning_objectives, evaluation, mentoring)
            SELECT @NewContractID, supervisor_id, stipend_related, learning_objectives, evaluation, mentoring
            FROM InternshipContract WHERE contract_id = @ContractID;
        END
        
        COMMIT TRANSACTION;
        
        SELECT @NewContractID AS new_contract_id, 'Contract renewed successfully' AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO



ALTER PROCEDURE ApproveLeaveRequest
    @LeaveRequestID INT,
    @ApproverID INT,
    @Status VARCHAR(20)   
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate leave request
        IF NOT EXISTS (SELECT 1 FROM LeaveRequest WHERE request_id = @LeaveRequestID)
        BEGIN
            RAISERROR('Leave request does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate approver
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @ApproverID)
        BEGIN
            RAISERROR('Approver does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate status
        IF @Status NOT IN ('Approved', 'Rejected')
        BEGIN
            RAISERROR('Invalid status. Allowed values: Approved, Rejected.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update leave request
        UPDATE LeaveRequest
        SET 
            status = @Status,
            approved_by = @ApproverID,
            approved_at = GETDATE()
        WHERE request_id = @LeaveRequestID;

        COMMIT TRANSACTION;

        PRINT 'Leave request processed successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO


CREATE OR ALTER PROCEDURE AssignMission
    @EmployeeID INT,
    @ManagerID INT = NULL,
    @Destination VARCHAR(200) = NULL,
    @MissionName NVARCHAR(200),
    @StartDate DATE,
    @EndDate DATE = NULL,
    @Location NVARCHAR(200) = NULL,
    @AssignedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate employee exists
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate manager exists if provided
        IF @ManagerID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @ManagerID)
        BEGIN
            RAISERROR('Manager does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate end date
        IF @EndDate IS NOT NULL AND @EndDate < @StartDate
        BEGIN
            RAISERROR('End date cannot be before start date.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert into Mission table (corrected from MissionAssignment)
        INSERT INTO Mission (
            employee_id,
            manager_id,
            destination,
            mission_name,
            start_date,
            end_date,
            location,
            status,
            assigned_by
        )
        VALUES (
            @EmployeeID,
            @ManagerID,
            @Destination,
            @MissionName,
            @StartDate,
            @EndDate,
            @Location,
            CASE WHEN @ManagerID IS NOT NULL THEN 'Pending' ELSE 'Assigned' END,
            @AssignedBy
        );

        COMMIT TRANSACTION;

        PRINT 'Mission assigned successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO


ALTER PROCEDURE ReviewReimbursement
    @ClaimID INT,
    @ApproverID INT,
    @Decision VARCHAR(20)  
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate reimbursement claim
        IF NOT EXISTS (SELECT 1 FROM Reimbursement WHERE reimbursement_id = @ClaimID)
        BEGIN
            RAISERROR('Reimbursement claim does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate approver
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @ApproverID)
        BEGIN
            RAISERROR('Approver does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate decision
        IF @Decision NOT IN ('Approved', 'Rejected')
        BEGIN
            RAISERROR('Invalid decision. Allowed values: Approved, Rejected.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update reimbursement record
        UPDATE Reimbursement
        SET 
            current_status = @Decision,
            approval_date = GETDATE()
        WHERE reimbursement_id = @ClaimID;

        COMMIT TRANSACTION;

        PRINT 'Reimbursement claim processed successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE GetActiveContracts
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        
        SELECT 
            c.contract_id,
            e.employee_id,
            e.full_name AS employee_name,
            c.contract_type,
            c.start_date,
            c.end_date,
            c.status
        FROM EmploymentContract c
        INNER JOIN Employee e ON c.employee_id = e.employee_id
        WHERE 
            c.status = 'Active'
            AND GETDATE() BETWEEN c.start_date AND c.end_date
        ORDER BY c.end_date;
    END TRY
    BEGIN CATCH
        THROW; -- re-raise error for debugging
    END CATCH
END;
GO


ALTER PROCEDURE GetTeamByManager
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.employee_id,
        e.employee_code,
        e.first_name + ' ' + e.last_name AS employee_name,
        e.email,
        d.department_name,
        p.position_title AS position_title, -- FIXED
        e.hire_date,
        e.is_active
    FROM Employee e
    LEFT JOIN Department d ON e.department_id = d.department_id
    LEFT JOIN Position p ON e.position_id = p.position_id
    WHERE e.manager_id = @ManagerID
    ORDER BY e.first_name, e.last_name;
END;
GO


ALTER PROCEDURE UpdateLeavePolicy
    @PolicyID INT,
    @EligibilityRules VARCHAR(200),
    @NoticePeriod TIME
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate policy existence
        IF NOT EXISTS (SELECT 1 FROM LeavePolicy WHERE policy_id = @PolicyID)
        BEGIN
            RAISERROR('Leave policy does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update policy
        UPDATE LeavePolicy
        SET 
            eligibility_rules = @EligibilityRules,
            notice_period = @NoticePeriod
        WHERE policy_id = @PolicyID;

        COMMIT TRANSACTION;

        PRINT 'Leave policy updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO

ALTER PROCEDURE GetExpiringContracts
    @DaysBefore INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT 
            c.contract_id,
            e.employee_id,
            e.full_name AS employee_name,
            c.contract_type,
            c.start_date,
            c.end_date,
            DATEDIFF(DAY, GETDATE(), c.end_date) AS days_remaining,
            c.status
        FROM EmploymentContract c
        INNER JOIN Employee e ON c.employee_id = e.employee_id
        WHERE 
            c.status = 'Active'
            AND c.end_date BETWEEN GETDATE() AND DATEADD(DAY, @DaysBefore, GETDATE())
        ORDER BY c.end_date;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


ALTER PROCEDURE AssignDepartmentHead
    @DepartmentID INT,
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate department
        IF NOT EXISTS (SELECT 1 FROM Department WHERE department_id = @DepartmentID)
        BEGIN
            RAISERROR('Department does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate manager
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @ManagerID)
        BEGIN
            RAISERROR('Manager (employee) does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Assign department head
        UPDATE Department
        SET department_head_id = @ManagerID
        WHERE department_id = @DepartmentID;

        COMMIT TRANSACTION;

        PRINT 'Department head assigned successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO


ALTER PROCEDURE CreateEmployeeProfile
    @FirstName VARCHAR(50),
    @LastName VARCHAR(50),
    @DepartmentID INT,
    @RoleID INT,
    @HireDate DATE,
    @Email VARCHAR(100),
    @Phone VARCHAR(20),
    @NationalID VARCHAR(50),
    @DateOfBirth DATE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate department
        IF NOT EXISTS (SELECT 1 FROM Department WHERE department_id = @DepartmentID)
        BEGIN
            RAISERROR('Department does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate role
        IF NOT EXISTS (SELECT 1 FROM Role WHERE role_id = @RoleID)
        BEGIN
            RAISERROR('Role does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Check for duplicates
        IF EXISTS (SELECT 1 FROM Employee WHERE national_id = @NationalID OR email = @Email)
        BEGIN
            RAISERROR('An employee with the same National ID or Email already exists.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert new employee
        INSERT INTO Employee (
            full_name,
            department_id,
            position_id,
            hire_date,
            email,
            phone,
            national_id,
            date_of_birth,
            employment_status,
            created_at
        )
        VALUES (
            @FirstName + ' ' + @LastName,
            @DepartmentID,
            @RoleID,
            @HireDate,
            @Email,
            @Phone,
            @NationalID,
            @DateOfBirth,
            'Active',
            GETDATE()
        );

        COMMIT TRANSACTION;

        PRINT 'New employee profile created successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO


ALTER PROCEDURE UpdateEmployeeProfile
    @EmployeeID INT,
    @FieldName VARCHAR(50),
    @NewValue VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate employee existence
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Field-specific updates
        IF @FieldName = 'full_name'
            UPDATE Employee SET full_name = @NewValue WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'email'
            UPDATE Employee SET email = @NewValue WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'phone'
            UPDATE Employee SET phone = @NewValue WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'address'
            UPDATE Employee SET address = @NewValue WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'position_id'
            UPDATE Employee SET position_id = CAST(@NewValue AS INT) WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'department_id'
            UPDATE Employee SET department_id = CAST(@NewValue AS INT) WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'date_of_birth'
            UPDATE Employee SET date_of_birth = CAST(@NewValue AS DATE) WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'national_id'
            UPDATE Employee SET national_id = @NewValue WHERE employee_id = @EmployeeID;
        ELSE IF @FieldName = 'employment_status'
            UPDATE Employee SET employment_status = @NewValue WHERE employee_id = @EmployeeID;
        ELSE
        BEGIN
            RAISERROR('Invalid or unsupported field name.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        COMMIT TRANSACTION;

        PRINT 'Employee profile updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE SetProfileCompleteness
    @EmployeeID INT,
    @CompletenessPercentage INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate employee
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate percentage
        IF @CompletenessPercentage < 0 OR @CompletenessPercentage > 100
        BEGIN
            RAISERROR('Completeness percentage must be between 0 and 100.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update profile_completion (bit)
        UPDATE Employee
        SET 
            profile_completion = CASE WHEN @CompletenessPercentage = 100 THEN 1 ELSE 0 END,
            updated_at = GETDATE()
        WHERE employee_id = @EmployeeID;

        COMMIT TRANSACTION;

        PRINT 'Profile completeness updated successfully.';

        SELECT employee_id, profile_completion
        FROM Employee
        WHERE employee_id = @EmployeeID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO



ALTER PROCEDURE GenerateProfileReport
    @FilterField VARCHAR(50),
    @FilterValue VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @FilterField = 'country_of_birth'
        BEGIN
            SELECT employee_id, full_name, department_id, position_id, employment_status AS status, country_of_birth, date_of_birth
            FROM Employee
            WHERE country_of_birth = @FilterValue
            ORDER BY full_name;
        END
        ELSE IF @FilterField = 'department_id'
        BEGIN
            SELECT employee_id, full_name, department_id, position_id, employment_status AS status, country_of_birth, date_of_birth
            FROM Employee
            WHERE department_id = CAST(@FilterValue AS INT)
            ORDER BY full_name;
        END
        ELSE IF @FilterField = 'position_id'
        BEGIN
            SELECT employee_id, full_name, department_id, position_id, employment_status AS status, country_of_birth, date_of_birth
            FROM Employee
            WHERE position_id = CAST(@FilterValue AS INT)
            ORDER BY full_name;
        END
        ELSE IF @FilterField = 'status'
        BEGIN
            SELECT employee_id, full_name, department_id, position_id, employment_status AS status, country_of_birth, date_of_birth
            FROM Employee
            WHERE employment_status = @FilterValue
            ORDER BY full_name;
        END
        ELSE
        BEGIN
            RAISERROR('Invalid filter field. Allowed fields: country_of_birth, department_id, position_id, status.', 16, 1);
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO





ALTER PROCEDURE CreateShiftType
    @ShiftID INT,
    @Name VARCHAR(100),
    @Type VARCHAR(50),          -- e.g., Normal, Split, Overnight, Mission
    @Start_Time TIME,
    @End_Time TIME,
    @Break_Duration INT,        -- in minutes
    @Shift_Date DATE,
    @Status VARCHAR(50)         -- e.g., Active, Inactive, Draft
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF @Type NOT IN ('Normal', 'Split', 'Overnight', 'Mission')
        BEGIN
            RAISERROR('Invalid shift type. Allowed values: Normal, Split, Overnight, Mission.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @End_Time <= @Start_Time
        BEGIN
            RAISERROR('End time must be after start time.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

       
        IF @Break_Duration < 0
        BEGIN
            RAISERROR('Break duration cannot be negative.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

       
        IF EXISTS (SELECT 1 FROM Shift WHERE shift_id = @ShiftID)
        BEGIN
            UPDATE Shift
            SET shift_name = @Name,
                shift_type = @Type,
                start_time = @Start_Time,
                end_time = @End_Time,
                break_duration = @Break_Duration,
                shift_date = @Shift_Date,
                status = @Status,
                updated_at = GETDATE()
            WHERE shift_id = @ShiftID;
        END
        ELSE
        BEGIN
            INSERT INTO Shift(shift_id, shift_name, shift_type, start_time, end_time, break_duration, shift_date, status, created_at)
            VALUES(@ShiftID, @Name, @Type, @Start_Time, @End_Time, @Break_Duration, @Shift_Date, @Status, GETDATE());
        END

        COMMIT TRANSACTION;

        PRINT 'Shift type created or updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO






ALTER PROCEDURE AssignRotationalShift
    @EmployeeID INT,
    @ShiftCycle INT,             
    @StartDate DATE,
    @EndDate DATE,
    @Status VARCHAR(20)      
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentDate DATE = @StartDate;
    DECLARE @ShiftIndex INT = 0;
    DECLARE @ShiftName VARCHAR(20);
    DECLARE @ShiftID INT;

    BEGIN TRY
        BEGIN TRANSACTION;

       
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        WHILE @CurrentDate <= @EndDate
        BEGIN
            
            SET @ShiftName = 
                CASE (@ShiftIndex % 3)
                    WHEN 0 THEN 'Morning'
                    WHEN 1 THEN 'Evening'
                    WHEN 2 THEN 'Night'
                END;

            
            SELECT TOP 1 @ShiftID = shift_id
            FROM Shift
            WHERE shift_name = @ShiftName;

            IF @ShiftID IS NULL
            BEGIN
                RAISERROR('Shift "%s" not found in Shift table.', 16, 1, @ShiftName);
                ROLLBACK TRANSACTION;
                RETURN;
            END

            
            INSERT INTO ShiftAssignment (
                employee_id,
                shift_id,
                start_date,
                end_date,
                status
            )
            VALUES (
                @EmployeeID,
                @ShiftID,
                @CurrentDate,
                DATEADD(DAY, @ShiftCycle - 1, @CurrentDate),
                @Status
            );

           
            SET @CurrentDate = DATEADD(DAY, @ShiftCycle, @CurrentDate);
            SET @ShiftIndex = @ShiftIndex + 1;
        END

        COMMIT TRANSACTION;

        PRINT 'Rotational shifts assigned successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE NotifyShiftExpiry
    @EmployeeID INT,
    @ShiftAssignmentID INT,
    @ExpiryDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate employee
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate shift assignment
        IF NOT EXISTS (SELECT 1 FROM ShiftAssignment WHERE assignment_id = @ShiftAssignmentID)
        BEGIN
            RAISERROR('Shift assignment does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Log shift expiry notification
        INSERT INTO ShiftExpiryNotification (
            employee_id,
            shift_assignment_id,
            expiry_date,
            notified_at
        )
        VALUES (
            @EmployeeID,
            @ShiftAssignmentID,
            @ExpiryDate,
            GETDATE()
        );

        COMMIT TRANSACTION;

        PRINT 'Shift expiry notification logged successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO


ALTER PROCEDURE DefineShortTimeRules
    @RuleName VARCHAR(50),
    @LateMinutes INT,
    @EarlyLeaveMinutes INT,
    @PenaltyType VARCHAR(50)   -- e.g., 'Deduction', 'Warning', 'Unpaid'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF @RuleName IS NULL OR LTRIM(RTRIM(@RuleName)) = ''
        BEGIN
            RAISERROR('Rule name cannot be empty.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @LateMinutes < 0 OR @EarlyLeaveMinutes < 0
        BEGIN
            RAISERROR('Late minutes and early leave minutes must be non-negative.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @PenaltyType NOT IN ('Deduction', 'Warning', 'Unpaid')
        BEGIN
            RAISERROR('Invalid penalty type. Allowed values: Deduction, Warning, Unpaid.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        INSERT INTO ShortTimeRules(rule_name, late_minutes, early_leave_minutes, penalty_type, created_at)
        VALUES(@RuleName, @LateMinutes, @EarlyLeaveMinutes, @PenaltyType, GETDATE());

        COMMIT TRANSACTION;

        PRINT 'Short time rule defined successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; -- re-raise error for debugging
    END CATCH
END;
GO




ALTER PROCEDURE SetGracePeriod
    @Minutes INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

       
        IF @Minutes < 0 OR @Minutes > 60
        BEGIN
            RAISERROR('Grace period must be between 0 and 60 minutes.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF EXISTS (SELECT 1 FROM SystemConfig WHERE config_key = 'GracePeriodMinutes')
        BEGIN
            UPDATE SystemConfig
            SET config_value = CAST(@Minutes AS VARCHAR),
                updated_at = GETDATE()
            WHERE config_key = 'GracePeriodMinutes';
        END
        ELSE
        BEGIN
            INSERT INTO SystemConfig (config_key, config_value, created_at)
            VALUES ('GracePeriodMinutes', CAST(@Minutes AS VARCHAR), GETDATE());
        END

        COMMIT TRANSACTION;

        PRINT 'Grace period set successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE DefinePenaltyThreshold
    @LateMinutes INT,
    @DeductionType VARCHAR(50)   -- e.g., 'Half-Day', 'Full-Day', 'Warning'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

       
        IF @LateMinutes <= 0
        BEGIN
            RAISERROR('Late minutes must be greater than zero.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @DeductionType NOT IN ('Half-Day', 'Full-Day', 'Warning', 'Deduction')
        BEGIN
            RAISERROR('Invalid deduction type. Allowed values: Half-Day, Full-Day, Warning, Deduction.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

       
        INSERT INTO PenaltyThresholds(late_minutes, deduction_type, created_at)
        VALUES(@LateMinutes, @DeductionType, GETDATE());

        COMMIT TRANSACTION;

        PRINT 'Penalty threshold defined successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;

GO


CREATE OR ALTER PROCEDURE DefinePermissionLimits
    @MinHours INT,
    @MaxHours INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @MinHours < 0 OR @MaxHours < 0 OR @MinHours > @MaxHours
        BEGIN
            RAISERROR('Invalid range. Ensure MinHours and MaxHours are non-negative and MinHours <= MaxHours.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM SystemConfig WHERE config_key = 'PermissionMinHours')
        BEGIN
            UPDATE SystemConfig
            SET config_value = CAST(@MinHours AS VARCHAR), updated_at = GETDATE()
            WHERE config_key = 'PermissionMinHours';
        END
        ELSE
        BEGIN
            INSERT INTO SystemConfig (config_key, config_value, created_at)
            VALUES ('PermissionMinHours', CAST(@MinHours AS VARCHAR), GETDATE());
        END

        IF EXISTS (SELECT 1 FROM SystemConfig WHERE config_key = 'PermissionMaxHours')
        BEGIN
            UPDATE SystemConfig
            SET config_value = CAST(@MaxHours AS VARCHAR), updated_at = GETDATE()
            WHERE config_key = 'PermissionMaxHours';
        END
        ELSE
        BEGIN
            INSERT INTO SystemConfig (config_key, config_value, created_at)
            VALUES ('PermissionMaxHours', CAST(@MaxHours AS VARCHAR), GETDATE());
        END

        COMMIT TRANSACTION;

        PRINT 'Permission limits defined successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE EscalatePendingRequests
    @Deadline DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        UPDATE Request
        SET status = 'Escalated',
            escalated_at = GETDATE(),
            escalated_to = m.manager_id
        FROM Request r
        INNER JOIN Employee e ON r.employee_id = e.employee_id
        INNER JOIN Employee m ON e.manager_id = m.employee_id
        WHERE r.status = 'Pending'
          AND r.request_date < @Deadline;

        COMMIT TRANSACTION;

        PRINT 'Pending requests escalated successfully to higher managers.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; -- re-raise error for debugging
    END CATCH
END;
GO



ALTER PROCEDURE LinkVacationToShift
    @VacationPackageID INT,
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

       
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

               IF NOT EXISTS (SELECT 1 FROM VacationPackage WHERE package_id = @VacationPackageID)
        BEGIN
            RAISERROR('Vacation package does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        INSERT INTO VacationSchedule (
            employee_id,
            package_id,
            linked_at
        )
        VALUES (
            @EmployeeID,
            @VacationPackageID,
            GETDATE()
        );

        COMMIT TRANSACTION;

        PRINT 'Vacation package linked to employee schedule successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



CREATE OR ALTER PROCEDURE ConfigureLeavePolicies
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Replace these with actual leave_id values from your Leave table
        DECLARE @AnnualLeaveID INT = 1;
        DECLARE @SickLeaveID INT = 2;
        DECLARE @CasualLeaveID INT = 3;

        IF NOT EXISTS (SELECT 1 FROM LeavePolicy WHERE name = 'Annual Leave')
        BEGIN
            INSERT INTO LeavePolicy (
                name, purpose, notice_period, special_leave_type,
                eligibility_rules, reset_on_new_year, leave_id,
                eligibility_months, accrual_rate, max_balance, is_active
            )
            VALUES (
                'Annual Leave', 'Annual paid time off', '00:00:00', NULL,
                NULL, 1, @AnnualLeaveID,
                0, 2.5, 30.0, 1
            );
        END

        IF NOT EXISTS (SELECT 1 FROM LeavePolicy WHERE name = 'Sick Leave')
        BEGIN
            INSERT INTO LeavePolicy (
                name, purpose, notice_period, special_leave_type,
                eligibility_rules, reset_on_new_year, leave_id,
                eligibility_months, accrual_rate, max_balance, is_active
            )
            VALUES (
                'Sick Leave', 'Leave for illness', '00:00:00', NULL,
                NULL, 1, @SickLeaveID,
                0, 1.25, 15.0, 1
            );
        END

        IF NOT EXISTS (SELECT 1 FROM LeavePolicy WHERE name = 'Casual Leave')
        BEGIN
            INSERT INTO LeavePolicy (
                name, purpose, notice_period, special_leave_type,
                eligibility_rules, reset_on_new_year, leave_id,
                eligibility_months, accrual_rate, max_balance, is_active
            )
            VALUES (
                'Casual Leave', 'Short-term personal leave', '00:00:00', NULL,
                NULL, 1, @CasualLeaveID,
                0, 0.83, 10.0, 1
            );
        END

        COMMIT TRANSACTION;

        PRINT 'Leave policies configured successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO


CREATE OR ALTER PROCEDURE ApplyLeaveConfiguration
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE LeavePolicy
        SET is_active = 0
        WHERE is_active = 1;

        COMMIT TRANSACTION;

        PRINT 'Validated leave configurations applied successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO



ALTER PROCEDURE UpdateLeaveEntitlements
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmploymentDate DATE;
    DECLARE @YearsOfService INT;
    DECLARE @BaseEntitlement INT = 21; -- Default annual leave days
    DECLARE @ExtraDays INT = 0;
    DECLARE @TotalEntitlement DECIMAL(5,2);
    DECLARE @CurrentYear INT = YEAR(GETDATE());
    DECLARE @LeaveID INT = 1; -- Assuming 1 = Annual Leave

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate employee
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Get hire date
        SELECT @EmploymentDate = hire_date
        FROM Employee
        WHERE employee_id = @EmployeeID;

        -- Calculate years of service
        SET @YearsOfService = DATEDIFF(YEAR, @EmploymentDate, GETDATE());

        -- Determine extra entitlement
        IF @YearsOfService >= 5 AND @YearsOfService < 10
            SET @ExtraDays = 2;
        ELSE IF @YearsOfService >= 10
            SET @ExtraDays = 5;

        -- Total entitlement
        SET @TotalEntitlement = @BaseEntitlement + @ExtraDays;

        -- Update or insert leave entitlement
        IF EXISTS (
            SELECT 1 FROM LeaveEntitlement 
            WHERE employee_id = @EmployeeID AND leave_id = @LeaveID AND year = @CurrentYear
        )
        BEGIN
            UPDATE LeaveEntitlement
            SET allocated_days = @TotalEntitlement
            WHERE employee_id = @EmployeeID AND leave_id = @LeaveID AND year = @CurrentYear;
        END
        ELSE
        BEGIN
            INSERT INTO LeaveEntitlement (
                entitlement, employee_id, leave_id, year, allocated_days, used_days, balance_days, carry_forward_days
            )
            VALUES (
                'Annual Leave', @EmployeeID, @LeaveID, @CurrentYear, @TotalEntitlement, 0, @TotalEntitlement, 0
            );
        END

        COMMIT TRANSACTION;

        PRINT 'Leave entitlements updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO


ALTER PROCEDURE ConfigureLeaveEligibility
    @LeaveType VARCHAR(50),
    @MinTenure INT,             
    @EmployeeType VARCHAR(50)  
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @LeaveType IS NULL OR LTRIM(RTRIM(@LeaveType)) = ''
        BEGIN
            RAISERROR('Leave type cannot be empty.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @MinTenure < 0
        BEGIN
            RAISERROR('Minimum tenure must be non-negative.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @EmployeeType NOT IN ('Full-Time', 'Part-Time', 'Contract', 'Intern')
        BEGIN
            RAISERROR('Invalid employee type. Allowed values: Full-Time, Part-Time, Contract, Intern.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        INSERT INTO LeaveEligibility(leave_type, min_tenure, employee_type, created_at)
        VALUES(@LeaveType, @MinTenure, @EmployeeType, GETDATE());

        COMMIT TRANSACTION;

        PRINT 'Leave eligibility rule configured successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO


CREATE OR ALTER PROCEDURE ManageLeaveTypes
    @Action VARCHAR(20),               -- 'CreateOrUpdate', 'Update', 'Deactivate'
    @LeaveType VARCHAR(50) = NULL,
    @LeaveDescription VARCHAR(100) = NULL,
    @Description VARCHAR(500) = NULL,
    @LeaveID INT = NULL,
    @IsPaid BIT = NULL,
    @MaxDaysPerYear INT = NULL,
    @RequiresApproval BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Action = 'CreateOrUpdate' AND @LeaveType IS NOT NULL
        BEGIN
            IF EXISTS (SELECT 1 FROM Leave WHERE leave_type = @LeaveType)
            BEGIN
                UPDATE Leave
                SET 
                    leave_description = ISNULL(@LeaveDescription, leave_description),
                    description = ISNULL(@Description, description)
                  
                WHERE leave_type = @LeaveType;

                SELECT 'Leave type updated successfully.' AS message;
            END
            ELSE
            BEGIN
                INSERT INTO Leave (
                    leave_type, leave_description, description,
                    is_paid, max_days_per_year, requires_approval, is_active
                )
                VALUES (
                    @LeaveType, @LeaveDescription, @Description,
                    ISNULL(@IsPaid, 1), ISNULL(@MaxDaysPerYear, 0),
                    ISNULL(@RequiresApproval, 1), 1
                );

                SELECT SCOPE_IDENTITY() AS leave_id, 'Leave type created successfully.' AS message;
            END
        END
        ELSE IF @Action = 'Update' AND @LeaveID IS NOT NULL
        BEGIN
            UPDATE Leave
            SET 
                leave_type = ISNULL(@LeaveType, leave_type),
                leave_description = ISNULL(@LeaveDescription, leave_description),
                description = ISNULL(@Description, description),
                is_paid = ISNULL(@IsPaid, is_paid),
                max_days_per_year = ISNULL(@MaxDaysPerYear, max_days_per_year),
                requires_approval = ISNULL(@RequiresApproval, requires_approval)
            WHERE leave_id = @LeaveID;

            SELECT 'Leave type updated successfully.' AS message;
        END
        ELSE IF @Action = 'Deactivate' AND @LeaveID IS NOT NULL
        BEGIN
            UPDATE Leave SET is_active = 0 WHERE leave_id = @LeaveID;
            SELECT 'Leave type deactivated successfully.' AS message;
        END
        ELSE
        BEGIN
            RAISERROR('Invalid action or missing parameters.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE AssignLeaveEntitlement
    @EmployeeID INT,
    @LeaveID INT,
    @Entitlement DECIMAL(5,2)   -- e.g., number of days
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate employee
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate leave type
        IF NOT EXISTS (SELECT 1 FROM Leave WHERE leave_id = @LeaveID)
        BEGIN
            RAISERROR('Leave type does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate entitlement
        IF @Entitlement < 0
        BEGIN
            RAISERROR('Entitlement value cannot be negative.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DECLARE @CurrentYear INT = YEAR(GETDATE());

        -- Update or insert leave entitlement
        IF EXISTS (
            SELECT 1 FROM LeaveEntitlement 
            WHERE employee_id = @EmployeeID AND leave_id = @LeaveID AND year = @CurrentYear
        )
        BEGIN
            UPDATE LeaveEntitlement
            SET allocated_days = @Entitlement
            WHERE employee_id = @EmployeeID AND leave_id = @LeaveID AND year = @CurrentYear;
        END
        ELSE
        BEGIN
            INSERT INTO LeaveEntitlement (
                employee_id, leave_id, year, allocated_days, used_days, balance_days, carry_forward_days
            )
            VALUES (
                @EmployeeID, @LeaveID, @CurrentYear, @Entitlement, 0, @Entitlement, 0
            );
        END

        COMMIT TRANSACTION;

        PRINT 'Leave entitlement assigned successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO





ALTER PROCEDURE ConfigureSpecialLeave
    @LeaveType VARCHAR(50),     -- e.g., 'Bereavement', 'Jury Duty'
    @Rules VARCHAR(200)         -- description of rules or conditions
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

       
        IF @LeaveType IS NULL OR LTRIM(RTRIM(@LeaveType)) = ''
        BEGIN
            RAISERROR('Leave type cannot be empty.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @Rules IS NULL OR LTRIM(RTRIM(@Rules)) = ''
        BEGIN
            RAISERROR('Rules cannot be empty.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF EXISTS (SELECT 1 FROM SpecialLeave WHERE leave_type = @LeaveType)
        BEGIN
            UPDATE SpecialLeave
            SET rules = @Rules,
                updated_at = GETDATE()
            WHERE leave_type = @LeaveType;
        END
        ELSE
        BEGIN
            INSERT INTO SpecialLeave(leave_type, rules, created_at)
            VALUES(@LeaveType, @Rules, GETDATE());
        END

        COMMIT TRANSACTION;

        PRINT 'Special leave type configured successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; -- re-raise error for debugging
    END CATCH
    END;
    GO






ALTER PROCEDURE SetLeaveYearRules
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF @StartDate >= @EndDate
        BEGIN
            RAISERROR('StartDate must be earlier than EndDate.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF EXISTS (SELECT 1 FROM SystemConfig WHERE config_key = 'LeaveYearStart')
        BEGIN
            UPDATE SystemConfig
            SET config_value = CONVERT(VARCHAR, @StartDate, 23), updated_at = GETDATE()
            WHERE config_key = 'LeaveYearStart';
        END
        ELSE
        BEGIN
            INSERT INTO SystemConfig (config_key, config_value, created_at)
            VALUES ('LeaveYearStart', CONVERT(VARCHAR, @StartDate, 23), GETDATE());
        END

        IF EXISTS (SELECT 1 FROM SystemConfig WHERE config_key = 'LeaveYearEnd')
        BEGIN
            UPDATE SystemConfig
            SET config_value = CONVERT(VARCHAR, @EndDate, 23), updated_at = GETDATE()
            WHERE config_key = 'LeaveYearEnd';
        END
        ELSE
        BEGIN
            INSERT INTO SystemConfig (config_key, config_value, created_at)
            VALUES ('LeaveYearEnd', CONVERT(VARCHAR, @EndDate, 23), GETDATE());
        END

        COMMIT TRANSACTION;

        PRINT 'Leave year rules configured successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE AdjustLeaveBalance
    @EmployeeID INT,
    @LeaveType VARCHAR(50),
    @Adjustment DECIMAL(5,2)  
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        BEGIN
            RAISERROR('Employee does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @Adjustment = 0
        BEGIN
            RAISERROR('Adjustment value cannot be zero.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF EXISTS (SELECT 1 FROM LeaveBalance WHERE employee_id = @EmployeeID AND leave_type = @LeaveType)
        BEGIN
            UPDATE LeaveBalance
            SET balance = balance + @Adjustment,
                updated_at = GETDATE()
            WHERE employee_id = @EmployeeID AND leave_type = @LeaveType;
        END
        ELSE
        BEGIN
            
            INSERT INTO LeaveBalance(employee_id, leave_type, balance, created_at)
            VALUES(@EmployeeID, @LeaveType, @Adjustment, GETDATE());
        END

        COMMIT TRANSACTION;

        PRINT 'Leave balance adjusted successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW; 
    END CATCH
END;
GO



ALTER PROCEDURE ManageLeaveRoles
    @RoleID INT,
    @Permissions VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF NOT EXISTS (SELECT 1 FROM Role WHERE role_id = @RoleID)
        BEGIN
            RAISERROR('Role does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

       
        IF EXISTS (SELECT 1 FROM LeaveRolePermission WHERE role_id = @RoleID)
        BEGIN
            UPDATE LeaveRolePermission
            SET permissions = @Permissions,
                updated_at = GETDATE()
            WHERE role_id = @RoleID;

            PRINT 'Leave role permissions updated successfully.';
        END
        ELSE
        BEGIN
            INSERT INTO LeaveRolePermission (role_id, permissions, created_at)
            VALUES (@RoleID, @Permissions, GETDATE());

            PRINT 'Leave role permissions assigned successfully.';
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

ALTER PROCEDURE FinalizeLeaveRequest
    @LeaveRequestID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentStatus VARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate leave request
        IF NOT EXISTS (SELECT 1 FROM LeaveRequest WHERE request_id = @LeaveRequestID)
        BEGIN
            RAISERROR('Leave request does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Get current status
        SELECT @CurrentStatus = status
        FROM LeaveRequest
        WHERE request_id = @LeaveRequestID;

        -- Only approved requests can be finalized
        IF @CurrentStatus <> 'Approved'
        BEGIN
            RAISERROR('Only approved leave requests can be finalized.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Finalize the request
        UPDATE LeaveRequest
        SET 
            status = 'Finalized',
            approved_at = GETDATE()
        WHERE request_id = @LeaveRequestID;

        COMMIT TRANSACTION;

        PRINT 'Leave request finalized successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO


ALTER PROCEDURE OverrideLeaveDecision
    @LeaveRequestID INT,
    @Reason VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentStatus VARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate leave request
        IF NOT EXISTS (SELECT 1 FROM LeaveRequest WHERE request_id = @LeaveRequestID)
        BEGIN
            RAISERROR('Leave request does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Get current status
        SELECT @CurrentStatus = status
        FROM LeaveRequest
        WHERE request_id = @LeaveRequestID;

        -- Only rejected or pending requests can be overridden
        IF @CurrentStatus NOT IN ('Rejected', 'Pending')
        BEGIN
            RAISERROR('Only rejected or pending requests can be overridden.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- No update performed as requested

        COMMIT TRANSACTION;

        PRINT 'Override validation completed, but no changes were made.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE BulkProcessLeaveRequests
    @LeaveRequestIDs VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IDTable TABLE (LeaveRequestID INT);
    DECLARE @IDString VARCHAR(500) = @LeaveRequestIDs;
    DECLARE @NextID VARCHAR(10);
    DECLARE @Pos INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Parse comma-separated IDs into table
        WHILE LEN(@IDString) > 0
        BEGIN
            SET @Pos = CHARINDEX(',', @IDString);
            IF @Pos > 0
            BEGIN
                SET @NextID = LEFT(@IDString, @Pos - 1);
                SET @IDString = RIGHT(@IDString, LEN(@IDString) - @Pos);
            END
            ELSE
            BEGIN
                SET @NextID = @IDString;
                SET @IDString = '';
            END

            INSERT INTO @IDTable (LeaveRequestID)
            SELECT CAST(@NextID AS INT)
            WHERE ISNUMERIC(@NextID) = 1;
        END

        -- Validate all IDs exist
        IF EXISTS (
            SELECT 1
            FROM @IDTable t
            LEFT JOIN LeaveRequest lr ON t.LeaveRequestID = lr.request_id
            WHERE lr.request_id IS NULL
        )
        BEGIN
            RAISERROR('One or more leave request IDs are invalid.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Finalize approved requests
        UPDATE LeaveRequest
        SET 
            status = 'Finalized'
        WHERE request_id IN (SELECT LeaveRequestID FROM @IDTable)
          AND status = 'Approved';

        COMMIT TRANSACTION;

        PRINT 'Bulk leave request processing completed successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

GO



ALTER PROCEDURE VerifyMedicalLeave
    @LeaveRequestID INT,
    @DocumentID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate leave request
        IF NOT EXISTS (SELECT 1 FROM LeaveRequest WHERE request_id = @LeaveRequestID)
        BEGIN
            RAISERROR('Leave request does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate medical document
        IF NOT EXISTS (SELECT 1 FROM MedicalDocument WHERE document_id = @DocumentID)
        BEGIN
            RAISERROR('Medical document does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate document linkage
        IF NOT EXISTS (
            SELECT 1 
            FROM LeaveRequestDocument 
            WHERE request_id = @LeaveRequestID AND document_id = @DocumentID
        )
        BEGIN
            RAISERROR('Document is not linked to the specified leave request.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Mark document as verified
        UPDATE MedicalDocument
        SET is_verified = 1,
            verified_at = GETDATE()
        WHERE document_id = @DocumentID;

        

        COMMIT TRANSACTION;

        PRINT 'Medical leave document verified successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO





ALTER PROCEDURE SyncLeaveBalances
    @LeaveRequestID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmployeeID INT;
    DECLARE @LeaveDays INT;
    DECLARE @LeaveID INT;
    DECLARE @CurrentStatus VARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate leave request
        IF NOT EXISTS (SELECT 1 FROM LeaveRequest WHERE request_id = @LeaveRequestID)
        BEGIN
            RAISERROR('Leave request does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Get leave details
        SELECT 
            @EmployeeID = employee_id,
            @LeaveDays = DATEDIFF(DAY, start_date, end_date) + 1,
            @LeaveID = leave_id,
            @CurrentStatus = status
        FROM LeaveRequest
        WHERE request_id = @LeaveRequestID;

        -- Ensure request is finalized
        IF @CurrentStatus <> 'Finalized'
        BEGIN
            RAISERROR('Leave request must be finalized before syncing balances.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Update leave entitlement
        UPDATE LeaveEntitlement
        SET used_days = used_days + @LeaveDays
        WHERE employee_id = @EmployeeID AND leave_id = @LeaveID AND year = YEAR(GETDATE());

        COMMIT TRANSACTION;

        PRINT 'Leave balances synced successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE OverrideLeaveDecision
    @LeaveRequestID INT,
    @Reason VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentStatus VARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate leave request
        IF NOT EXISTS (SELECT 1 FROM LeaveRequest WHERE request_id = @LeaveRequestID)
        BEGIN
            RAISERROR('Leave request does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Get current status
        SELECT @CurrentStatus = status
        FROM LeaveRequest
        WHERE request_id = @LeaveRequestID;

        -- Only rejected or pending requests can be overridden
        IF @CurrentStatus NOT IN ('Rejected', 'Pending')
        BEGIN
            RAISERROR('Only rejected or pending requests can be overridden.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Apply override (status only)
        UPDATE LeaveRequest
        SET 
            status = 'Overridden'
        WHERE request_id = @LeaveRequestID;

        COMMIT TRANSACTION;

        PRINT 'Leave decision overridden successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

GO





ALTER PROCEDURE SyncLeaveToattendance
    @LeaveRequestID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmployeeID INT;
    DECLARE @StartDate DATE;
    DECLARE @EndDate DATE;
    DECLARE @Status VARCHAR(50);
    DECLARE @LeaveType VARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate leave request
        IF NOT EXISTS (SELECT 1 FROM LeaveRequest WHERE request_id = @LeaveRequestID)
        BEGIN
            RAISERROR('Leave request does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Get leave details
        SELECT 
            @EmployeeID = employee_id,
            @StartDate = start_date,
            @EndDate = end_date,
            @Status = status,
            @LeaveType = CAST(leave_id AS VARCHAR)  -- Assuming leave_id maps to exception_type
        FROM LeaveRequest
        WHERE request_id = @LeaveRequestID;

        -- Ensure request is finalized
        IF @Status <> 'Finalized'
        BEGIN
            RAISERROR('Only finalized leave requests can be synced to attendance.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert attendance exceptions
        INSERT INTO AttendanceException (employee_id, exception_date, exception_type, reference_id, created_at)
        SELECT 
            @EmployeeID,
            DATEADD(DAY, number, @StartDate),
            @LeaveType,
            @LeaveRequestID,
            GETDATE()
        FROM master.dbo.spt_values
        WHERE type = 'P'
          AND number <= DATEDIFF(DAY, @StartDate, @EndDate);

        COMMIT TRANSACTION;

        PRINT 'Leave successfully synced to attendance system.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

GO




ALTER PROCEDURE UpdateInsuranceBrackets
    @BracketID INT,
    @NewMinSalary DECIMAL(10,2),
    @NewMaxSalary DECIMAL(10,2),
    @NewEmployeeContribution DECIMAL(5,2),
    @NewEmployerContribution DECIMAL(5,2),
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF NOT EXISTS (SELECT 1 FROM InsuranceBracket WHERE bracket_id = @BracketID)
        BEGIN
            RAISERROR('Insurance bracket does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @NewMinSalary < 0 OR @NewMaxSalary <= @NewMinSalary
        BEGIN
            RAISERROR('Invalid salary range. Ensure MinSalary >= 0 and MaxSalary > MinSalary.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        IF @NewEmployeeContribution < 0 OR @NewEmployerContribution < 0
        BEGIN
            RAISERROR('Contribution values must be non-negative.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        
        UPDATE InsuranceBracket
        SET 
            min_salary = @NewMinSalary,
            max_salary = @NewMaxSalary,
            employee_contribution = @NewEmployeeContribution,
            employer_contribution = @NewEmployerContribution,
            updated_by = @UpdatedBy,
            updated_at = GETDATE()
        WHERE bracket_id = @BracketID;

        COMMIT TRANSACTION;

        PRINT 'Insurance bracket updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO



ALTER PROCEDURE ApprovePolicyUpdate
    @PolicyID INT,
    @ApprovedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM PayrollPolicy WHERE policy_id = @PolicyID)
        THROW 50001, 'Policy not found', 1;
    
    UPDATE PayrollPolicy SET is_active = 1 WHERE policy_id = @PolicyID;
    
    SELECT 'Policy update approved successfully' AS message;
END;
GO


ALTER PROCEDURE GeneratePayroll
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    
    SELECT 
        e.employee_id,
        e.full_name,
        e.base_salary,
        ISNULL(SUM(p.amount), 0) AS total_adjustments,
        e.base_salary + ISNULL(SUM(p.amount), 0) AS gross_salary
    FROM Employee e
    LEFT JOIN PayrollAdjustment p
        ON e.employee_id = p.employee_id
        AND p.effective_date BETWEEN @StartDate AND @EndDate
    WHERE e.status = 'Active'
    GROUP BY e.employee_id, e.full_name, e.base_salary;
END;
GO




ALTER PROCEDURE AdjustPayrollItem
    @PayrollID INT,
    @Type VARCHAR(50), -- 'Allowance' or 'Deduction'
    @Amount DECIMAL(10,2),
    @duration INT,
    @timezone VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        
        IF EXISTS (SELECT 1 FROM PayrollAdjustment WHERE payroll_id = @PayrollID AND type = @Type)
        BEGIN
            UPDATE PayrollAdjustment
            SET amount = @Amount,
                duration_minutes = @duration,
                timezone = @timezone,
                updated_at = GETDATE()
            WHERE payroll_id = @PayrollID AND type = @Type;

            PRINT 'Payroll item updated successfully.';
        END
        ELSE
        BEGIN
            INSERT INTO PayrollAdjustment (payroll_id, type, amount, duration_minutes, timezone, created_at)
            VALUES (@PayrollID, @Type, @Amount, @duration, @timezone, GETDATE());

            PRINT 'Payroll item added successfully.';
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


ALTER PROCEDURE CalculateNetSalary
    @PayrollID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @BaseSalary DECIMAL(10,2) = 0;
    DECLARE @TotalAllowances DECIMAL(10,2);
    DECLARE @TotalDeductions DECIMAL(10,2);
    DECLARE @GrossSalary DECIMAL(10,2);
    DECLARE @NetSalary DECIMAL(10,2);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Calculate allowances and deductions
        SELECT 
            @TotalAllowances = ISNULL(SUM(CASE WHEN item_type = 'Allowance' THEN amount ELSE 0 END), 0),
            @TotalDeductions = ISNULL(SUM(CASE WHEN item_type = 'Deduction' THEN amount ELSE 0 END), 0)
        FROM AllowanceDeduction
        WHERE payroll_id = @PayrollID;

        -- Compute gross and net salary
        SET @GrossSalary = @BaseSalary + @TotalAllowances;
        SET @NetSalary = @GrossSalary - @TotalDeductions;

        -- Update payroll record
        UPDATE Payroll
        SET 
            total_allowances = @TotalAllowances,
            total_deductions = @TotalDeductions,
            gross_salary = @GrossSalary,
            net_salary = @NetSalary
        WHERE payroll_id = @PayrollID;

        COMMIT TRANSACTION;

        -- Output result
        SELECT @NetSalary AS net_salary, 'Net salary calculated successfully' AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

GO

ALTER PROCEDURE ApplyPayrollPolicy
    @PolicyID INT,
    @PayrollID INT,
    @type VARCHAR(20),         -- 'Bonus', 'Overtime', 'Deduction'
    @description VARCHAR(50),
    @Amount DECIMAL(10,2)      -- ✅ New parameter
AS
BEGIN
    SET NOCOUNT ON;

    -- Optional: Validate policy exists
    IF NOT EXISTS (SELECT 1 FROM PayrollPolicy WHERE policy_id = @PolicyID)
    BEGIN
        RAISERROR('Invalid policy ID.', 16, 1);
        RETURN;
    END

    -- Insert adjustment
    INSERT INTO PayrollAdjustment (payroll_id, type, amount, description, created_at)
    VALUES (@PayrollID, @type, @Amount, @description, GETDATE());

    PRINT 'Payroll policy applied successfully.';
END;
GO





ALTER PROCEDURE GetMonthlyPayrollSummary
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATE;
    DECLARE @EndDate DATE;

    BEGIN TRY
        
        SET @StartDate = DATEFROMPARTS(@Year, @Month, 1);
        SET @EndDate = EOMONTH(@StartDate);

        
        SELECT 
            SUM(p.base_salary + ISNULL(adj.total_adjustments, 0)) AS TotalSalaryExpenditure
        FROM Payroll p
        LEFT JOIN (
            SELECT 
                payroll_id,
                SUM(CASE WHEN type = 'Allowance' THEN amount ELSE -amount END) AS total_adjustments
            FROM PayrollAdjustment
            GROUP BY payroll_id
        ) adj ON p.payroll_id = adj.payroll_id
        WHERE p.pay_period_start >= @StartDate AND p.pay_period_end <= @EndDate;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO






ALTER PROCEDURE GetEmployeePayrollHistory
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Payroll
    WHERE employee_id = @EmployeeID
    ORDER BY created_at DESC;
END;
GO


GO

ALTER PROCEDURE GetBonusEligibleEmployees
    @Eligibility_criteria VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Eligibility_criteria = 'TenureOver2Years'
    BEGIN
        SELECT e.employee_id, e.full_name, e.hire_date
        FROM Employee e
        WHERE DATEDIFF(YEAR, e.hire_date, GETDATE()) >= 2;
    END
    ELSE IF @Eligibility_criteria = 'PerformanceAbove90'
    BEGIN
        SELECT e.employee_id, e.full_name, p.performance_score
        FROM Employee e
        JOIN PerformanceReview p ON e.employee_id = p.employee_id
        WHERE p.review_year = YEAR(GETDATE())
          AND p.performance_score >= 90;
    END
    ELSE IF @Eligibility_criteria = 'SalesTargetMet'
    BEGIN
        SELECT e.employee_id, e.full_name, s.total_sales
        FROM Employee e
        JOIN SalesSummary s ON e.employee_id = s.employee_id
        WHERE s.period = FORMAT(GETDATE(), 'yyyyMM')
          AND s.total_sales >= s.target;
    END
    ELSE
    BEGIN
        RAISERROR('Invalid eligibility criteria specified.', 16, 1);
    END
END;
GO





ALTER PROCEDURE UpdateSalaryType
    @EmployeeID INT,
    @SalaryTypeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE employee_id = @EmployeeID)
        THROW 50001, 'Employee not found', 1;
    
    IF NOT EXISTS (SELECT 1 FROM SalaryType WHERE salarytype_id = @SalaryTypeID)
        THROW 50002, 'Salary type not found', 1;

    -- Removed: UPDATE Employee SET salarytype_id = @SalaryTypeID WHERE employee_id = @EmployeeID;

    SELECT 'Salary type update skipped — column not found' AS message;
END;
GO





CREATE OR ALTER PROCEDURE ValidateAttendanceBeforePayroll
    @PayrollPeriodID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StartDate DATE = (SELECT start_date FROM PayrollPeriod WHERE payroll_period_id = @PayrollPeriodID);
    DECLARE @EndDate DATE = (SELECT end_date FROM PayrollPeriod WHERE payroll_period_id = @PayrollPeriodID);
    
    SELECT 
        e.employee_id,
        e.first_name + ' ' + e.last_name AS employee_name,
        COUNT(a.attendance_id) AS attendance_days,
        SUM(a.hours_worked) AS total_hours,
        SUM(a.overtime_hours) AS total_overtime
    FROM Employee e
    LEFT JOIN Attendance a ON e.employee_id = a.employee_id 
        AND a.attendance_date BETWEEN @StartDate AND @EndDate
    WHERE e.is_active = 1
    GROUP BY e.employee_id, e.first_name, e.last_name
    HAVING COUNT(a.attendance_id) < DATEDIFF(DAY, @StartDate, @EndDate) * 0.8;
END;
GO



CREATE OR ALTER PROCEDURE SyncAttendanceToPayroll
    @SyncDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Example: Update payroll hours based on attendance
    UPDATE p
    SET total_hours = a.total_hours
    FROM Payroll p
    JOIN AttendanceSummary a ON p.employee_id = a.employee_id
    WHERE a.attendance_date = @SyncDate;

    PRINT 'Attendance synced to payroll successfully.';
END;
GO




CREATE OR ALTER PROCEDURE SyncApprovedPermissionsToPayroll
    @PayrollPeriodID INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PayrollPermissionLog (payroll_id, permission_type, hours, approved_by, created_at)
    SELECT 
        p.payroll_id,
        perm.permission_type,
        perm.hours,
        perm.approved_by,
        GETDATE()
    FROM Permission perm
    JOIN Payroll p ON perm.employee_id = p.employee_id
    WHERE perm.status = 'Approved'
      AND p.payroll_period_id = @PayrollPeriodID;

    PRINT 'Approved permissions synced to payroll.';
END;
GO



CREATE OR ALTER PROCEDURE ConfigurePayGrades
    @GradeName VARCHAR(50),
    @MinSalary DECIMAL(10,2),
    @MaxSalary DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PayGrade (grade_name, min_salary, max_salary, created_at)
    VALUES (@GradeName, @MinSalary, @MaxSalary, GETDATE());

    PRINT 'Pay grade configured successfully.';
END;
GO




CREATE OR ALTER PROCEDURE ConfigureShiftAllowances
    @ShiftType VARCHAR(50),
    @AllowanceName VARCHAR(50),
    @Amount DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ShiftAllowance (shift_type, allowance_name, amount, created_at)
    VALUES (@ShiftType, @AllowanceName, @Amount, GETDATE());

    PRINT 'Shift allowance configured successfully.';
END;
GO




CREATE OR ALTER PROCEDURE EnableMultiCurrencyPayroll
    @CurrencyCode VARCHAR(10),
    @ExchangeRate DECIMAL(10,4)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO CurrencyExchange (currency_code, exchange_rate, effective_date)
    VALUES (@CurrencyCode, @ExchangeRate, GETDATE());

    PRINT 'Multi-currency payroll enabled.';
END;
GO



CREATE OR ALTER PROCEDURE ManageTaxRules
    @TaxRuleName VARCHAR(50),
    @CountryCode VARCHAR(10),
    @Rate DECIMAL(5,2),
    @Exemption DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO TaxRule (rule_name, country_code, rate, exemption, created_at)
    VALUES (@TaxRuleName, @CountryCode, @Rate, @Exemption, GETDATE());

    PRINT 'Tax rule configured successfully.';
END;
GO




CREATE OR ALTER PROCEDURE ApprovePayrollConfigChanges
    @ConfigID INT,
    @ApproverID INT,
    @Status VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE PayrollConfigChange
    SET approval_status = @Status,
        approved_by = @ApproverID,
        approved_at = GETDATE()
    WHERE config_id = @ConfigID;

    PRINT 'Payroll configuration change approval recorded.';
END;
GO




CREATE OR ALTER PROCEDURE ConfigureSigningBonus
    @EmployeeID INT,
    @BonusAmount DECIMAL(10,2),
    @EffectiveDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SigningBonus (employee_id, bonus_amount, effective_date, created_at)
    VALUES (@EmployeeID, @BonusAmount, @EffectiveDate, GETDATE());

    PRINT 'Signing bonus configured successfully.';
END;
GO




CREATE OR ALTER PROCEDURE ConfigureTerminationBenefits
    @EmployeeID INT,
    @CompensationAmount DECIMAL(10,2),
    @EffectiveDate DATE,
    @Reason VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO TerminationBenefit (employee_id, compensation_amount, effective_date, reason, created_at)
    VALUES (@EmployeeID, @CompensationAmount, @EffectiveDate, @Reason, GETDATE());

    PRINT 'Termination benefits configured successfully.';
END;
GO

	


CREATE OR ALTER PROCEDURE ConfigureInsuranceBrackets
    @InsuranceType VARCHAR(50),
    @MinSalary DECIMAL(10,2),
    @MaxSalary DECIMAL(10,2),
    @EmployeeContribution DECIMAL(5,2),
    @EmployerContribution DECIMAL(5,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO InsuranceBracket (
        insurance_type, min_salary, max_salary,
        employee_contribution, employer_contribution, created_at
    )
    VALUES (
        @InsuranceType, @MinSalary, @MaxSalary,
        @EmployeeContribution, @EmployerContribution, GETDATE()
    );

    PRINT 'Insurance bracket configured successfully.';
END;
GO

CREATE OR ALTER PROCEDURE UpdateInsuranceBrackets
    @BracketID INT,
    @MinSalary DECIMAL(10,2),
    @MaxSalary DECIMAL(10,2),
    @EmployeeContribution DECIMAL(5,2),
    @EmployerContribution DECIMAL(5,2)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE InsuranceBracket
    SET 
        min_salary = @MinSalary,
        max_salary = @MaxSalary,
        employee_contribution = @EmployeeContribution,
        employer_contribution = @EmployerContribution,
        updated_at = GETDATE()
    WHERE bracket_id = @BracketID;

    PRINT 'Insurance bracket updated successfully.';
END;
GO



CREATE OR ALTER PROCEDURE ConfigurePayrollPolicies
    @PolicyType VARCHAR(50),
    @PolicyDetails NVARCHAR(MAX), -- Optional: can be removed if unused
    @effectivedate DATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PayrollPolicy (policy_type, effective_date)
    VALUES (@PolicyType, @effectivedate);

    PRINT 'Payroll policy configured successfully.';
END;
GO



CREATE OR ALTER PROCEDURE DefinePayGrades
    @GradeName VARCHAR(50),
    @MinSalary DECIMAL(10,2),
    @MaxSalary DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PayGrade (
        grade_name, min_salary, max_salary, created_at
    )
    VALUES (
        @GradeName, @MinSalary, @MaxSalary, GETDATE()
    );

    PRINT 'Pay grade defined successfully.';
END;
GO




CREATE OR ALTER PROCEDURE ConfigureEscalationWorkflow
    @ThresholdAmount DECIMAL(10,2),
    @ApproverRole VARCHAR(50),
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO EscalationWorkflow (
        threshold_amount, approver_role, created_by, created_at
    )
    VALUES (
        @ThresholdAmount, @ApproverRole, @CreatedBy, GETDATE()
    );

    PRINT 'Escalation workflow configured successfully.';
END;
GO

CREATE OR ALTER PROCEDURE DefinePayType
    @EmployeeID INT,
    @PayType VARCHAR(50),
    @EffectiveDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO EmployeePayType (
        employee_id, pay_type, effective_date, created_at
    )
    VALUES (
        @EmployeeID, @PayType, @EffectiveDate, GETDATE()
    );

    PRINT 'Employee pay type defined successfully.';
END;
GO


CREATE OR ALTER PROCEDURE ConfigureOvertimeRules
    @DayType VARCHAR(20),
    @Multiplier DECIMAL(3,2),
    @hourspermonth INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO OvertimeRule (
        day_type, multiplier, hours_per_month, created_at
    )
    VALUES (
        @DayType, @Multiplier, @hourspermonth, GETDATE()
    );

    PRINT 'Overtime rule configured successfully.';
END;
GO


CREATE OR ALTER PROCEDURE ConfigureShiftAllowance
    @ShiftType VARCHAR(20),
    @AllowanceAmount DECIMAL(10,2),
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
       
        IF EXISTS (SELECT 1 FROM ShiftAllowance WHERE shift_type = @ShiftType)
        BEGIN
            UPDATE ShiftAllowance
            SET 
                allowance_amount = @AllowanceAmount,
                updated_by = @CreatedBy,
                updated_at = GETDATE()
            WHERE shift_type = @ShiftType;

            PRINT 'Shift allowance updated successfully.';
        END
        ELSE
        BEGIN
            INSERT INTO ShiftAllowance (
                shift_type, allowance_amount, created_by, created_at
            )
            VALUES (
                @ShiftType, @AllowanceAmount, @CreatedBy, GETDATE()
            );

            PRINT 'Shift allowance configured successfully.';
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO
CREATE OR ALTER PROCEDURE ConfigureSigningBonusPolicy
    @BonusType VARCHAR(50),
    @Amount DECIMAL(10,2),
    @EligibilityCriteria NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        
        IF EXISTS (SELECT 1 FROM SigningBonusPolicy WHERE bonus_type = @BonusType)
        BEGIN
            UPDATE SigningBonusPolicy
            SET 
                amount = @Amount,
                eligibility_criteria = @EligibilityCriteria,
                updated_at = GETDATE()
            WHERE bonus_type = @BonusType;

            PRINT 'Signing bonus policy updated successfully.';
        END
        ELSE
        BEGIN
            INSERT INTO SigningBonusPolicy (
                bonus_type, amount, eligibility_criteria, created_at
            )
            VALUES (
                @BonusType, @Amount, @EligibilityCriteria, GETDATE()
            );

            PRINT 'Signing bonus policy configured successfully.';
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO



CREATE OR ALTER PROCEDURE GenerateTaxStatement
    @EmployeeID INT,
    @TaxYear INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.employee_id,
        e.first_name + ' ' + e.last_name AS employee_name,
        e.employee_code,
        pp.period_name,
        p.gross_salary,
        p.total_deductions,
        p.net_salary
    FROM Payroll p
    INNER JOIN Employee e ON p.employee_id = e.employee_id
    INNER JOIN PayrollPeriod pp ON p.payroll_id = pp.payroll_id
    WHERE e.employee_id = @EmployeeID
      AND YEAR(pp.start_date) = @TaxYear
      AND p.status = 'Finalized'
    ORDER BY pp.start_date;
END;
GO



CREATE OR ALTER PROCEDURE ApprovePayrollConfiguration
    @ConfigID INT,
    @ApprovedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    EXEC ApprovePayrollConfigChanges @ConfigID, @ApprovedBy;
END;
GO


CREATE OR ALTER PROCEDURE ModifyPastPayroll
    @PayrollRunID INT,
    @EmployeeID INT,
    @FieldName VARCHAR(50),
    @NewValue DECIMAL(10,2),
    @ModifiedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @ValidField BIT = 0;

    
    IF @FieldName IN ('base_salary', 'bonus', 'deduction', 'overtime', 'net_salary')
        SET @ValidField = 1;

    IF @ValidField = 0
    BEGIN
        RAISERROR('Invalid field name. Modification not permitted.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        
        SET @SQL = '
            UPDATE Payroll
            SET ' + QUOTENAME(@FieldName) + ' = @NewValue,
                modified_by = @ModifiedBy,
                modified_at = GETDATE()
            WHERE payroll_run_id = @PayrollRunID AND employee_id = @EmployeeID;
        ';

        EXEC sp_executesql 
            @SQL,
            N'@NewValue DECIMAL(10,2), @ModifiedBy INT, @PayrollRunID INT, @EmployeeID INT',
            @NewValue = @NewValue,
            @ModifiedBy = @ModifiedBy,
            @PayrollRunID = @PayrollRunID,
            @EmployeeID = @EmployeeID;

        COMMIT TRANSACTION;

        PRINT 'Payroll entry modified successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


CREATE OR ALTER PROCEDURE ReviewLeaveRequest
    @LeaveRequestID INT,
    @ManagerID INT,
    @Decision VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LeaveRequest
    SET 
        status = @Decision
    WHERE request_id = @LeaveRequestID;

    SELECT 
        @LeaveRequestID AS LeaveRequestID, 
        @ManagerID AS ManagerID, 
        @Decision AS Decision;
END;
GO


	



CREATE OR ALTER PROCEDURE AssignShift
    @EmployeeID INT,
    @ShiftID INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO EmployeeShift (employee_id, shift_id, assigned_at)
    VALUES (@EmployeeID, @ShiftID, GETDATE());

    PRINT 'Shift assigned successfully.';
END;
GO


CREATE OR ALTER PROCEDURE ViewTeamAttendance
    @ManagerID INT,
    @DateRangeStart DATE,
    @DateRangeEnd DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.employee_id, 
        e.full_name, 
        a.attendance_date
    FROM Attendance a
    JOIN Employee e ON a.employee_id = e.employee_id
    WHERE e.manager_id = @ManagerID
      AND a.attendance_date BETWEEN @DateRangeStart AND @DateRangeEnd;
END;
GO

GO


CREATE OR ALTER PROCEDURE SendTeamNotification
    @ManagerID INT,
    @MessageContent VARCHAR(255),
    @UrgencyLevel VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO TeamNotification (manager_id, message_content, urgency_level, sent_at)
    VALUES (@ManagerID, @MessageContent, @UrgencyLevel, GETDATE());

    PRINT 'Notification sent to team.';
END;
GO


CREATE OR ALTER PROCEDURE ApproveMissionCompletion
    @MissionID INT,
    @ManagerID INT,
    @Remarks VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Mission
    SET status = 'Completed'
    WHERE mission_id = @MissionID;

    PRINT 'Mission marked as completed.';
END;
GO

GO


CREATE OR ALTER PROCEDURE RequestReplacement
    @EmployeeID INT,
    @Reason VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ReplacementRequest (employee_id, reason, requested_at)
    VALUES (@EmployeeID, @Reason, GETDATE());

    PRINT 'Replacement request submitted.';
END;
GO


CREATE OR ALTER PROCEDURE ViewDepartmentSummary
    @DepartmentID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.department_name,
        COUNT(DISTINCT e.employee_id) AS employee_count,
        COUNT(DISTINCT p.project_id) AS active_projects
    FROM Department d
    LEFT JOIN Employee e ON d.department_id = e.department_id
    LEFT JOIN Project p ON d.department_id = p.department_id AND p.status = 'Active'
    WHERE d.department_id = @DepartmentID
    GROUP BY d.department_name;
END;
GO


CREATE OR ALTER PROCEDURE ReassignShift
    @EmployeeID INT,
    @OldShiftID INT,
    @NewShiftID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE EmployeeShift
    SET shift_id = @NewShiftID,
        reassigned_at = GETDATE()
    WHERE employee_id = @EmployeeID AND shift_id = @OldShiftID;

    PRINT 'Shift reassigned successfully.';
END;
GO


CREATE OR ALTER PROCEDURE GetPendingLeaveRequests
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lr.request_id,
        lr.employee_id,
        e.first_name + ' ' + e.last_name AS employee_name,
        lr.start_date,
        lr.end_date,
        lr.total_days,
        lr.reason,
        lr.status,
        lr.submitted_at
    FROM LeaveRequest lr
    INNER JOIN Employee e ON lr.employee_id = e.employee_id
    INNER JOIN Leave l ON lr.leave_id = l.leave_id
    WHERE e.manager_id = @ManagerID
      AND lr.status IN ('Submitted', 'Pending')
    ORDER BY lr.submitted_at;
END;
GO



CREATE OR ALTER PROCEDURE GetTeamStatistics
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(DISTINCT e.employee_id) AS team_size,
        AVG(e.base_salary) AS avg_salary,
        COUNT(DISTINCT CASE WHEN a.status = 'Present' THEN a.attendance_date END) AS total_attendance_days,
        COUNT(DISTINCT CASE WHEN lr.status = 'Approved' THEN lr.request_id END) AS approved_leaves
    FROM Employee e
    LEFT JOIN Attendance a ON e.employee_id = a.employee_id AND a.attendance_date >= DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
    LEFT JOIN LeaveRequest lr ON e.employee_id = lr.employee_id
    WHERE e.manager_id = @ManagerID
    AND e.is_active = 1;
END;
GO


CREATE OR ALTER PROCEDURE ViewTeamProfiles
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.employee_id,
        e.employee_code,
        e.first_name + ' ' + e.last_name AS employee_name,
        e.email,
        e.phone,
        e.hire_date
    FROM Employee e
    LEFT JOIN Position p ON e.position_id = p.position_id
    WHERE e.manager_id = @ManagerID
      AND e.is_active = 1
    ORDER BY e.first_name, e.last_name;
END;
GO



CREATE OR ALTER PROCEDURE GetTeamSummary
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    EXEC GetTeamStatistics @ManagerID;
END;
GO


CREATE OR ALTER PROCEDURE FilterTeamProfiles
    @ManagerID INT,
    @Skill VARCHAR(50),
    @RoleID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT e.employee_id, e.full_name, e.job_title
    FROM Employee e
    JOIN EmployeeSkill es ON e.employee_id = es.employee_id
    WHERE e.manager_id = @ManagerID
      AND es.skill_name = @Skill
      AND e.role_id = @RoleID;
END;
GO


CREATE OR ALTER PROCEDURE ViewTeamCertifications
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.employee_id,
        e.first_name + ' ' + e.last_name AS employee_name,
        ev.verification_date,
        ev.expiry_date,
        ev.status
    FROM Employee e
    INNER JOIN Employee_Verification ev ON e.employee_id = ev.employee_id
    INNER JOIN Verification v ON ev.verification_id = v.verification_id
    WHERE e.manager_id = @ManagerID
    ORDER BY e.first_name, e.last_name, ev.expiry_date;
END;
GO




CREATE OR ALTER PROCEDURE AddManagerNotes
    @EmployeeID INT,
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ManagerNotes (employee_id, manager_id, created_at)
    VALUES (@EmployeeID, @ManagerID, GETDATE());

    PRINT 'Manager note added successfully.';
END;
GO

CREATE OR ALTER PROCEDURE RecordManualAttendance
    @EmployeeID INT,
    @Date DATE,
    @ClockIn TIME,
    @ClockOut TIME,
    @Reason VARCHAR(200),
    @RecordedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ManualAttendance (
        employee_id, attendance_date, clock_in, clock_out, reason, recorded_by, recorded_at
    )
    VALUES (
        @EmployeeID, @Date, @ClockIn, @ClockOut, @Reason, @RecordedBy, GETDATE()
    );

    PRINT 'Manual attendance recorded with audit trail.';
END;
GO

CREATE OR ALTER PROCEDURE ReviewMissedPunches
    @ManagerID INT,
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.employee_id,
        e.full_name,
        a.attendance_date
    FROM Attendance a
    JOIN Employee e ON a.employee_id = e.employee_id
    WHERE e.manager_id = @ManagerID
      AND a.attendance_date = @Date;
END;
GO


CREATE OR ALTER PROCEDURE ApproveTimeRequest
    @RequestID INT,
    @ManagerID INT,
    @Decision VARCHAR(20),
    @Comments VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TimeRequest
    SET status = @Decision,
        reviewed_by = @ManagerID,
        review_comments = @Comments,
        reviewed_at = GETDATE()
    WHERE request_id = @RequestID;

    PRINT 'Time management request processed.';
END;
GO

CREATE OR ALTER PROCEDURE ApproveLeaveRequest
    @LeaveRequestID INT,
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LeaveRequest
    SET status = 'Approved'
    WHERE request_id = @LeaveRequestID;

    PRINT 'Leave request approved.';
END;
GO




CREATE OR ALTER PROCEDURE RejectLeaveRequest
    @LeaveRequestID INT,
    @ManagerID INT,
    @Reason VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LeaveRequest
    SET status = 'Rejected'
    WHERE request_id = @LeaveRequestID;

    PRINT 'Leave request rejected.';
END;
GO




CREATE OR ALTER PROCEDURE DelegateLeaveApproval
    @ManagerID INT,
    @DelegateID INT,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LeaveApprovalDelegation (
        manager_id, delegate_id, start_date, end_date, created_at
    )
    VALUES (
        @ManagerID, @DelegateID, @StartDate, @EndDate, GETDATE()
    );

    PRINT 'Leave approval delegation configured.';
END;
GO

CREATE OR ALTER PROCEDURE FlagIrregularLeave
    @EmployeeID INT,
    @ManagerID INT,
    @PatternDescription VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO IrregularLeaveFlag (
        employee_id, manager_id, pattern_description, flagged_at
    )
    VALUES (
        @EmployeeID, @ManagerID, @PatternDescription, GETDATE()
    );

    PRINT 'Irregular leave pattern flagged.';
END;
GO

CREATE OR ALTER PROCEDURE NotifyNewLeaveRequest
    @ManagerID INT,
    @RequestID INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LeaveNotification (
        manager_id, request_id, notification_time
    )
    VALUES (
        @ManagerID, @RequestID, GETDATE()
    );

    PRINT 'Manager notified of new leave request.';
END;
GO


CREATE OR ALTER PROCEDURE SubmitLeaveRequest
    @EmployeeID INT,
    @StartDate DATE,
    @EndDate DATE,
    @Reason VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LeaveRequest (
        employee_id, start_date, end_date, reason, status, submitted_at
    )
    VALUES (
        @EmployeeID, @StartDate, @EndDate, @Reason, 'Pending', GETDATE()
    );

    PRINT 'Leave request submitted successfully.';
END;
GO


CREATE OR ALTER PROCEDURE GetLeaveBalance
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        *
    FROM LeaveEntitlement
    WHERE employee_id = @EmployeeID;
END;
GO


CREATE OR ALTER PROCEDURE RecordAttendance
    @EmployeeID INT,
    @ShiftID INT,
    @EntryTime TIME,
    @ExitTime TIME
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Attendance (
        employee_id, shift_id, attendance_date
    )
    VALUES (
        @EmployeeID, @ShiftID, CAST(GETDATE() AS DATE)
    );

    PRINT 'Attendance recorded successfully.';
END;
GO


CREATE OR ALTER PROCEDURE SubmitReimbursement
    @EmployeeID INT,
    @ExpenseType VARCHAR(50),
    @Amount DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Reimbursement (
        employee_id
    )
    VALUES (
        @EmployeeID
    );

    PRINT 'Reimbursement request submitted.';
END;
GO


CREATE OR ALTER PROCEDURE AddEmployeeSkill
    @EmployeeID INT,
    @SkillName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO EmployeeSkill (employee_id, skill_name, added_at)
    VALUES (@EmployeeID, @SkillName, GETDATE());

    PRINT 'Skill added successfully.';
END;
GO

CREATE OR ALTER PROCEDURE ViewAssignedShifts
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sa.assignment_id,
        ss.shift_id,
        ss.start_time,
        ss.end_time,
        sa.start_date,
        sa.end_date,
        sa.is_active
    FROM ShiftAssignment sa
    INNER JOIN ShiftSchedule ss ON sa.shift_id = ss.shift_id
    WHERE sa.employee_id = @EmployeeID
    ORDER BY sa.start_date DESC;
END;
GO


CREATE OR ALTER PROCEDURE ViewMyContracts
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.contract_id,
        c.contract_type,
        c.start_date,
        c.end_date
    FROM Contract c
    WHERE c.employee_id = @EmployeeID
    ORDER BY c.start_date DESC;
END;
GO


CREATE OR ALTER PROCEDURE ViewMyPayroll
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Payroll
    WHERE employee_id = @EmployeeID;
END;
GO


CREATE OR ALTER PROCEDURE UpdatePersonalDetails
    @EmployeeID INT,
    @Phone VARCHAR(20),
    @Address VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Employee
    SET phone = @Phone,
        address = @Address,
        updated_at = GETDATE()
    WHERE employee_id = @EmployeeID;

    PRINT 'Personal details updated successfully.';
END;
GO

CREATE OR ALTER PROCEDURE ViewEmployeeProfile
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    EXEC ViewEmployeeInfo @EmployeeID;
END;
GO

CREATE OR ALTER PROCEDURE UpdateContactInformation
    @EmployeeID INT,
    @RequestType VARCHAR(50),
    @NewValue VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF @RequestType = 'Phone'
    BEGIN
        UPDATE Employee
        SET phone = @NewValue,
            updated_at = GETDATE()
        WHERE employee_id = @EmployeeID;
    END
    ELSE IF @RequestType = 'Address'
    BEGIN
        UPDATE Employee
        SET address = @NewValue,
            updated_at = GETDATE()
        WHERE employee_id = @EmployeeID;
    END
    ELSE
    BEGIN
        RAISERROR('Invalid request type. Use Phone or Address.', 16, 1);
        RETURN;
    END

    PRINT 'Contact information updated successfully.';
END;
GO

CREATE OR ALTER PROCEDURE ViewEmploymentTimeline
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Hire' AS event_type,
        hire_date AS event_date,
        'Employee hired' AS description
    FROM Employee
    WHERE employee_id = @EmployeeID
    
    UNION ALL
    
    SELECT 
        'Contract' AS event_type,
        start_date AS event_date,
        contract_type + ' contract started' AS description
    FROM Contract
    WHERE employee_id = @EmployeeID
    
    UNION ALL
    
    SELECT 
        'Promotion' AS event_type,
        updated_at AS event_date,
        'Position updated' AS description
    FROM Employee
    WHERE employee_id = @EmployeeID
    
    ORDER BY event_date DESC;
END;
GO

CREATE OR ALTER PROCEDURE UpdateEmergencyContact
    @EmployeeID INT,
    @ContactName VARCHAR(100),
    @Relation VARCHAR(50),
    @Phone VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM EmergencyContact WHERE employee_id = @EmployeeID)
    BEGIN
        UPDATE EmergencyContact
        SET contact_name = @ContactName,
            relation = @Relation,
            phone = @Phone,
            updated_at = GETDATE()
        WHERE employee_id = @EmployeeID;
    END
    ELSE
    BEGIN
        INSERT INTO EmergencyContact (
            employee_id, contact_name, relation, phone, created_at
        )
        VALUES (
            @EmployeeID, @ContactName, @Relation, @Phone, GETDATE()
        );
    END

    PRINT 'Emergency contact updated successfully.';
END;
GO
ALTER PROCEDURE RequestHRDocument
    @EmployeeID INT,
    @DocumentType VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO HRDocumentRequest (
        employee_id, document_type, status, requested_at
    )
    VALUES (
        @EmployeeID, @DocumentType, 'Pending', GETDATE()
    );

    PRINT 'HR document request submitted.';
END;
GO


ALTER PROCEDURE NotifyProfileUpdate
    @EmployeeID INT,
    @NotificationType VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ProfileNotification (
        employee_id, notification_type, notification_time
    )
    VALUES (
        @EmployeeID, @NotificationType, GETDATE()
    );

    PRINT 'Profile update notification recorded.';
END;
GO

ALTER PROCEDURE LogFlexibleAttendance
    @EmployeeID INT,
    @Date DATE,
    @CheckIn TIME,
    @CheckOut TIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalHours DECIMAL(5,2);
    SET @TotalHours = DATEDIFF(MINUTE, @CheckIn, @CheckOut) / 60.0;

    INSERT INTO FlexibleAttendance (
        employee_id, attendance_date, check_in, check_out, total_hours, logged_at
    )
    VALUES (
        @EmployeeID, @Date, @CheckIn, @CheckOut, @TotalHours, GETDATE()
    );

    PRINT 'Flexible attendance logged. Total hours: ' + CAST(@TotalHours AS VARCHAR);
END;
GO

ALTER PROCEDURE NotifyMissedPunch
    @EmployeeID INT,
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO MissedPunchNotification (
        employee_id, attendance_date, notification_time
    )
    VALUES (
        @EmployeeID, @Date, GETDATE()
    );

    PRINT 'Missed punch notification sent.';
END;
GO

ALTER PROCEDURE RecordMultiplePunches
    @EmployeeID INT,
    @ClockInOutTime DATETIME,
    @Type VARCHAR(10) -- 'IN' or 'OUT'
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AttendancePunch (
        employee_id, punch_time, punch_type, recorded_at
    )
    VALUES (
        @EmployeeID, @ClockInOutTime, @Type, GETDATE()
    );

    PRINT 'Punch recorded successfully.';
END;
GO
ALTER PROCEDURE SubmitCorrectionRequest
    @EmployeeID INT,
    @Date DATE,
    @CorrectionType VARCHAR(50),
    @Reason VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AttendanceCorrectionRequest (
        employee_id, attendance_date, correction_type, reason, status
    )
    VALUES (
        @EmployeeID, @Date, @CorrectionType, @Reason, 'Pending'
    );

    PRINT 'Correction request submitted.';
END;
GO


ALTER PROCEDURE ViewRequestStatus
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        'Correction' AS RequestType,
        request_id AS RequestID,
        attendance_date AS RequestDate,
        correction_type AS RequestDetail,
        status AS RequestStatus
    FROM AttendanceCorrectionRequest
    WHERE employee_id = @EmployeeID

    UNION ALL

    SELECT 
        'Overtime' AS RequestType,
        request_id AS RequestID,
        overtime_date AS RequestDate,
        hours_requested AS RequestDetail,
        status AS RequestStatus
    FROM OvertimeRequest
    WHERE employee_id = @EmployeeID;
END;
GO


ALTER PROCEDURE AttachLeaveDocuments
    @LeaveRequestID INT,
    @FilePath VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LeaveDocuments (
        leave_request_id, file_path, uploaded_at
    )
    VALUES (
        @LeaveRequestID, @FilePath, GETDATE()
    );

    PRINT 'Document attached to leave request successfully.';
END;
GO
ALTER PROCEDURE ModifyLeaveRequest
    @RequestID INT,
    @StartDate DATE,
    @EndDate DATE,
    @Reason VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LeaveRequest
    SET start_date = @StartDate,
        end_date = @EndDate,
        reason = @Reason
    WHERE request_id = @RequestID
      AND status = 'Pending';

    PRINT 'Leave request modified successfully.';
END;
GO


ALTER PROCEDURE CancelLeaveRequest
    @RequestID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LeaveRequest
    SET status = 'Cancelled'
    WHERE request_id = @RequestID
      AND status = 'Pending';

    PRINT 'Leave request cancelled successfully.';
END;
GO


ALTER PROCEDURE ViewLeaveBalance
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM LeaveEntitlement
    WHERE employee_id = @EmployeeID;
END;
GO

ALTER PROCEDURE ViewLeaveHistory
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        request_id,
        leave_id,
        start_date,
        end_date,
        reason,
        status,
        submitted_at,
        approved_at
    FROM LeaveRequest
    WHERE employee_id = @EmployeeID
    ORDER BY submitted_at DESC;
END;
GO

ALTER PROCEDURE SubmitLeaveAfterAbsence
    @EmployeeID INT,
    @LeaveTypeID INT,   -- this maps to leave_id
    @StartDate DATE,
    @EndDate DATE,
    @Reason VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if more than 7 days have passed
    IF DATEDIFF(DAY, @EndDate, GETDATE()) > 7
    BEGIN
        RAISERROR('Leave requests after absence must be submitted within 7 days.', 16, 1);
        RETURN;
    END;

    INSERT INTO LeaveRequest (
        employee_id, 
        leave_id,              -- corrected
        start_date, 
        end_date, 
        reason, 
        status, 
        submitted_at, 
        is_irregular           -- closest match to "backdated"
    )
    VALUES (
        @EmployeeID, 
        @LeaveTypeID, 
        @StartDate, 
        @EndDate, 
        @Reason, 
        'Pending', 
        GETDATE(), 
        1                      -- mark as irregular/backdated
    );

    PRINT 'Backdated leave request submitted successfully.';
END;
GO


ALTER PROCEDURE NotifyLeaveStatusChange
    @EmployeeID INT,
    @RequestID INT,
    @Status VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LeaveStatusNotification (
        employee_id, leave_request_id, status, notification_time
    )
    VALUES (
        @EmployeeID, @RequestID, @Status, GETDATE()
    );

    PRINT 'Leave status notification sent.';
END;
GO




