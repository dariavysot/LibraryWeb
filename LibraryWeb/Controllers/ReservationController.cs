using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> Create(string selectedBookTitle, DateTime startDate, DateTime endDate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Не вдалося визначити користувача.";
                return RedirectToAction("Index", "Home");
            }
            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users
                .Include(u => u.Membership)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                TempData["Error"] = "Користувач не знайдений.";
                return RedirectToAction("Index", "Home");
            }

            // --- Перевірка членства ---
            if (user.Membership == null || user.Membership.EndDate < DateTime.Now || user.Membership.Status != "Активне")
            {
                TempData["Error"] = "Ви не можете створити резервацію без активного членства.";
                return RedirectToAction("Index", "Home");
            }

            // --- Знаходимо книгу за назвою ---
            var book = await _context.Books
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(b => b.Title == selectedBookTitle);

            if (book == null)
            {
                TempData["Error"] = "Книга не знайдена. Переконайтесь, що ви вибрали її зі списку.";
                return RedirectToAction("Create");
            }

            int selectedBookId = book.BookID;

            // --- Перевірка на повторну резервацію ---
            bool alreadyReserved = await _context.Reservations
                .Include(r => r.Copy)
                .AnyAsync(r =>
                    r.UserID == userId &&
                    r.Copy.BookID == selectedBookId &&
                    (r.Status == "Очікує оплату" || r.Status == "Активна")
                );

            if (alreadyReserved)
            {
                TempData["Error"] = "Ви вже створили резервацію для цієї книги.";
                return RedirectToAction("Index", "Home");
            }

            // --- Перевірка дат ---
            if (endDate <= startDate)
            {
                TempData["Error"] = "Дата завершення має бути пізнішою за дату початку.";
                return RedirectToAction("Create");
            }

            // --- Доступні копії ---
            var copies = await _context.Copies
                .Include(c => c.Book)
                .Where(c => c.BookID == selectedBookId && c.Status == "Доступна")
                .ToListAsync();

            if (!copies.Any())
            {
                TempData["Error"] = "Немає доступних примірників цієї книги.";
                return RedirectToAction("Index");
            }

            // --- Пошук копії без конфліктів по датах ---
            Copy? selectedCopy = null;

            foreach (var copy in copies)
            {
                bool conflict = await _context.Reservations.AnyAsync(r =>
                    r.InventoryNum == copy.InventoryNum &&
                    (r.Status == "Активна" || r.Status == "Очікує оплату") &&
                    (startDate < r.EndDate && endDate > r.StartDate)
                );

                if (!conflict)
                {
                    selectedCopy = copy;
                    break;
                }
            }

            if (selectedCopy == null)
            {
                TempData["Error"] = "Немає доступних копій на обраний період.";
                return RedirectToAction("Index");
            }

            // --- Розрахунок ---
            int totalDays = (endDate - startDate).Days;
            if (totalDays <= 0) totalDays = 1;

            decimal totalAmount = totalDays * 50m;

            // --- Створення резервації ---
            var reservation = new Reservation
            {
                UserID = user.UserID,
                InventoryNum = selectedCopy.InventoryNum,
                UserName = user.Name,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Очікує оплату",
                Amount = totalAmount
            };

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

            TempData["Success"] = $"Резервація створена!";
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
            var reservation = _context.Reservations
                .Include(r => r.Payment)
                .FirstOrDefault(r => r.ReservationID == id);

            if (reservation == null)
                return NotFound();

            // Якщо резервація вже була скасована — нічого не робимо
            if (reservation.Status != "Скасована")
            {
                reservation.Status = "Скасована";

                // Якщо є платіж, що очікує оплату → скасовуємо його
                if (reservation.Payment != null &&
                    reservation.Payment.Status == "Очікує оплату")
                {
                    reservation.Payment.Status = "Скасований";
                }

                _context.SaveChanges();
            }

            TempData["Success"] = "Резервація успішно скасована!";
            return RedirectToAction("Index");
        }

    }
}
