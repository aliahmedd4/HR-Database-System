-- Fix ShiftAssignment table to allow 'Individual' and 'Custom' assignment types
-- This matches the application code requirements

USE HR_Payroll_System;
GO

-- Drop the existing CHECK constraint (find the actual name first if needed)
-- Run: SELECT name FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('ShiftAssignment') AND definition LIKE '%assignment_type%'
ALTER TABLE ShiftAssignment
DROP CONSTRAINT CK__ShiftAssi__assig__29221CFB;
GO

-- Add new CHECK constraint that allows all assignment types used in the application
ALTER TABLE ShiftAssignment
ADD CONSTRAINT CK_ShiftAssignment_AssignmentType 
CHECK (assignment_type IN ('Employee', 'Department', 'Individual', 'Custom'));
GO

-- Verify the constraint
SELECT 
    name AS ConstraintName,
    definition AS CheckDefinition
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('ShiftAssignment') 
  AND name LIKE '%AssignmentType%';
GO

