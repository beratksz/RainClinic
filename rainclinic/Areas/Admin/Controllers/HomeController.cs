using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace rainclinic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return View(claims);
        }

    }
}
