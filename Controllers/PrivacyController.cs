using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class PrivacyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}