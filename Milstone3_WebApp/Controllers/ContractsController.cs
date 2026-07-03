using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Milstone3_WebApp;
using Milstone3_WebApp.Services;

namespace Milstone3_WebApp.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly HrPayrollSystemContext _context;
        private readonly NotificationService _notificationService;

        public ContractsController(HrPayrollSystemContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // -------------------- LIST ACTIVE AND EXPIRING CONTRACTS --------------------
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var expirationThreshold = today.AddDays(30); // Contracts expiring within 30 days

            // Get all active contracts (not expired)
            var allContracts = await _context.Contracts
                .Where(c => c.EndDate == null || c.EndDate >= today)
                .OrderBy(c => c.EndDate)
                .ToListAsync();

            // Separate active and expiring contracts
            var activeContracts = allContracts
                .Where(c => c.EndDate == null || c.EndDate > expirationThreshold)
                .ToList();

            var expiringContracts = allContracts
                .Where(c => c.EndDate.HasValue && 
                           c.EndDate.Value >= today && 
                           c.EndDate.Value <= expirationThreshold)
                .ToList();

            ViewData["ActiveContracts"] = activeContracts;
            ViewData["ExpiringContracts"] = expiringContracts;
            ViewData["Today"] = today;
            ViewData["ExpirationThreshold"] = expirationThreshold;

            return View(allContracts);
        }

        // -------------------- CONTRACT DETAILS --------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(m => m.ContractId == id);

            if (contract == null) return NotFound();
            return View(contract);
        }

        // -------------------- CREATE CONTRACT (HR Admin Only) --------------------
        [Authorize(Roles = "HRAdmin")]
        [HttpGet]
        public IActionResult Create()
        {
            // Get active employees for dropdown
            var employees = _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .ToList();

            ViewData["EmployeeId"] = new SelectList(employees, "EmployeeId", "FullName");
            ViewData["ContractTypes"] = new SelectList(new[] { "FullTime", "PartTime", "Consultant", "Internship" });

            return View();
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set CreatedAt if not set
                    if (!contract.CreatedAt.HasValue)
                    {
                        contract.CreatedAt = DateTime.Now;
                    }

                    // Set CurrentState if not set
                    if (string.IsNullOrEmpty(contract.CurrentState))
                    {
                        contract.CurrentState = "Active";
                    }

                    await using var contractTx = await _context.Database.BeginTransactionAsync();

                    _context.Contracts.Add(contract);
                    await _context.SaveChangesAsync();

                    // Update employee's contract reference in the same transaction
                    var employee = await _context.Employees.FindAsync(contract.EmployeeId);
                    if (employee != null)
                    {
                        employee.ContractId = contract.ContractId;
                        await _context.SaveChangesAsync();
                    }

                    await contractTx.CommitAsync();

                    // Send notification after commit so it only fires on success
                    await _notificationService.CreateContractCreatedNotification(
                        contract.EmployeeId,
                        contract.ContractType ?? contract.Type ?? "Contract",
                        contract.StartDate,
                        contract.EndDate
                    );

                    TempData["SuccessMessage"] = "Contract created successfully and notification sent to employee.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating contract: {ex.Message}");
                }
            }

            // Reload dropdowns on error
            var employees = _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .ToList();

            ViewData["EmployeeId"] = new SelectList(employees, "EmployeeId", "FullName", contract.EmployeeId);
            ViewData["ContractTypes"] = new SelectList(new[] { "FullTime", "PartTime", "Consultant", "Internship" }, contract.ContractType);

            return View(contract);
        }

        // -------------------- EDIT CONTRACT (HR Admin Only) --------------------
        [Authorize(Roles = "HRAdmin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            // Get active employees for dropdown
            var employees = _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .ToList();

            ViewData["EmployeeId"] = new SelectList(employees, "EmployeeId", "FullName", contract.EmployeeId);
            ViewData["ContractTypes"] = new SelectList(new[] { "FullTime", "PartTime", "Consultant", "Internship" }, contract.ContractType);
            ViewData["ContractStates"] = new SelectList(new[] { "Active", "Expired", "Terminated", "Renewed" }, contract.CurrentState);

            return View(contract);
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.ContractId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original contract to compare changes
                    var originalContract = await _context.Contracts.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.ContractId == id);

                    _context.Update(contract);
                    await _context.SaveChangesAsync();

                    // Send notification to employee about the update
                    if (originalContract != null)
                    {
                        await _notificationService.CreateContractUpdatedNotification(
                            contract.EmployeeId,
                            contract.ContractType ?? contract.Type ?? "Contract",
                            contract.StartDate,
                            contract.EndDate
                        );
                    }

                    TempData["SuccessMessage"] = "Contract updated successfully and notification sent to employee.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.ContractId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Reload dropdowns on error
            var employees = _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FullName)
                .ToList();

            ViewData["EmployeeId"] = new SelectList(employees, "EmployeeId", "FullName", contract.EmployeeId);
            ViewData["ContractTypes"] = new SelectList(new[] { "FullTime", "PartTime", "Consultant", "Internship" }, contract.ContractType);
            ViewData["ContractStates"] = new SelectList(new[] { "Active", "Expired", "Terminated", "Renewed" }, contract.CurrentState);

            return View(contract);
        }

        // -------------------- RENEW CONTRACT (HR Admin Only) --------------------
        [Authorize(Roles = "HRAdmin")]
        [HttpGet]
        public async Task<IActionResult> Renew(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound();

            // Pre-fill renewal form with current contract details
            ViewData["OriginalContract"] = contract;
            ViewData["ContractTypes"] = new SelectList(new[] { "FullTime", "PartTime", "Consultant", "Internship" }, contract.ContractType);

            return View(contract);
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id, Contract renewalContract)
        {
            var originalContract = await _context.Contracts.FindAsync(id);
            if (originalContract == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Store old end date for notification
                    var oldEndDate = originalContract.EndDate;

                    // Update the existing contract with new dates
                    originalContract.StartDate = renewalContract.StartDate;
                    originalContract.EndDate = renewalContract.EndDate;
                    originalContract.CurrentState = "Active";
                    originalContract.ContractType = renewalContract.ContractType ?? originalContract.ContractType;
                    originalContract.Type = renewalContract.Type ?? originalContract.Type;

                    _context.Update(originalContract);
                    await _context.SaveChangesAsync();

                    // Send notification to employee about the renewal
                    await _notificationService.CreateContractRenewedNotification(
                        originalContract.EmployeeId,
                        originalContract.ContractType ?? originalContract.Type ?? "Contract",
                        renewalContract.StartDate,
                        renewalContract.EndDate,
                        oldEndDate
                    );

                    TempData["SuccessMessage"] = "Contract renewed successfully and notification sent to employee.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error renewing contract: {ex.Message}");
                }
            }

            // Reload on error
            ViewData["OriginalContract"] = originalContract;
            ViewData["ContractTypes"] = new SelectList(new[] { "FullTime", "PartTime", "Consultant", "Internship" }, renewalContract.ContractType);

            return View(renewalContract);
        }

        // -------------------- DELETE CONTRACT --------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(m => m.ContractId == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Contract deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractId == id);
        }
    }
}
