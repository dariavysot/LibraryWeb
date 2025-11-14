using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Controllers
{
    [Authorize]
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly LibraryContext _context;

        public PaymentController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? status, string? type, string? paymentId, string? userSearch, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Payments.Include(p => p.User).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(p => p.Type == type);

            // --- Search by Payment ID ---
            if (!string.IsNullOrEmpty(paymentId))
            {
                if (int.TryParse(paymentId, out int pid))
                    query = query.Where(p => p.PaymentID == pid);
                else
                    query = query.Where(p => p.PaymentID.ToString().Contains(paymentId));
            }

            if (!string.IsNullOrEmpty(userSearch))
            {
                if (int.TryParse(userSearch, out int userId))
                {
                    query = query.Where(p => p.UserID == userId);
                }
                else
                {
                    query = query.Where(p => p.UserName != null && p.UserName.Contains(userSearch));
                }
            }

            if (fromDate.HasValue)
                query = query.Where(p => p.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.Date <= toDate.Value);



            var payments = await query.OrderByDescending(p => p.Date).ToListAsync();
            ViewBag.TotalAmount = payments.Sum(p => p.Amount);
            ViewBag.StatusList = new List<string> { "Оплачено", "Очікує оплату" };
            ViewBag.TypeList = _context.Payments.Select(p => p.Type).Distinct().ToList();

            return View(payments);
        }

        // --- Створення платежу для резервації ---
        [HttpPost("create/{reservationId}")]
        public async Task<IActionResult> CreatePaymentForReservation(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Payment)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationID == reservationId);

            if (reservation == null)
                return NotFound("Резервацію не знайдено.");

            if (reservation.Payment != null)
                return BadRequest("Оплата для цієї резервації вже існує.");

            var payment = new Payment
            {
                Amount = reservation.Amount,
                Type = "Резервація книги",
                Status = "Очікує оплату",
                UserID = reservation.UserID,
                UserName = reservation.User?.Name,
                Reservation = reservation // EF автоматично встановить ReservationID
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Платіж створено, очікує оплати.";
            return RedirectToAction("Index", "Reservation");
        }

        // --- Підтвердження оплати ---
        [HttpPost("confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId);

            if (payment == null)
                return NotFound("Платіж не знайдено.");

            payment.Status = "Оплачено";
            payment.Date = DateTime.Now;

            if (payment.Reservation != null)
            {
                payment.Reservation.Status = "Активна";
                _context.Reservations.Update(payment.Reservation);
            }

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Оплата успішно підтверджена.";
            return RedirectToAction("Index", "Reservation");
        }

        // Підтвердження оплати членства
        [HttpPost("membership/confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmMembershipPayment(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Membership)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId);

            if (payment == null || payment.Membership == null)
            {
                TempData["Error"] = "Не знайдено пов'язане членство.";
                return RedirectToAction("Index", "Membership");
            }

            if (payment.Status == "Оплачено")
            {
                TempData["Error"] = "Цей платіж уже підтверджено.";
                return RedirectToAction("Details", "Membership", new { id = payment.Membership.MembershipID });
            }

            payment.Status = "Оплачено";
            payment.Date = DateTime.Now;

            payment.Membership.Status = "Активне";
            _context.Memberships.Update(payment.Membership);

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Оплата успішно підтверджена.";
            return RedirectToAction("Details", "Membership", new { id = payment.Membership.MembershipID });
        }

    }
}
