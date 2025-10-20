using LibraryWeb.Data;
using Microsoft.AspNetCore.Mvc;

namespace LibraryWeb.Controllers
{
    [Route("user")]
    public class LoginController : Controller
    {
        private readonly LibraryContext _context;

        public LoginController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("login")]
        public IActionResult Index()
        {
            return View("~/Views/User/Login.cshtml");
        }

        [HttpPost("login")]
        public IActionResult Index(string login, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == login && u.UserPassword == password);

            if (user == null)
            {
                ViewBag.Error = "Невірний логін або пароль.";
                return View("~/Views/User/Login.cshtml");
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            return RedirectToAction("Index", "Profile");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
