using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Services
{
    public class WorkSphereAiService
    {
        private readonly ApplicationDbContext _context;

        public WorkSphereAiService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // MY TASKS
        // =========================================================

        public async Task<string> GetMyTasksAsync(int employeeId)
        {
            var tasks = await _context.EmployeeTasks
                .AsNoTracking()
                .Where(t => t.EmployeeId == employeeId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            if (!tasks.Any())
            {
                return "The employee currently has no tasks assigned.";
            }

            var result = new System.Text.StringBuilder();

            result.AppendLine("Employee's tasks:");

            foreach (var task in tasks)
            {
                result.AppendLine(
                    $"- {task.Title} | Status: {task.Status}" +
                    (task.DueDate.HasValue
                        ? $" | Due: {task.DueDate.Value:dd MMM yyyy}"
                        : ""));

                if (!string.IsNullOrWhiteSpace(task.Description))
                {
                    result.AppendLine($"  Description: {task.Description}");
                }
            }

            return result.ToString();
        }


        // =========================================================
        // MY ATTENDANCE
        // =========================================================

        public async Task<string> GetMyAttendanceAsync(int employeeId)
        {
            var attendance = await _context.Attendances
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            if (!attendance.Any())
            {
                return "There are no attendance records available for this employee.";
            }

            int total = attendance.Count;
            int present = attendance.Count(a => a.IsPresent);
            int late = attendance.Count(a => a.IsLate);
            int absent = attendance.Count(a => !a.IsPresent);

            double percentage = total > 0
                ? (double)present / total * 100
                : 0;

            var result = new System.Text.StringBuilder();

            result.AppendLine("Employee attendance summary:");
            result.AppendLine($"- Total attendance records: {total}");
            result.AppendLine($"- Present: {present}");
            result.AppendLine($"- Late: {late}");
            result.AppendLine($"- Absent: {absent}");
            result.AppendLine($"- Attendance percentage: {percentage:0.##}%");

            result.AppendLine();
            result.AppendLine("Recent attendance:");

            foreach (var record in attendance.Take(10))
            {
                string status;

                if (!record.IsPresent)
                {
                    status = "Absent";
                }
                else if (record.IsLate)
                {
                    status = "Late";
                }
                else
                {
                    status = "Present";
                }

                result.AppendLine(
                    $"- {record.Date:dd MMM yyyy}: {status}");
            }

            return result.ToString();
        }


        // =========================================================
        // MY PERFORMANCE
        // =========================================================

        public async Task<string> GetMyPerformanceAsync(int employeeId)
        {
            var performance = await _context.EmployeePerformances
                .AsNoTracking()
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            if (!performance.Any())
            {
                return "There are no performance records available for this employee.";
            }

            double averageScore = performance.Average(p => p.Score);

            var result = new System.Text.StringBuilder();

            result.AppendLine("Employee performance summary:");
            result.AppendLine($"- Average performance score: {averageScore:0.##}%");
            result.AppendLine();
            result.AppendLine("Performance records:");

            foreach (var record in performance)
            {
                result.AppendLine(
                    $"- {record.Month}: {record.Score:0.##}%");

                if (!string.IsNullOrWhiteSpace(record.Comments))
                {
                    result.AppendLine(
                        $"  Comments: {record.Comments}");
                }
            }

            return result.ToString();
        }


        // =========================================================
        // UPCOMING COMPANY EVENTS
        // =========================================================

        public async Task<string> GetUpcomingEventsAsync()
        {
            var now = DateTime.Now;

            var events = await _context.CompanyEvents
                .AsNoTracking()
                .Where(e => e.EventDate >= now)
                .OrderBy(e => e.EventDate)
                .Take(10)
                .ToListAsync();

            if (!events.Any())
            {
                return "There are currently no upcoming company events.";
            }

            var result = new System.Text.StringBuilder();

            result.AppendLine("Upcoming company events:");

            foreach (var companyEvent in events)
            {
                result.AppendLine(
                    $"- {companyEvent.Title} | " +
                    $"{companyEvent.EventDate:dd MMM yyyy HH:mm}");

                if (!string.IsNullOrWhiteSpace(companyEvent.Location))
                {
                    result.AppendLine(
                        $"  Location: {companyEvent.Location}");
                }

                if (!string.IsNullOrWhiteSpace(companyEvent.Description))
                {
                    result.AppendLine(
                        $"  Description: {companyEvent.Description}");
                }
            }

            return result.ToString();
        }


        // =========================================================
        // LATEST COMPANY NEWS
        // =========================================================

        public async Task<string> GetLatestNewsAsync()
        {
            var news = await _context.News
                .AsNoTracking()
                .OrderByDescending(n => n.PublishedDate)
                .Take(10)
                .ToListAsync();

            if (!news.Any())
            {
                return "There is currently no company news available.";
            }

            var result = new System.Text.StringBuilder();

            result.AppendLine("Latest company news:");

            foreach (var item in news)
            {
                result.AppendLine(
                    $"- {item.Title} | " +
                    $"{item.PublishedDate:dd MMM yyyy}");

                if (item.IsImportant)
                {
                    result.AppendLine("  Important announcement.");
                }

                result.AppendLine(
                    $"  {item.Content}");
            }

            return result.ToString();
        }


        // =========================================================
        // ADMIN: EMPLOYEE COUNT
        // =========================================================

        public async Task<string> GetEmployeeCountAsync()
        {
            var count = await _context.Employees
                .AsNoTracking()
                .CountAsync();

            return $"There are currently {count} employees in WorkSphere.";
        }


        // =========================================================
        // ADMIN: TASK SUMMARY
        // =========================================================

        public async Task<string> GetTaskSummaryAsync()
        {
            var tasks = await _context.EmployeeTasks
                .AsNoTracking()
                .ToListAsync();

            if (!tasks.Any())
            {
                return "There are currently no tasks in WorkSphere.";
            }

            int total = tasks.Count;

            int completed = tasks.Count(t =>
                t.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase));

            int pending = tasks.Count(t =>
                !t.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase));

            var result = new System.Text.StringBuilder();

            result.AppendLine("WorkSphere task summary:");
            result.AppendLine($"- Total tasks: {total}");
            result.AppendLine($"- Completed: {completed}");
            result.AppendLine($"- Not completed: {pending}");

            return result.ToString();
        }
    }
}