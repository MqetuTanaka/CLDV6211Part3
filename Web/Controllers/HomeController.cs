using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebdevP3Context _context;


        public HomeController(WebdevP3Context context)
        {
            _context = context;
        }


        public IActionResult Index()
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

        public IActionResult TestDbConnection()
        {
            try
            {
                bool canConnect = _context.Database.CanConnect();
                return Content(canConnect ? "✅ Database connection successful." : "❌ Connection failed.");
            }
            catch (Exception ex)
            {
                return Content($"❌ Database connection error: {ex.Message}");
            }
        }
    }
}
