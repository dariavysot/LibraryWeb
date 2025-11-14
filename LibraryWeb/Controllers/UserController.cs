using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Index(string search, UserRole? role, string membershipType)
        {
            var query = _context.Users
                .Include(u => u.Membership)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.Name != null && u.Name.Contains(search)) ||
                     (u.Login != null && u.Login.Contains(search))
                );
            }

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (!string.IsNullOrEmpty(membershipType))
                query = query.Where(u => u.Membership != null && u.Membership.Type == membershipType);

            var users = query.ToList();

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.MembershipType = membershipType;
            ViewBag.MembershipTypes = _context.Memberships
                .Select(m => m.Type)
                .Distinct()
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

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Reader", Text = "Reader", Selected = (user.Role == UserRole.Reader) },
                new SelectListItem { Value = "Employee", Text = "Employee", Selected = (user.Role == UserRole.Employee) },
                new SelectListItem { Value = "Admin", Text = "Admin", Selected = (user.Role == UserRole.Admin) }
            };

            return View("~/Views/User/Admin/Edit.cshtml", user);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User model)
        {

            ModelState.Remove("Login");
            ModelState.Remove("UserPassword");

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
                var user = _context.Users
                    .Include(u => u.Loans)
                    .Include(u => u.Reservations)
                    .Include(u => u.Membership)
                    .FirstOrDefault(u => u.UserID == id);

                if (user == null)
                {
                    TempData["Error"] = "Користувача не знайдено.";
                    return RedirectToAction("Index");
                }

                // Перевірка на активні або прострочені позики
                var hasActiveOrOverdue = user.Loans
                    .Any(l => l.Status == "Активна" || l.Status == "Прострочена");

                if (hasActiveOrOverdue)
                {
                    TempData["Error"] = "Неможливо видалити користувача, який має активну або прострочену позику.";
                    return RedirectToAction("Index");
                }

                // --- Зберігаємо ім’я користувача у його резерваціях ---
                foreach (var reservation in user.Reservations)
                {
                    reservation.UserName = user.Name;
                    reservation.UserID = null; // розриваємо зв’язок
                }

                // Видалення членства вручну
                if (user.Membership != null)
                    _context.Memberships.Remove(user.Membership);


                // Видалення користувача (Membership видалиться каскадно)
                _context.Users.Remove(user);
                _context.SaveChanges();

                TempData["Success"] = "Користувача та пов’язані дані успішно видалено.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка при видаленні: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
