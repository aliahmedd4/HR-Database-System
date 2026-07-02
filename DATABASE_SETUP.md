# Database Setup and Connection Guide

## Database Status

✅ **Database exists**: `HR_Payroll_System`  
✅ **Tables**: 56 tables created  
✅ **Stored Procedures**: 62 procedures created  
✅ **Connection String**: Configured in `appsettings.Development.json`

## Connection Configuration

The application is configured to connect to SQL Server LocalDB:

**Connection String:**
```
Server=(localdb)\MSSQLLocalDB;Database=HR_Payroll_System;Trusted_Connection=True;
```

**Location:**
- `appsettings.Development.json` - Development connection string
- `Program.cs` - Database context configuration via dependency injection
- `HrPayrollSystemContext.cs` - Fallback connection (only if not configured externally)

## Database Components

### Critical Tables
All required tables are present:
- ✅ Employee
- ✅ Attendance
- ✅ ShiftSchedule
- ✅ LeaveRequest
- ✅ Payroll
- ✅ Contract
- ✅ Department
- ✅ Role
- And 48 more tables...

### Stored Procedures Used by Application

The application uses stored procedures in the following controllers:

#### EmployeesController
- `ViewEmployeeInfo` - View employee details
- `AddEmployee` - Create new employee
- `UpdateEmployeeInfo` - Update employee information
- `UpdateEmergencyContact` - Update emergency contact
- `RenewContract` - Renew employee contract
- `GetExpiringContracts` - Get contracts expiring soon

#### MissionsController
- `AssignMission` - Assign mission to employee
- `ApproveMissionCompletion` - Approve mission completion
- `ViewMyMissions` - View employee's missions

#### LeavesController
- `SubmitLeaveRequest` - Submit leave request

#### PayrollsController
- `GeneratePayroll` - Generate payroll for period
- `AdjustPayrollItem` - Adjust payroll item
- `GetEmployeePayrollHistory` - Get employee payroll history

#### ContractsController
- `GetActiveContracts` - Get active contracts
- `CreateContract` - Create new contract

## Verification

To verify database connection and components, run:

```powershell
powershell -ExecutionPolicy Bypass -File verify-database-connection.ps1
```

## Application Components Connected to Database

### Controllers Using Database
1. ✅ **AccountController** - User authentication and registration
2. ✅ **EmployeesController** - Employee management (uses stored procedures)
3. ✅ **AttendanceController** - Attendance tracking (direct EF Core)
4. ✅ **ShiftsController** - Shift management (direct EF Core)
5. ✅ **LeavesController** - Leave management (uses stored procedures)
6. ✅ **MissionsController** - Mission management (uses stored procedures)
7. ✅ **PayrollsController** - Payroll processing (uses stored procedures)
8. ✅ **ContractsController** - Contract management (uses stored procedures)
9. ✅ **NotificationsController** - Notifications (direct EF Core)
10. ✅ **AnalyticsController** - Analytics and reports (direct EF Core)
11. ✅ **HierarchyController** - Organizational hierarchy (direct EF Core)
12. ✅ **HomeController** - Dashboard (direct EF Core)

### Data Access Methods

The application uses two methods to access the database:

1. **Entity Framework Core (Direct)**
   - Used by: Attendance, Shifts, Notifications, Analytics, Hierarchy
   - Example: `_context.Attendances.ToListAsync()`

2. **Stored Procedures**
   - Used by: Employees, Missions, Leaves, Payrolls, Contracts
   - Example: `_context.Database.ExecuteSqlRawAsync("EXEC AddEmployee ...")`

## Testing Database Connection

### Method 1: Run the Application
```bash
dotnet run
```
The application will automatically connect to the database on startup.

### Method 2: Test Connection Manually
```sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "HR_Payroll_System" -Q "SELECT @@VERSION"
```

### Method 3: Use Verification Script
```powershell
powershell -ExecutionPolicy Bypass -File verify-database-connection.ps1
```

## Troubleshooting

### Issue: "Cannot open database"
**Solution:**
1. Ensure SQL Server LocalDB is running
2. Verify database exists: `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases"`
3. If database doesn't exist, run `table.sql` first, then `procedures.sql`

### Issue: "Stored procedure not found"
**Solution:**
1. Run `procedures.sql` to create all stored procedures
2. Verify procedure exists: `sqlcmd -S "(localdb)\MSSQLLocalDB" -d "HR_Payroll_System" -Q "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'ProcedureName'"`

### Issue: "Table not found"
**Solution:**
1. Run `table.sql` to create all tables
2. Verify table exists: `sqlcmd -S "(localdb)\MSSQLLocalDB" -d "HR_Payroll_System" -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TableName'"`

## Next Steps

1. ✅ Database is set up and connected
2. ✅ All components are configured
3. ✅ Application is ready to run

Run the application:
```bash
dotnet run
```

Then access: `http://localhost:5222` or `https://localhost:7200`

