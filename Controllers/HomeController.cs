using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var dashboard = new DashboardViewModel
            {
                TotalEmployees = _context.Employees.Count(),

                TotalDepartments = _context.Employees
                    .Select(e => e.Department)
                    .Distinct()
                    .Count(),

                AverageSalary = _context.Employees.Any()
                    ? _context.Employees.Average(e => e.Salary)
                    : 0
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}