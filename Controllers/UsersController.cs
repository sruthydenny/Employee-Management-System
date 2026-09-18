using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        // =========================================================
        // GET: Users
        // =========================================================
        public async Task<IActionResult> Index()
        {
            // Get all registered users and their assigned employee
            var users = await _userManager.Users
                .Include(u => u.Employee)
                .ToListAsync();

            // Get all employees for the assignment dropdown
            ViewBag.Employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "User",
                    EmployeeId = user.EmployeeId,
                    EmployeeName = user.Employee?.FullName
                });
            }

            return View(userList);
        }


        // =========================================================
        // GET: Users/AssignEmployee
        // =========================================================
        public async Task<IActionResult> AssignEmployee(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            ViewBag.UserEmail = user.Email;
            ViewBag.UserId = user.Id;
            ViewBag.CurrentEmployeeId = user.EmployeeId;

            return View(employees);
        }


        // =========================================================
        // POST: Users/AssignEmployee
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignEmployee(
            string id,
            int? employeeId)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();


            // Remove employee assignment
            if (employeeId == null)
            {
                user.EmployeeId = null;
            }
            else
            {
                // Check that the employee actually exists
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                if (employee == null)
                {
                    TempData["Error"] =
                        "Selected employee does not exist.";

                    return RedirectToAction(nameof(Index));
                }

                user.EmployeeId = employee.Id;
            }


            // Save user
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] =
                    "Unable to update the employee assignment.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Success"] =
                "Employee assignment updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // POST: Users/MakeAdmin
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var result = await _userManager.AddToRoleAsync(
                    user,
                    "Admin");

                if (!result.Succeeded)
                {
                    TempData["Error"] =
                        "Unable to make this user an Admin.";

                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Success"] =
                "User has been made an Admin.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // POST: Users/RemoveAdmin
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();


            // Prevent admin from removing their own Admin role
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] =
                    "You cannot remove your own Admin role.";

                return RedirectToAction(nameof(Index));
            }


            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var result = await _userManager.RemoveFromRoleAsync(
                    user,
                    "Admin");

                if (!result.Succeeded)
                {
                    TempData["Error"] =
                        "Unable to remove the Admin role.";

                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Success"] =
                "Admin role removed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }


    // =============================================================
    // User View Model
    // =============================================================
    public class UserViewModel
    {
        public string Id { get; set; } = "";

        public string Email { get; set; } = "";

        public string Role { get; set; } = "User";

        public int? EmployeeId { get; set; }

        public string? EmployeeName { get; set; }
    }
}