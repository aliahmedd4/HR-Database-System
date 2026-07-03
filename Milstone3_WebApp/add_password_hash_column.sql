-- Run this script once in SQL Server before restarting the app.
-- Adds per-user password hash column to the Employee table.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Employee') AND name = 'password_hash'
)
BEGIN
    ALTER TABLE Employee ADD password_hash NVARCHAR(MAX) NULL;
    PRINT 'Column password_hash added to Employee table.';
END
ELSE
BEGIN
    PRINT 'Column password_hash already exists — skipping.';
END
