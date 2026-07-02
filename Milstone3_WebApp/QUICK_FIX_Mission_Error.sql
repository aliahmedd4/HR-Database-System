-- QUICK FIX for "Invalid object name 'MissionAssignment'" error
-- Run this script in SQL Server Management Studio
-- Database: HR_Payroll_System

USE [HR_Payroll_System];
GO

-- Step 1: Drop the old broken stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssignMission]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[AssignMission];
    PRINT 'Dropped old AssignMission stored procedure';
END
GO

-- Step 2: Create the fixed stored procedure
CREATE PROCEDURE AssignMission
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

        -- Insert into Mission table (CORRECTED - was MissionAssignment)
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

PRINT 'SUCCESS: AssignMission stored procedure has been fixed!';
PRINT 'The procedure now uses the correct Mission table instead of MissionAssignment.';
GO

