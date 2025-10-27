using Microsoft.AspNetCore.Mvc;

namespace LibraryWeb.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
