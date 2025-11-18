using LibraryWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryContext _context;

        public HomeController(LibraryContext context)
        {
            _context = context;
        }

        // --- Головна сторінка для всіх ---
        [HttpGet("/")]
        public IActionResult Index()
        {
            //показати останні книги та базову інформацію
            var newBooks = _context.Books
                 .OrderByDescending(b => b.DateAdded)
                 .Take(5)
                 .Select(b => new
                 {
                     b.Title,
                     b.PublicationYear
                 })
                 .ToList();

            ViewBag.NewBooks = newBooks;
            ViewBag.TotalBooks = _context.Copies.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.ActiveLoans = _context.Loans.Count(l => l.Status == "Активна" || l.Status == "Прострочена");

            return View();
        }

        // --- Dashboard після логіну ---
        [HttpGet("/dashboard")]
        public IActionResult Dashboard()
        {
            var user = HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            if (user.IsInRole("Admin"))
            {
                return RedirectToAction("AdminDashboard");
            }
            else if (user.IsInRole("Employee"))
            {
                return RedirectToAction("EmployeeDashboard");
            }
            else
            {
                return RedirectToAction("ReaderDashboard");
            }
        }

        // --- Admin Dashboard ---
        [HttpGet("/dashboard/admin")]
        public IActionResult AdminDashboard()
        {
            ViewBag.TotalBooks = _context.Copies.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.ActiveLoans = _context.Loans.Count(l => l.Status == "Активна" || l.Status == "Прострочена");
            ViewBag.TotalMemberships = _context.MembershipTypes.Count();
            ViewBag.TotalReservations = _context.Reservations.Count();

            return View();
        }

        // --- Employee Dashboard ---
        [HttpGet("/dashboard/employee")]
        public IActionResult EmployeeDashboard()
        {
            ViewBag.ActiveLoans = _context.Loans.Count(l => l.Status == "Активна" || l.Status == "Прострочена");
            ViewBag.TotalBooks = _context.Copies.Count();
            ViewBag.ActiveMemberships = _context.Memberships.Count(m => m.EndDate > DateTime.Now);
            ViewBag.TotalReservations = _context.Reservations.Count();

            return View();
        }

        [HttpGet("/dashboard/reader")]
        public IActionResult ReaderDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Index");

            int userId = int.Parse(userIdClaim.Value);

            // Останні книги
            ViewBag.NewBooks = _context.Books
                .OrderByDescending(b => b.DateAdded)
                .Take(5)
                .Select(b => new {
                    b.BookID,
                    b.Title,
                    b.Author,
                    b.CoverImagePath
                })
                .ToList();

            // Загальна кількість книг
            ViewBag.TotalBooks = _context.Copies.Count();

            // Кількість активних бронювань
            ViewBag.ActiveLoansCount = _context.Loans
                .Count(l => l.UserID == userId && l.Status == "Активна");

            // Кількість прострочених бронювань
            ViewBag.OverdueLoansCount = _context.Loans
                .Count(l => l.UserID == userId && l.Status == "Прострочена");

            ViewBag.ActiveLoansList = _context.Loans
                 .Include(l => l.Copy)
                     .ThenInclude(c => c.Book)
                 .Where(l => l.UserID == userId && (l.Status == "Активна" || l.Status == "Прострочена"))
                 .Select(l => new
                 {
                     l.LoanID,
                     l.EndDate,
                     l.Status,
                     Title = l.Copy.Book.Title,
                     Author = l.Copy.Book.Author,
                     Cover = l.Copy.Book.CoverImagePath
                 })
                 .OrderByDescending(l => l.Status == "Прострочена")
                 .ThenBy(l => l.EndDate)
                 .ToList();


            // Активні резервації (лічильник)
            ViewBag.ActiveReservationsCount = _context.Reservations
                .Count(r => r.UserID == userId && r.Status == "Активна");

            // Список активних резервацій
            ViewBag.ActiveReservationsList = _context.Reservations
                .Include(r => r.Copy)
                    .ThenInclude(c => c.Book)
                .Where(r => r.UserID == userId && r.Status == "Активна")
                .Select(r => new
                {
                    r.ReservationID,
                    r.StartDate,
                    r.EndDate,
                    r.Status,
                    Title = r.Copy.Book.Title,
                    Author = r.Copy.Book.Author,
                    Cover = r.Copy.Book.CoverImagePath
                })
                .ToList();


            // Активне членство
            var activeMembership = _context.Memberships
                .Include(m => m.Payment)
                .Include(m => m.MembershipType)
                .Where(m => m.UserID == userId && m.EndDate >= DateTime.Now)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefault();

            ViewBag.ActiveMembership = activeMembership;

            ViewBag.MembershipStatus = activeMembership?.Status ?? "Неактивне";
            ViewBag.MembershipEndDate = activeMembership != null
                ? activeMembership.EndDate.ToString("yyyy-MM-dd")
                : "-";

            // Чи є неоплачений платіж за активним членством
            ViewBag.MembershipPaymentPending =
                activeMembership?.Payment?.Status == "Очікує оплату";

            return View("~/Views/Home/ReaderDashboard.cshtml");
        }

    }
}
