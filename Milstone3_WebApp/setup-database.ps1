# PowerShell script to set up the HR_Payroll_System database
# This script will execute the table.sql and procedures.sql files

Write-Host "Setting up HR_Payroll_System database..." -ForegroundColor Green

$server = "(localdb)\MSSQLLocalDB"
$database = "HR_Payroll_System"
$tableScript = "table.sql"
$proceduresScript = "procedures.sql"

# Check if SQL files exist
if (-not (Test-Path $tableScript)) {
    Write-Host "Error: $tableScript not found!" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $proceduresScript)) {
    Write-Host "Error: $proceduresScript not found!" -ForegroundColor Red
    exit 1
}

# Execute table.sql
Write-Host "Executing table.sql..." -ForegroundColor Yellow
$tableResult = sqlcmd -S $server -i $tableScript -b
if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: Some errors occurred while executing table.sql" -ForegroundColor Yellow
    Write-Host $tableResult
} else {
    Write-Host "Tables created successfully!" -ForegroundColor Green
}

# Execute procedures.sql
Write-Host "Executing procedures.sql..." -ForegroundColor Yellow
$proceduresResult = sqlcmd -S $server -d $database -i $proceduresScript -b
if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: Some errors occurred while executing procedures.sql" -ForegroundColor Yellow
    Write-Host $proceduresResult
} else {
    Write-Host "Stored procedures created successfully!" -ForegroundColor Green
}

# Verify setup
Write-Host "`nVerifying database setup..." -ForegroundColor Cyan
$tableCount = sqlcmd -S $server -d $database -Q "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1 -W
$procedureCount = sqlcmd -S $server -d $database -Q "SELECT COUNT(*) as ProcedureCount FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'" -h -1 -W

Write-Host "Tables in database: $tableCount" -ForegroundColor Cyan
Write-Host "Stored procedures in database: $procedureCount" -ForegroundColor Cyan

Write-Host "`nDatabase setup complete!" -ForegroundColor Green

