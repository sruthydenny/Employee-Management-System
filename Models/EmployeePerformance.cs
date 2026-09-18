using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class EmployeePerformance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Range(0, 100)]
        public double Score { get; set; }

        [Required]
        [StringLength(50)]
        public string Month { get; set; } = "";

        public string Comments { get; set; } = "";
    }
}