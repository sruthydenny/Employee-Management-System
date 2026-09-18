namespace EmployeeManagementSystem.Models
{
    public class UserDashboardViewModel
    {
        // Employee information
        public int EmployeeId { get; set; }

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Department { get; set; } = "";

        public string Position { get; set; } = "";

        public decimal Salary { get; set; }

        public DateTime HireDate { get; set; }


        // Dashboard statistics
        public double AttendancePercentage { get; set; }

        public double PerformancePercentage { get; set; }

        public int PendingTasks { get; set; }

        public int CompletedTasks { get; set; }


        // Dashboard content
        public List<NewsItemViewModel> News { get; set; } = new();

        public List<UpcomingEventViewModel> UpcomingEvents { get; set; } = new();

        public List<TaskViewModel> Tasks { get; set; } = new();

        public List<PerformanceViewModel> PerformanceHistory { get; set; } = new();
    }


    public class NewsItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Content { get; set; } = "";

        public DateTime PublishedDate { get; set; }

        public bool IsImportant { get; set; }
    }


    public class UpcomingEventViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = "";
    }


    public class TaskViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime? DueDate { get; set; }

        public string Status { get; set; } = "Pending";
    }


    public class PerformanceViewModel
    {
        public int Id { get; set; }

        public string Month { get; set; } = "";

        public double Score { get; set; }

        public string Comments { get; set; } = "";
    }
}