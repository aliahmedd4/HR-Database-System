-- Script to check for triggers and fix the AssignMission stored procedure
-- Run this script in SQL Server Management Studio against your database

USE [HR_Payroll_System]; -- Your database name from appsettings
GO

-- Step 1: Check for any triggers on the Mission table
PRINT 'Checking for triggers on Mission table...';
SELECT 
    t.name AS TriggerName,
    OBJECT_NAME(t.parent_id) AS TableName,
    t.is_disabled AS IsDisabled,
    OBJECT_DEFINITION(t.object_id) AS TriggerDefinition
FROM sys.triggers t
WHERE OBJECT_NAME(t.parent_id) = 'Mission';
GO

-- Step 2: Drop any triggers that might be calling the stored procedure (if found)
-- Uncomment the following lines if triggers are found and you want to drop them:
/*
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'tr_Mission_Insert' AND parent_id = OBJECT_ID('Mission'))
BEGIN
    DROP TRIGGER tr_Mission_Insert;
    PRINT 'Dropped trigger tr_Mission_Insert';
END
GO
*/

-- Step 3: Check current stored procedure definition
PRINT 'Checking current AssignMission stored procedure...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssignMission]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'AssignMission stored procedure exists. Checking if it references MissionAssignment...';
    IF EXISTS (
        SELECT * FROM sys.sql_modules 
        WHERE object_id = OBJECT_ID(N'[dbo].[AssignMission]') 
        AND definition LIKE '%MissionAssignment%'
    )
    BEGIN
        PRINT 'WARNING: Stored procedure still references MissionAssignment table!';
    END
    ELSE
    BEGIN
        PRINT 'Stored procedure appears to be correct.';
    END
END
ELSE
BEGIN
    PRINT 'AssignMission stored procedure does not exist.';
END
GO

-- Step 4: Drop and recreate the stored procedure with correct definition
PRINT 'Fixing AssignMission stored procedure...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssignMission]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[AssignMission]
GO

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

        -- Insert into Mission table (not MissionAssignment)
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

PRINT 'AssignMission stored procedure has been fixed successfully!';
GO

-- Step 5: Verify the fix
PRINT 'Verifying stored procedure fix...';
IF EXISTS (
    SELECT * FROM sys.sql_modules 
    WHERE object_id = OBJECT_ID(N'[dbo].[AssignMission]') 
    AND definition LIKE '%INSERT INTO Mission%'
    AND definition NOT LIKE '%MissionAssignment%'
)
BEGIN
    PRINT 'SUCCESS: Stored procedure is now using the correct Mission table.';
END
ELSE
BEGIN
    PRINT 'WARNING: Verification failed. Please check the stored procedure manually.';
END
GO

