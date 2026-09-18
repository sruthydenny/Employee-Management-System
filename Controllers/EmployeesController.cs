using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employees
        public async Task<IActionResult> Index(
            string searchString,
            string department)
        {
            // ADMIN
            // Admins can see all employees and use the filters.
            if (User.IsInRole("Admin"))
            {
                var employees = _context.Employees.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    employees = employees.Where(e =>
                        e.FullName.Contains(searchString));
                }

                if (!string.IsNullOrWhiteSpace(department))
                {
                    employees = employees.Where(e =>
                        e.Department == department);
                }

                ViewData["CurrentFilter"] = searchString;
                ViewData["CurrentDepartment"] = department;

                ViewBag.Departments = await _context.Employees
                    .Select(e => e.Department)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();

                return View(await employees.ToListAsync());
            }

            // USER
            // Normal users go directly to their own employee profile.
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.EmployeeId == null)
            {
                return View("Details", null);
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);

            if (employee == null)
            {
                return View("Details", null);
            }

            return View("Details", employee);
        }


        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Admins can view any employee.
            if (User.IsInRole("Admin"))
            {
                return View(employee);
            }

            // Normal users can only view their own employee record.
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.EmployeeId != employee.Id)
            {
                return Forbid();
            }

            return View(employee);
        }


        // GET: Employees/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }


        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("Id,FullName,Email,Department,Position,Salary,HireDate")]
            Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }


        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int? id,
            [Bind("Id,FullName,Email,Department,Position,Salary,HireDate")]
            Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }


        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // POST: Employees/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool EmployeeExists(int? id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}