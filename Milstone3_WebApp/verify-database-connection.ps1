# PowerShell script to verify database connection and all components
Write-Host "=== HR Payroll System - Database Connection Verification ===" -ForegroundColor Cyan
Write-Host ""

$server = "(localdb)\MSSQLLocalDB"
$database = "HR_Payroll_System"
$connectionString = "Server=$server;Database=$database;Trusted_Connection=True;"

# Test database connection
Write-Host "1. Testing database connection..." -ForegroundColor Yellow
try {
    $testQuery = sqlcmd -S $server -d $database -Q "SELECT @@VERSION" -h -1 -W
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] Database connection successful!" -ForegroundColor Green
    } else {
        Write-Host "   [FAIL] Database connection failed!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   [FAIL] Error connecting to database: $_" -ForegroundColor Red
    exit 1
}

# Count tables
Write-Host "`n2. Checking database tables..." -ForegroundColor Yellow
$tableCount = sqlcmd -S $server -d $database -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1 -W
Write-Host "   [OK] Found $tableCount tables" -ForegroundColor Green

# Count stored procedures
Write-Host "`n3. Checking stored procedures..." -ForegroundColor Yellow
$procCount = sqlcmd -S $server -d $database -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'" -h -1 -W
Write-Host "   [OK] Found $procCount stored procedures" -ForegroundColor Green

# Check critical tables
Write-Host "`n4. Verifying critical tables..." -ForegroundColor Yellow
$criticalTables = @("Employee", "Attendance", "ShiftSchedule", "LeaveRequest", "Payroll", "Contract", "Department", "Role")
$missingTables = @()

foreach ($table in $criticalTables) {
    $exists = sqlcmd -S $server -d $database -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table'" -h -1 -W
    if ([int]$exists -gt 0) {
        Write-Host "   [OK] Table '$table' exists" -ForegroundColor Green
    } else {
        Write-Host "   [MISSING] Table '$table' is missing!" -ForegroundColor Red
        $missingTables += $table
    }
}

# Check critical stored procedures
Write-Host "`n5. Verifying critical stored procedures..." -ForegroundColor Yellow
$criticalProcs = @("ViewEmployeeInfo", "AddEmployee", "UpdateEmployeeInfo", "UpdateEmergencyContact", "RenewContract", "GetExpiringContracts", "AssignMission", "ApproveMissionCompletion", "ViewMyMissions", "SubmitLeaveRequest", "GeneratePayroll", "AdjustPayrollItem", "GetEmployeePayrollHistory", "GetActiveContracts", "CreateContract")
$missingProcs = @()

foreach ($proc in $criticalProcs) {
    $exists = sqlcmd -S $server -d $database -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = '$proc' AND ROUTINE_TYPE = 'PROCEDURE'" -h -1 -W
    if ([int]$exists -gt 0) {
        Write-Host "   [OK] Procedure '$proc' exists" -ForegroundColor Green
    } else {
        Write-Host "   [MISSING] Procedure '$proc' is missing!" -ForegroundColor Red
        $missingProcs += $proc
    }
}

# Summary
Write-Host "`n=== Verification Summary ===" -ForegroundColor Cyan
if ($missingTables.Count -eq 0 -and $missingProcs.Count -eq 0) {
    Write-Host "[SUCCESS] All critical components are present!" -ForegroundColor Green
    Write-Host "[SUCCESS] Database is ready for use!" -ForegroundColor Green
} else {
    Write-Host "[WARNING] Some components are missing:" -ForegroundColor Yellow
    if ($missingTables.Count -gt 0) {
        Write-Host "  Missing tables: $($missingTables -join ', ')" -ForegroundColor Red
    }
    if ($missingProcs.Count -gt 0) {
        Write-Host "  Missing procedures: $($missingProcs -join ', ')" -ForegroundColor Red
    }
    Write-Host "`nPlease run the SQL scripts (table.sql and procedures.sql) to create missing components." -ForegroundColor Yellow
}

Write-Host "`nConnection String: $connectionString" -ForegroundColor Cyan
Write-Host ""
