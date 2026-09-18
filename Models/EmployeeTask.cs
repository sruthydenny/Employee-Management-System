using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class EmployeeTask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [StringLength(1000)]
        public string Description { get; set; } = "";

        public DateTime? DueDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        // Employee assigned to this task
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}