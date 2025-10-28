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
            // Можна показати останні книги та базову інформацію
            var newBooks = _context.Books
                .OrderByDescending(b => b.DateAdded)
                .Take(5)
                .ToList();

            ViewBag.NewBooks = newBooks;
            ViewBag.TotalBooks = _context.Books.Count();
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
            ViewBag.TotalBooks = _context.Books.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.ActiveLoans = _context.Loans.Count(l => l.Status == "Активна" || l.Status == "Прострочена");
            ViewBag.TotalMemberships = _context.MembershipTypes.Count();

            return View();
        }

        // --- Employee Dashboard ---
        [HttpGet("/dashboard/employee")]
        public IActionResult EmployeeDashboard()
        {
            ViewBag.ActiveLoans = _context.Loans
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Where(l => l.Status == "Активна" || l.Status == "Прострочена")
                .ToList();

            ViewBag.AvailableCopies = _context.Copies.Count(c => c.Status == "Доступна");

            return View();
        }

        [HttpGet("/dashboard/reader")]
        public IActionResult ReaderDashboard()
        {
            var user = HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Index");
            }

            var userId = int.Parse(userIdClaim.Value);

            ViewBag.MyLoans = _context.Loans
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Where(l => l.UserID == userId)
                .ToList();

            return View();
        }

    }
}
