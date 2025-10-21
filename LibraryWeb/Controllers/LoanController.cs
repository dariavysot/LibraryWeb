using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Route("loans")]
    public class LoanController : Controller
    {
        private readonly LibraryContext _context;

        public LoanController(LibraryContext context)
        {
            _context = context;
        }

        // --- Список всіх позик ---
        [HttpGet("")]
        public IActionResult Index()
        {
            var loans = _context.Loans
                .Include(l => l.Reader)
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Include(l => l.Employee)
                .ToList();

            return View("~/Views/Loan/Index.cshtml", loans);
        }

        // --- Деталі ---
        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            var loan = _context.Loans
                .Include(l => l.Reader)
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Include(l => l.Employee)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null) return NotFound();
            return View("~/Views/Loan/Details.cshtml", loan);
        }

        // --- Створення ---
        // Створення позики (GET)
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.Users = _context.Users.ToList();
            ViewBag.Books = _context.Books
                .Where(b => b.Copies.Any(c => c.Status == "Доступна"))
                .ToList();

            ViewBag.StartDate = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd");

            return View();
        }

        // Створення позики (POST)
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int ReaderID, int selectedBookId)
        {
            var reader = _context.Users.FirstOrDefault(u => u.UserID == ReaderID);
            if (reader == null)
            {
                TempData["Error"] = "Оберіть існуючого читача.";
                return RedirectToAction("Create");
            }

            var copy = _context.Copies
                .Include(c => c.Book)
                .FirstOrDefault(c => c.BookID == selectedBookId && c.Status == "Доступна");

            if (copy == null)
            {
                TempData["Error"] = "Немає доступних примірників цієї книги.";
                return RedirectToAction("Create");
            }

            var loan = new Loan
            {
                ReaderID = ReaderID,
                InventoryNum = copy.InventoryNum,
                Status = "Активна",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(14)
            };

            copy.Status = "Позичена";

            _context.Loans.Add(loan);
            _context.SaveChanges();

            TempData["Success"] = $"Позика для книги '{copy.Book.Title}' успішно створена!";
            return RedirectToAction("Index");
        }

        // --- Повернення (GET) ---
        [HttpGet("return/{id}")]
        public IActionResult Return(int id)
        {
            var loan = _context.Loans
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Include(l => l.Reader)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null)
            {
                TempData["Error"] = "Позика не знайдена.";
                return RedirectToAction("Index");
            }

            return View("~/Views/Loan/Return.cshtml", loan);
        }

        // --- Повернення (POST) ---
        [HttpPost("return/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult ReturnConfirmed(int id, DateTime returnDate, string condition)
        {
            var loan = _context.Loans
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null)
            {
                TempData["Error"] = "Позика не знайдена.";
                return RedirectToAction("Index");
            }

            loan.ReturnDate = returnDate;
            loan.Copy.Status = "Доступна";
            loan.Status = "Повернено";

            // --- Обчислення штрафу ---
            double fine = 0;

            if (returnDate > loan.EndDate)
                fine += (returnDate - loan.EndDate).Days * 5; // 5 грн за день запізнення

            if (condition == "Пошкоджена")
                fine += 100; // фіксований штраф за пошкодження

            _context.SaveChanges();

            if (fine > 0)
                TempData["Warning"] = $"Книга '{loan.Copy.Book.Title}' повернена із штрафом {fine} грн.";
            else
                TempData["Success"] = $"Книга '{loan.Copy.Book.Title}' успішно повернена без штрафу.";

            return RedirectToAction("Index");
        }

        // --- Видалення ---
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var loan = _context.Loans.Include(l => l.Copy).FirstOrDefault(l => l.LoanID == id);
            if (loan == null) return NotFound();

            loan.Copy.Status = "Доступна";
            _context.Loans.Remove(loan);
            _context.SaveChanges();

            TempData["Success"] = "Позика успішно видалена!";
            return RedirectToAction("Index");
        }
    }
}
