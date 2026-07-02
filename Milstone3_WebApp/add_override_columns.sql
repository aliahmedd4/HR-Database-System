-- Migration script to add OverrideReason and OverriddenAt columns to LeaveRequest table
-- Run this script on your database to fix the SqlException

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'LeaveRequest' AND COLUMN_NAME = 'override_reason')
BEGIN
    ALTER TABLE LeaveRequest
    ADD override_reason VARCHAR(200) NULL;
    PRINT 'Column override_reason added to LeaveRequest table.';
END
ELSE
BEGIN
    PRINT 'Column override_reason already exists.';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'LeaveRequest' AND COLUMN_NAME = 'overridden_at')
BEGIN
    ALTER TABLE LeaveRequest
    ADD overridden_at DATETIME NULL;
    PRINT 'Column overridden_at added to LeaveRequest table.';
END
ELSE
BEGIN
    PRINT 'Column overridden_at already exists.';
END
GO

PRINT 'Migration completed successfully.';

