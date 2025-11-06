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

            return View();
        }

        // --- Employee Dashboard ---
        [HttpGet("/dashboard/employee")]
        public IActionResult EmployeeDashboard()
        {
            ViewBag.ActiveLoans = _context.Loans.Count(l => l.Status == "Активна" || l.Status == "Прострочена");
            ViewBag.TotalBooks = _context.Copies.Count();
            ViewBag.ActiveMemberships = _context.Memberships.Count(m => m.EndDate > DateTime.Now);

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
                .Select(b => new { b.Title, b.PublicationYear })
                .ToList();

            ViewBag.TotalBooks = _context.Copies.Count();

            // Активні позики користувача
            ViewBag.MyActiveLoans = _context.Loans.Count(l => l.UserID == userId && (l.Status == "Активна" || l.Status == "Прострочена"));

            // Активне членство
            ViewBag.ActiveMembership = _context.Memberships
                .Where(m => m.UserID == userId && m.EndDate > DateTime.Now)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefault();

            ViewBag.ActiveReservations = _context.Reservations
                .Count(r => r.UserID == userId && r.Status == "Активна");


            return View("~/Views/Home/ReaderDashboard.cshtml");
        }

    }
}
