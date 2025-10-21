using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Route("reservations")]
    public class ReservationController : Controller
    {
        private readonly LibraryContext _context;

        public ReservationController(LibraryContext context)
        {
            _context = context;
        }

        // --- Список всіх резервацій ---
        [HttpGet("")]
        public IActionResult Index()
        {
            // Отримуємо ID поточного користувача
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Не вдалося визначити користувача.";
                return RedirectToAction("Index", "Home");
            }
            int userId = int.Parse(userIdClaim.Value);

            // Вибираємо лише резервації для цього користувача
            var reservations = _context.Reservations
                .Where(r => r.UserID == userId)
                .Include(r => r.User)
                .Include(r => r.Copy)
                .ThenInclude(c => c.Book)
                .ToList();

            return View(reservations);
        }

        // --- Створення резервації (GET) ---
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.Books = _context.Books
                .Where(b => b.Copies.Any(c => c.Status == "Доступна"))
                .ToList();

            return View();
        }

        // --- Створення резервації (POST) ---
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int selectedBookId, DateTime startDate, DateTime endDate)
        {
            // Отримуємо ID поточного користувача
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Не вдалося визначити користувача.";
                return RedirectToAction("Index", "Home");
            }
            int userId = int.Parse(userIdClaim.Value);

            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                TempData["Error"] = "Користувач не знайдений.";
                return RedirectToAction("Index", "Home");
            }

            var copy = _context.Copies
                .Include(c => c.Book)
                .FirstOrDefault(c => c.BookID == selectedBookId && c.Status == "Доступна");

            if (copy == null)
            {
                TempData["Error"] = "Немає доступних примірників цієї книги.";
                return RedirectToAction("Index");
            }

            // Перевірка на конфлікт з іншими резерваціями
            var conflict = _context.Reservations.Any(r =>
                r.InventoryNum == copy.InventoryNum &&
                ((startDate >= r.StartDate && startDate < r.EndDate) ||
                 (endDate > r.StartDate && endDate <= r.EndDate)));

            if (conflict)
            {
                TempData["Error"] = "Цей примірник вже зарезервований на обраний проміжок.";
                return RedirectToAction("Index");
            }

            var reservation = new Reservation
            {
                UserID = user.UserID,
                InventoryNum = copy.InventoryNum,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Активна"
            };

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            TempData["Success"] = $"Резервація для книги '{copy.Book.Title}' створена!";
            return RedirectToAction("Index");
        }

        // --- Видалення резервації ---
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.ReservationID == id);
            if (reservation == null) return NotFound();

            _context.Reservations.Remove(reservation);
            _context.SaveChanges();

            TempData["Success"] = "Резервація успішно видалена!";
            return RedirectToAction("Index");
        }
    }
}
