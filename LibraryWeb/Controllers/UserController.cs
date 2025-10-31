using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/users")]
    public class UserController : Controller
    {
        private readonly LibraryContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserController(LibraryContext context)
        {
            _context = context;
        }

        // --- Список користувачів ---
        [HttpGet("")]
        public IActionResult Index()
        {
            var users = _context.Users
                .Include(u => u.Membership)
                .ToList();

            return View("~/Views/User/Admin/Index.cshtml", users);
        }

        // --- Редагування користувача ---
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено.";
                return RedirectToAction("Index");
            }

            return View("~/Views/User/Admin/Edit.cshtml", user);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Форма заповнена некоректно.";
                return View("~/Views/User/Admin/Edit.cshtml", model);
            }

            try
            {
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Index");
                }

                user.Name = model.Name;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Role = model.Role;
                user.Login = model.Login;

                // Якщо введено новий пароль — хешуємо перед збереженням
                if (!string.IsNullOrWhiteSpace(model.UserPassword))
                {
                    user.UserPassword = _passwordHasher.HashPassword(user, model.UserPassword);
                }

                _context.SaveChanges();

                TempData["Success"] = "Дані користувача успішно оновлено.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Сталася помилка при збереженні: {ex.Message}";
                return View("~/Views/User/Admin/Edit.cshtml", model);
            }
        }

        // --- Видалення користувача ---
        [HttpGet("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено.";
                return RedirectToAction("Index");
            }

            return View("~/Views/User/Admin/Delete.cshtml", user);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Index");
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                TempData["Success"] = "Користувача видалено.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка при видаленні: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
