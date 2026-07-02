# How to Run Milstone3 HRMS Web Application

## Prerequisites

Before running the project, ensure you have the following installed:

1. **.NET SDK 10.0** (or compatible version)
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

2. **SQL Server LocalDB** (or SQL Server Express)
   - Usually comes with Visual Studio
   - Or download SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads
   - Verify installation: SQL Server LocalDB should be available

3. **Visual Studio 2022** (Recommended) or **Visual Studio Code**
   - Visual Studio: https://visualstudio.microsoft.com/
   - VS Code: https://code.visualstudio.com/

## Step-by-Step Instructions

### Method 1: Using Visual Studio (Recommended)

1. **Open the Solution**
   - Open Visual Studio 2022
   - File → Open → Project/Solution
   - Navigate to: `Milstone3_WebApp/Milstone3_WebApp.slnx`
   - Click Open

2. **Restore NuGet Packages**
   - Right-click on the solution in Solution Explorer
   - Select "Restore NuGet Packages"
   - Wait for packages to restore

3. **Set Up Database**
   - The application uses SQL Server LocalDB
   - Connection string: `Server=(localdb)\\MSSQLLocalDB;Database=HR_Payroll_System;Trusted_Connection=True;`
   - If database doesn't exist, Entity Framework will create it on first run (if migrations are configured)
   - OR manually create the database using SQL Server Management Studio

4. **Run the Project**
   - Press `F5` or click the "Run" button (green play icon)
   - Or go to: Debug → Start Debugging
   - The browser will automatically open to: `https://localhost:7200` or `http://localhost:5222`

### Method 2: Using Command Line (Terminal/PowerShell)

1. **Open Terminal/PowerShell**
   - Navigate to the project directory:
     ```bash
     cd "Milstone3_WebApp/Milstone3_WebApp"
     ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the Project**
   ```bash
   dotnet build
   ```

4. **Run the Project**
   ```bash
   dotnet run
   ```
   
   Or specify the profile:
   ```bash
   dotnet run --launch-profile https
   ```

5. **Open Browser**
   - Navigate to: `https://localhost:7200` or `http://localhost:5222`
   - If you see a certificate warning, click "Advanced" → "Proceed to localhost"

### Method 3: Using Visual Studio Code

1. **Open the Project**
   - File → Open Folder
   - Select: `Milstone3_WebApp`

2. **Install C# Extension** (if not already installed)
   - Extensions → Search "C#" → Install

3. **Open Terminal in VS Code**
   - Terminal → New Terminal (Ctrl+`)

4. **Run Commands**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

## Database Setup

### Option 1: Let EF Create Database (If Migrations Exist)

If you have Entity Framework migrations:
```bash
dotnet ef database update
```

### Option 2: Manual Database Creation

1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**
2. Connect to: `(localdb)\MSSQLLocalDB`
3. Create a new database named: `HR_Payroll_System`
4. The application will create tables on first run

### Option 3: Update Connection String

If you want to use a different database:
1. Edit `appsettings.Development.json`
2. Update the `DefaultConnection` string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=HR_Payroll_System;Trusted_Connection=True;"
   }
   ```

## First-Time Login

After running the application:

1. **Register a New Account**
   - Go to: `/Account/Register`
   - Fill in the registration form
   - Select your role (Employee, LineManager, HRAdmin, etc.)

2. **Login**
   - Go to: `/Account/Login`
   - Use the email you registered
   - Use the role-specific password:
     - Employee: `employee123`
     - Line Manager: `manager123`
     - HR Admin: `admin123`
     - Executive: `executive123`
     - Payroll/Finance: `payroll123`
     - Recruiter: `recruiter123`

## Troubleshooting

### Issue: "Cannot connect to database"
**Solution:**
- Ensure SQL Server LocalDB is running
- Check connection string in `appsettings.Development.json`
- Verify database exists or create it manually

### Issue: "Port already in use"
**Solution:**
- Change the port in `Properties/launchSettings.json`
- Or stop the process using the port:
  ```bash
  # Windows
  netstat -ano | findstr :5222
  taskkill /PID <PID> /F
  ```

### Issue: "Certificate error" (HTTPS)
**Solution:**
- Click "Advanced" → "Proceed to localhost" in browser
- Or run with HTTP profile: `dotnet run --launch-profile http`

### Issue: "Package restore failed"
**Solution:**
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Issue: ".NET SDK not found"
**Solution:**
- Install .NET SDK from: https://dotnet.microsoft.com/download
- Verify: `dotnet --version`
- Restart your terminal/IDE

## Project Structure

```
Milstone3_WebApp/
├── Controllers/          # MVC Controllers
│   ├── AttendanceController.cs
│   ├── ShiftsController.cs
│   └── ...
├── Views/               # Razor Views
│   ├── Attendance/
│   ├── Shifts/
│   └── ...
├── Models/              # Data Models
├── wwwroot/            # Static files (CSS, JS, images)
├── Program.cs          # Application entry point
├── appsettings.json    # Configuration
└── HrPayrollSystemContext.cs  # EF Core DbContext
```

## Development URLs

- **HTTPS:** https://localhost:7200
- **HTTP:** http://localhost:5222
- **Login:** http://localhost:5222/Account/Login
- **Dashboard:** http://localhost:5222/Home

## Next Steps After Running

1. **Create Test Data:**
   - Register users with different roles
   - Create departments and positions
   - Create shift types
   - Assign shifts to employees

2. **Test Features:**
   - Record attendance (check-in/check-out)
   - Create and assign shifts
   - Submit leave requests
   - Test offline attendance sync

## Need Help?

- Check the application logs in the console/terminal
- Review error messages in the browser
- Verify database connection
- Ensure all NuGet packages are restored

