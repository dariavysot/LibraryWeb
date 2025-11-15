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
        public IActionResult Index(
    string? statusFilter,
    string? searchId = "",
    string? searchUser = "",
    string? searchBook = "",
    string? searchInventory = "",
    DateTime? dateFrom = null,
    DateTime? dateTo = null)
        {
            var today = DateTime.Today;

            var outdated = _context.Reservations
                .Where(r => r.EndDate < today && r.Status != "Скасована")
                .ToList();

            if (outdated.Any())
            {
                foreach (var r in outdated)
                    r.Status = "Скасована";

                _context.SaveChanges();
            }

            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Copy).ThenInclude(c => c.Book)
                .Include(r => r.Payment)
                .AsQueryable();

            // --- Пошук по ID резервації ---
            if (!string.IsNullOrWhiteSpace(searchId) && int.TryParse(searchId, out int resId))
                query = query.Where(r => r.ReservationID == resId);

            // --- Пошук по користувачу ---
            if (!string.IsNullOrWhiteSpace(searchUser))
            {
                var userLower = searchUser.ToLower();
                query = query.Where(r =>
                    (r.User != null && (
                        r.User.Name.ToLower().Contains(userLower) ||
                        r.User.Login.ToLower().Contains(userLower)
                    )) ||
                    (r.UserName != null && r.UserName.ToLower().Contains(userLower))
                );
            }

            // --- Пошук по книзі ---
            if (!string.IsNullOrWhiteSpace(searchBook))
            {
                var bookLower = searchBook.ToLower();
                query = query.Where(r =>
                    r.Copy != null &&
                    r.Copy.Book != null &&
                    r.Copy.Book.Title.ToLower().Contains(bookLower));
            }

            // --- Пошук по інвентарному номеру ---
            if (!string.IsNullOrWhiteSpace(searchInventory) &&
                int.TryParse(searchInventory, out int invNumber))
            {
                query = query.Where(r => r.InventoryNum == invNumber);
            }

            // --- Фільтр по статусу ---
            var statuses = new List<string> { "Усі", "Очікує оплату", "Активна", "Скасована" };
            ViewBag.Statuses = statuses;
            ViewBag.StatusFilter = statusFilter ?? "Усі";

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "Усі")
                query = query.Where(r => r.Status == statusFilter);

            // --- Фільтр по датах ---
            if (dateFrom.HasValue)
                query = query.Where(r => r.StartDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(r => r.EndDate <= dateTo.Value);

            var reservations = query
                .OrderByDescending(r => r.StartDate)
                .ToList();

            ViewBag.SearchId = searchId;
            ViewBag.SearchUser = searchUser;
            ViewBag.SearchBook = searchBook;
            ViewBag.SearchInventory = searchInventory;
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
