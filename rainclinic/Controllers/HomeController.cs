using System.Diagnostics;
using rainclinic.Models;
using Microsoft.AspNetCore.Mvc;

namespace rainclinic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult aile_ter()
        {
            return View();
        }
        public IActionResult bireysel_ter()
        {
            return View();
        }
        public IActionResult cift_ter()
        {
            return View();
        }
        public IActionResult ekibimiz()
        {
            return View();
        }
        public IActionResult kurumsal_ter()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
