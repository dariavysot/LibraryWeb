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
        // --- Список усіх резервацій ---
        [HttpGet("")]
        public IActionResult Index()
        {
            var reservations = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Copy)
                .ThenInclude(c => c.Book)
                .Include(r => r.Payment)
                .OrderByDescending(r => r.StartDate)
                .ToList();

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
