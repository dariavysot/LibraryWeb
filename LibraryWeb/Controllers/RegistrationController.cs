using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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
            return View("~/Views/User/Register.cshtml");
        }

        [HttpPost("register")]
        public IActionResult Index(User model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError("Name", "Ім’я обов’язкове");
            if (string.IsNullOrWhiteSpace(model.Login))
                ModelState.AddModelError("Login", "Логін обов’язковий");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Email обов’язковий");
            else if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ModelState.AddModelError("Email", "Введіть коректний Email");
            if (string.IsNullOrWhiteSpace(model.UserPassword))
                ModelState.AddModelError("UserPassword", "Пароль обов’язковий");

            if (model.BirthDate == default)
                ModelState.AddModelError("BirthDate", "Дата народження обов’язкова");
            else if (model.BirthDate > DateTime.Today)
                ModelState.AddModelError("BirthDate", "Дата народження не може бути в майбутньому");

            if (_context.Users.Any(u => u.Login == model.Login))
                ModelState.AddModelError("Login", "Користувач з таким логіном вже існує!");

            if (!ModelState.IsValid)
                return View("~/Views/User/Register.cshtml", model);

            var hasher = new PasswordHasher<User>();
            model.UserPassword = hasher.HashPassword(model, model.UserPassword);

            model.Role = UserRole.Admin;
            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Реєстрація успішна!";
            return RedirectToAction("Index", "Login");
        }
    }
}
