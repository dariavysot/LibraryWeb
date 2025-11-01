using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Authorize(Roles = "User")]
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Не вдалося визначити користувача.";
                return RedirectToAction("Index", "Home");
            }
            int userId = int.Parse(userIdClaim.Value);

            var reservations = _context.Reservations
                .Where(r => r.UserID == userId)
                .Include(r => r.User)
                .Include(r => r.Copy)
                .ThenInclude(c => c.Book)
                .Include(r => r.Payment)
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
        public async Task<IActionResult> Create(int selectedBookId, DateTime startDate, DateTime endDate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Не вдалося визначити користувача.";
                return RedirectToAction("Index", "Home");
            }
            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Користувач не знайдений.";
                return RedirectToAction("Index", "Home");
            }

            var copy = await _context.Copies
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.BookID == selectedBookId && c.Status == "Доступна");

            if (copy == null)
            {
                TempData["Error"] = "Немає доступних примірників цієї книги.";
                return RedirectToAction("Index");
            }

            if (endDate <= startDate)
            {
                TempData["Error"] = "Дата завершення має бути пізнішою за дату початку.";
                return RedirectToAction("Create");
            }

            bool conflict = await _context.Reservations.AnyAsync(r =>
                r.InventoryNum == copy.InventoryNum &&
                ((startDate >= r.StartDate && startDate < r.EndDate) ||
                 (endDate > r.StartDate && endDate <= r.EndDate)));

            if (conflict)
            {
                TempData["Error"] = "Цей примірник вже зарезервований на обраний проміжок.";
                return RedirectToAction("Index");
            }

            int totalDays = (endDate - startDate).Days;
            if (totalDays <= 0) totalDays = 1;
            decimal dailyRate = 20m;
            decimal totalAmount = totalDays * dailyRate;

            // --- Створюємо Reservation ---
            var reservation = new Reservation
            {
                UserID = user.UserID,
                InventoryNum = copy.InventoryNum,
                UserName = user.Name,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Очікує оплату",
                Amount = totalAmount
            };

            // --- Створюємо Payment і прив'язуємо до Reservation через навігаційне властивість ---
            var payment = new Payment
            {
                UserID = user.UserID,
                UserName = user.Name,
                Amount = totalAmount,
                Type = "Резервація книги",
                Status = "Очікує оплату",
                Reservation = reservation
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Резервація для книги '{copy.Book.Title}' створена. Сума до оплати: {totalAmount} грн";
            return RedirectToAction("Index");
        }


        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            var reservation = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Copy)
                .ThenInclude(c => c.Book)
                .Include(r => r.Payment)
                .FirstOrDefault(r => r.ReservationID == id);

            if (reservation == null)
                return NotFound();

            return View(reservation);
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
