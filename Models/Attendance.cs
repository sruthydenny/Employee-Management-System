using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }

        public bool IsLate { get; set; }
    }
}