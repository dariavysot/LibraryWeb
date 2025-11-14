using LibraryWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Controllers
{
    [Route("admin/reservations")]
    public class AdminReservationController : Controller
    {
        private readonly LibraryContext _context;

        public AdminReservationController(LibraryContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("")]
        public IActionResult Index(string? search, string? status, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Copy)
                .ThenInclude(c => c.Book)
                .Include(r => r.Payment)
                .AsQueryable();

            // --- Пошук по тексту ---
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.ReservationID.ToString().Contains(search) ||
                    (r.UserName != null && r.UserName.Contains(search)) ||                      // Імʼя з самої резервації
                    (r.User != null && r.User.Name.Contains(search)) ||                         // Імʼя з таблиці Users
                    (r.Copy != null && r.Copy.Book != null && r.Copy.Book.Title.Contains(search)) || // Назва книги
                    r.InventoryNum.ToString().Contains(search)                                  // Інвентарний номер
                );
            }
            // --- Фільтр по статусу ---
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            // --- Фільтр по датах ---
            if (dateFrom.HasValue)
            {
                query = query.Where(r => r.StartDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(r => r.EndDate <= dateTo.Value);
            }

            var reservations = query
                .OrderByDescending(r => r.StartDate)
                .ToList();

            // Зберігаємо значення для форми
            ViewBag.Search = search;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(reservations);
        }


        [Authorize(Roles = "Admin")]
        // --- Видалення резервації (для адміна) ---
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.ReservationID == id);
            if (reservation == null) return NotFound();

            _context.Reservations.Remove(reservation);
            _context.SaveChanges();

            TempData["Success"] = "Резервація видалена адміністратором.";
            return RedirectToAction("Index");
        }
    }
}
