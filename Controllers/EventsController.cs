using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.CompanyEvents
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            return View(events);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyEvent companyEvent)
        {
            if (!ModelState.IsValid)
            {
                return View(companyEvent);
            }

            _context.CompanyEvents.Add(companyEvent);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company event created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyEvent = await _context.CompanyEvents.FindAsync(id);

            if (companyEvent == null)
            {
                return NotFound();
            }

            return View(companyEvent);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyEvent companyEvent)
        {
            if (id != companyEvent.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(companyEvent);
            }

            try
            {
                _context.Update(companyEvent);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Company event updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyEventExists(companyEvent.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyEvent = await _context.CompanyEvents
                .FirstOrDefaultAsync(e => e.Id == id);

            if (companyEvent == null)
            {
                return NotFound();
            }

            return View(companyEvent);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyEvent = await _context.CompanyEvents.FindAsync(id);

            if (companyEvent != null)
            {
                _context.CompanyEvents.Remove(companyEvent);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Company event deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyEventExists(int id)
        {
            return _context.CompanyEvents.Any(e => e.Id == id);
        }
    }
}