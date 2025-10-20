using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryWeb.Controllers
{
    [Route("user")]
    public class RegistrationController : Controller
    {
        private readonly LibraryContext _context;

        public RegistrationController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("register")]
        public IActionResult Index()
        {
            // Шукає View у Views/User/Register.cshtml
            return View("~/Views/User/Register.cshtml");
        }

        [HttpPost("register")]
        public IActionResult Index(User model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Login == model.Login))
                {
                    ViewBag.Error = "Користувач з таким логіном вже існує!";
                    return View("~/Views/User/Register.cshtml", model);
                }

                model.Role = UserRole.User;
                _context.Users.Add(model);
                _context.SaveChanges();

                ViewBag.Success = "Реєстрація успішна! Тепер увійдіть у систему.";
                return RedirectToAction("Index", "Login");
            }

            return View("~/Views/User/Register.cshtml", model);
        }
    }
}
