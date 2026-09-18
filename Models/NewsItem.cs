using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class NewsItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public bool IsImportant { get; set; }
    }
}