using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;

namespace Milstone3_WebApp.Controllers
{
    [Authorize(Roles = "SystemAdmin,HRAdmin,PayrollFinance")]
    public class PayrollsController : Controller
    {
        private readonly HrPayrollSystemContext _context;

        public PayrollsController(HrPayrollSystemContext context)
        {
            _context = context;
        }

        // -------------------- LIST PAYROLLS --------------------
        public async Task<IActionResult> Index()
        {
            var payrolls = await _context.Payrolls
                .FromSqlRaw("SELECT * FROM Payroll")
                .ToListAsync();
            return View(payrolls);
        }

        // -------------------- PAYROLL DETAILS --------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payroll = await _context.Payrolls
                .FirstOrDefaultAsync(m => m.PayrollId == id);

            if (payroll == null) return NotFound();
            return View(payroll);
        }

        // -------------------- GENERATE PAYROLL --------------------
        // Uses procedure: GeneratePayroll
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DateTime startDate, DateTime endDate)
        {
            if (ModelState.IsValid)
            {
                var payrolls = await _context.Payrolls
                    .FromSqlRaw("EXEC GeneratePayroll @StartDate={0}, @EndDate={1}", startDate, endDate)
                    .ToListAsync();

                // Show generated payrolls in Index
                return View("Index", payrolls);
            }
            return View();
        }

        // -------------------- EDIT PAYROLL --------------------
        // Example: AdjustPayrollItem or UpdateSalaryType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int payrollId, string type, decimal amount)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC AdjustPayrollItem @PayrollID={0}, @Type={1}, @Amount={2}",
                payrollId, type, amount
            );
            return RedirectToAction(nameof(Index));
        }

        // -------------------- DELETE PAYROLL --------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payroll = await _context.Payrolls
                .FirstOrDefaultAsync(m => m.PayrollId == id);

            if (payroll == null) return NotFound();
            return View(payroll);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // No explicit procedure for delete, fallback simple delete
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // -------------------- EMPLOYEE PAYROLL HISTORY --------------------
        // Uses procedure: GetEmployeePayrollHistory
        public async Task<IActionResult> History(int employeeId)
        {
            var history = await _context.Payrolls
                .FromSqlRaw("EXEC GetEmployeePayrollHistory @EmployeeID={0}", employeeId)
                .ToListAsync();
            return View(history);
        }

        // -------------------- MONTHLY SUMMARY --------------------
        // Uses procedure: GetMonthlyPayrollSummary
        public async Task<IActionResult> MonthlySummary(int month, int year)
        {
            var summary = await _context.Payrolls
                .FromSqlRaw("EXEC GetMonthlyPayrollSummary @Month={0}, @Year={1}", month, year)
                .ToListAsync();
            return View(summary);
        }

        private bool PayrollExists(int id)
        {
            return _context.Payrolls.Any(e => e.PayrollId == id);
        }
    }
}
