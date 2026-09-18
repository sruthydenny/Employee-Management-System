using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PerformancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerformancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Performances
        public async Task<IActionResult> Index()
        {
            var performances = await _context.EmployeePerformances
                .Include(p => p.Employee)
                .OrderByDescending(p => p.Id)
                .ThenBy(p => p.Employee!.FullName)
                .ToListAsync();

            return View(performances);
        }

        // GET: Performances/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var performance = new EmployeePerformance
            {
                Score = 0,
                Month = DateTime.Now.ToString("MMMM yyyy")
            };

            return View(performance);
        }

        // POST: Performances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EmployeePerformance performance)
        {
            var employeeExists = await _context.Employees
                .AnyAsync(e => e.Id == performance.EmployeeId);

            if (!employeeExists)
            {
                ModelState.AddModelError(
                    "EmployeeId",
                    "Please select a valid employee.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await _context.Employees
                    .OrderBy(e => e.FullName)
                    .ToListAsync();

                return View(performance);
            }

            _context.EmployeePerformances.Add(performance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Performance record created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Performances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.EmployeePerformances
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (performance == null)
            {
                return NotFound();
            }

            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View(performance);
        }

        // POST: Performances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EmployeePerformance performance)
        {
            if (id != performance.Id)
            {
                return NotFound();
            }

            var employeeExists = await _context.Employees
                .AnyAsync(e => e.Id == performance.EmployeeId);

            if (!employeeExists)
            {
                ModelState.AddModelError(
                    "EmployeeId",
                    "Please select a valid employee.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await _context.Employees
                    .OrderBy(e => e.FullName)
                    .ToListAsync();

                return View(performance);
            }

            try
            {
                _context.Update(performance);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Performance record updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PerformanceExists(performance.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Performances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.EmployeePerformances
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (performance == null)
            {
                return NotFound();
            }

            return View(performance);
        }

        // POST: Performances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var performance = await _context.EmployeePerformances
                .FindAsync(id);

            if (performance != null)
            {
                _context.EmployeePerformances.Remove(performance);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Performance record deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PerformanceExists(int id)
        {
            return _context.EmployeePerformances
                .Any(p => p.Id == id);
        }
    }
}