using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================================
        // ADMIN - VIEW ALL TASKS
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.EmployeeTasks
                .Include(t => t.Employee)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(tasks);
        }


        // =========================================================
        // ADMIN - CREATE TASK
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View();
        }


        // =========================================================
        // ADMIN - CREATE TASK POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(EmployeeTask task)
        {
            if (ModelState.IsValid)
            {
                var employeeExists = await _context.Employees
                    .AnyAsync(e => e.Id == task.EmployeeId);

                if (!employeeExists)
                {
                    ModelState.AddModelError(
                        "EmployeeId",
                        "Please select a valid employee.");
                }
                else
                {
                    task.Status = "Pending";
                    task.CreatedDate = DateTime.Now;

                    _context.EmployeeTasks.Add(task);

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Task created successfully.";

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View(task);
        }


        // =========================================================
        // ADMIN - EDIT TASK
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var task = await _context.EmployeeTasks
                .FindAsync(id);

            if (task == null)
                return NotFound();

            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View(task);
        }


        // =========================================================
        // ADMIN - EDIT TASK POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            EmployeeTask task)
        {
            if (id != task.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var employeeExists = await _context.Employees
                    .AnyAsync(e => e.Id == task.EmployeeId);

                if (!employeeExists)
                {
                    ModelState.AddModelError(
                        "EmployeeId",
                        "Please select a valid employee.");
                }
                else
                {
                    var existingTask = await _context.EmployeeTasks
                        .FindAsync(id);

                    if (existingTask == null)
                        return NotFound();

                    existingTask.Title = task.Title;
                    existingTask.Description = task.Description;
                    existingTask.DueDate = task.DueDate;
                    existingTask.EmployeeId = task.EmployeeId;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Task updated successfully.";

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View(task);
        }


        // =========================================================
        // ADMIN - DELETE TASK
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var task = await _context.EmployeeTasks
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }


        // =========================================================
        // ADMIN - DELETE TASK POST
        // =========================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.EmployeeTasks
                .FindAsync(id);

            if (task != null)
            {
                _context.EmployeeTasks.Remove(task);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Task deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // USER - COMPLETE THEIR OWN TASK
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            // Get currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.EmployeeId == null)
            {
                return Forbid();
            }

            // Find task belonging ONLY to this user's employee
            var task = await _context.EmployeeTasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.EmployeeId == user.EmployeeId);

            // User cannot complete another employee's task
            if (task == null)
            {
                return Forbid();
            }

            // Mark task as completed
            task.Status = "Completed";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Task marked as completed.";

            return RedirectToAction("Index", "Home");
        }
    }
}