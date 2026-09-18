using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public bool IsImportant { get; set; }
    }
}