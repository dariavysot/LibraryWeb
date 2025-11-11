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
                ModelState.AddModelError("Name", "Name is required");
            if (string.IsNullOrWhiteSpace(model.Login))
                ModelState.AddModelError("Login", "Login is required");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Email is required");
            else if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ModelState.AddModelError("Email", "Enter a valid Email");
            if (string.IsNullOrWhiteSpace(model.UserPassword))
                ModelState.AddModelError("UserPassword", "Password is required");

            if (model.BirthDate == default)
                ModelState.AddModelError("BirthDate", "Date of birth is required");
            else if (model.BirthDate > DateTime.Today)
                ModelState.AddModelError("BirthDate", "Birth date cannot be in the future");

            if (_context.Users.Any(u => u.Login == model.Login))
                ModelState.AddModelError("Login", "A user with this login already exists!");

            if (!ModelState.IsValid)
                return View("~/Views/User/Register.cshtml", model);

            var hasher = new PasswordHasher<User>();
            model.UserPassword = hasher.HashPassword(model, model.UserPassword);

            model.Role = UserRole.User;
            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Registration successful!";
            return RedirectToAction("Index", "Login");
        }
    }
}
