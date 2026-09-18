using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class CompanyEvent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        [Required]
        public DateTime EventDate { get; set; }

        public string Location { get; set; } = "";
    }
}