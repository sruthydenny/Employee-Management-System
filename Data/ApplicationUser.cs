using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementSystem.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int? EmployeeId { get; set; }

        public Employee? Employee { get; set; }
    }
}