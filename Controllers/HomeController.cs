using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================================
        // DASHBOARD
        // =========================================================

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // =====================================================
            // ADMIN DASHBOARD
            // =====================================================

            if (User.IsInRole("Admin"))
            {
                var totalEmployees = await _context.Employees
                    .CountAsync();

                var totalDepartments = await _context.Employees
                    .Select(e => e.Department)
                    .Distinct()
                    .CountAsync();

                var averageSalary = await _context.Employees.AnyAsync()
                    ? await _context.Employees
                        .AverageAsync(e => e.Salary)
                    : 0;


                // -------------------------------------------------
                // ADMIN DASHBOARD STATISTICS
                // -------------------------------------------------

                ViewBag.TotalNews = await _context.News
                    .CountAsync();

                ViewBag.UpcomingEvents = await _context.CompanyEvents
                    .CountAsync(e => e.EventDate >= DateTime.Now);

                ViewBag.TotalTasks = await _context.EmployeeTasks
                    .CountAsync();

                ViewBag.PendingTasks = await _context.EmployeeTasks
                    .CountAsync(t => t.Status != "Completed");

                ViewBag.CompletedTasks = await _context.EmployeeTasks
                    .CountAsync(t => t.Status == "Completed");

                ViewBag.TotalPerformanceRecords =
                    await _context.EmployeePerformances
                        .CountAsync();


                // -------------------------------------------------
                // TODAY'S ATTENDANCE
                // -------------------------------------------------

                var today = DateTime.Today;

                var todayAttendance = await _context.Attendances
                    .Where(a => a.Date.Date == today)
                    .ToListAsync();

                ViewBag.TodayAttendance =
                    todayAttendance.Count;

                ViewBag.TodayPresent =
                    todayAttendance.Count(a => a.IsPresent);


                double todayAttendancePercentage = 0;

                if (todayAttendance.Any())
                {
                    todayAttendancePercentage =
                        (double)todayAttendance.Count(a => a.IsPresent)
                        / todayAttendance.Count
                        * 100;
                }

                ViewBag.TodayAttendancePercentage =
                    todayAttendancePercentage;


                // -------------------------------------------------
                // CREATE ADMIN DASHBOARD
                // -------------------------------------------------

                var dashboard = new DashboardViewModel
                {
                    TotalEmployees = totalEmployees,

                    TotalDepartments = totalDepartments,

                    AverageSalary = averageSalary
                };


                return View(dashboard);
            }


            // =====================================================
            // USER DASHBOARD
            // =====================================================

            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.EmployeeId == null)
            {
                return View(
                    "UserDashboard",
                    new UserDashboardViewModel());
            }


            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);

            if (employee == null)
            {
                return View(
                    "UserDashboard",
                    new UserDashboardViewModel());
            }


            // =====================================================
            // USER TASKS
            // =====================================================

            var tasks = await _context.EmployeeTasks
                .Where(t => t.EmployeeId == employee.Id)
                .OrderBy(t => t.Status == "Completed")
                .ThenBy(t => t.DueDate)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Status = t.Status
                })
                .ToListAsync();


            // =====================================================
            // COMPANY NEWS
            // =====================================================

            var news = await _context.News
                .OrderByDescending(n => n.PublishedDate)
                .Take(5)
                .Select(n => new NewsItemViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    PublishedDate = n.PublishedDate,
                    IsImportant = n.IsImportant
                })
                .ToListAsync();


            // =====================================================
            // UPCOMING EVENTS
            // =====================================================

            var upcomingEvents = await _context.CompanyEvents
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .Select(e => new UpcomingEventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location
                })
                .ToListAsync();


            // =====================================================
            // PERFORMANCE
            // =====================================================

            var performanceHistory =
                await _context.EmployeePerformances
                    .Where(p => p.EmployeeId == employee.Id)
                    .OrderByDescending(p => p.Id)
                    .Take(6)
                    .Select(p => new PerformanceViewModel
                    {
                        Id = p.Id,
                        Month = p.Month,
                        Score = p.Score,
                        Comments = p.Comments
                    })
                    .ToListAsync();


            // =====================================================
            // ATTENDANCE
            // =====================================================

            var attendanceRecords =
                await _context.Attendances
                    .Where(a => a.EmployeeId == employee.Id)
                    .ToListAsync();

            double attendancePercentage = 0;

            if (attendanceRecords.Any())
            {
                var presentDays =
                    attendanceRecords.Count(a => a.IsPresent);

                attendancePercentage =
                    (double)presentDays
                    / attendanceRecords.Count
                    * 100;
            }


            // =====================================================
            // PERFORMANCE AVERAGE
            // =====================================================

            double performancePercentage = 0;

            if (performanceHistory.Any())
            {
                performancePercentage =
                    performanceHistory.Average(p => p.Score);
            }


            // =====================================================
            // USER DASHBOARD
            // =====================================================

            var userDashboard = new UserDashboardViewModel
            {
                EmployeeId = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                Salary = employee.Salary,
                HireDate = employee.HireDate,

                Tasks = tasks,

                PendingTasks = tasks.Count(
                    t => t.Status != "Completed"),

                CompletedTasks = tasks.Count(
                    t => t.Status == "Completed"),

                News = news,

                UpcomingEvents = upcomingEvents,

                PerformanceHistory = performanceHistory,

                PerformancePercentage =
                    performancePercentage,

                AttendancePercentage =
                    attendancePercentage
            };


            return View(
                "UserDashboard",
                userDashboard);
        }


        // =========================================================
        // PRIVACY
        // =========================================================

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }


        // =========================================================
        // ERROR
        // =========================================================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }
    }
}