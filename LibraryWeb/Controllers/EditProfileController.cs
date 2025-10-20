using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryWeb.Controllers
{
    [Route("user")]
    public class EditProfileController : Controller
    {
        private readonly LibraryContext _context;

        public EditProfileController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("edit-profile")]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            return View("~/Views/User/EditProfile.cshtml", user);
        }

        [HttpPost("edit-profile")]
        public IActionResult Index(User model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            // Перевірка на пусті поля
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError("Name", "Ім'я обов'язкове");
            if (string.IsNullOrWhiteSpace(model.Login))
                ModelState.AddModelError("Login", "Логін обов'язковий");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Email обов'язковий");
            if (string.IsNullOrWhiteSpace(model.UserPassword))
                ModelState.AddModelError("UserPassword", "Пароль обов'язковий");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError("Phone", "Phone обов'язковий");


            // Перевірка унікальності логіна
            if (_context.Users.Any(u => u.Login == model.Login && u.UserID != userId))
                ModelState.AddModelError("Login", "Цей логін вже використовується");

            if (!ModelState.IsValid)
                return View("~/Views/User/EditProfile.cshtml", model);

            user.Name = model.Name;
            user.Login = model.Login;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.UserPassword = model.UserPassword;

            _context.SaveChanges();

            // Оновлення сесії
            HttpContext.Session.SetString("UserName", user.Name);
            //HttpContext.Session.SetString("UserRole", user.Role.ToString());

            TempData["Success"] = "Профіль успішно оновлено!";
            return RedirectToAction("Index", "Profile");
        }

    }
}
