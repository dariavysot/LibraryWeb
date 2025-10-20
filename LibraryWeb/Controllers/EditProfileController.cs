using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LibraryWeb.Controllers
{
    [Authorize]
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Index", "Login");

            int userId = int.Parse(userIdClaim.Value);
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return RedirectToAction("Index", "Login");

            return View("~/Views/User/EditProfile.cshtml", user);
        }

        [HttpPost("edit-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(User model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Index", "Login");

            int userId = int.Parse(userIdClaim.Value);
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return RedirectToAction("Index", "Login");

            model.UserID = user.UserID; // запобігаємо підміні ID

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError("Name", "Ім’я обов’язкове");
            if (string.IsNullOrWhiteSpace(model.Login))
                ModelState.AddModelError("Login", "Логін обов’язковий");
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email обов’язковий");
            }
            else if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Введіть коректний Email.");
            }
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError("Phone", "Телефон обов’язковий");

            if (_context.Users.Any(u => u.Login == model.Login && u.UserID != userId))
                ModelState.AddModelError("Login", "Цей логін уже використовується");

            if (!ModelState.IsValid)
                return View("~/Views/User/EditProfile.cshtml", model);

            user.Name = model.Name;
            user.Login = model.Login;
            user.Email = model.Email;
            user.Phone = model.Phone;

            if (!string.IsNullOrWhiteSpace(model.UserPassword))
            {
                var hasher = new PasswordHasher<User>();
                user.UserPassword = hasher.HashPassword(user, model.UserPassword);
            }

            _context.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);

            TempData["Success"] = "Профіль успішно оновлено!";
            return RedirectToAction("Index", "Profile");
        }
    }
}
