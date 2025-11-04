using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
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
        public IActionResult Index(string? statusFilter, string search = "")
        {
            var today = DateTime.Today;

            // --- Оновлення прострочених ---
            var activeLoans = _context.Loans.Where(l => l.Status == "Активна").ToList();
            foreach (var loan in activeLoans)
            {
                if (loan.EndDate < today)
                    loan.Status = "Прострочена";
            }
            _context.SaveChanges();

            var statuses = new List<string> { "Усі", "Активна", "Прострочена", "Повернено" };
            ViewBag.Statuses = statuses;

            if (string.IsNullOrEmpty(statusFilter))
                statusFilter = "Усі";

            ViewBag.StatusFilter = statusFilter;
            ViewBag.Search = search;

            var loans = _context.Loans
                .Include(l => l.User)
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .AsQueryable();

            if (statusFilter != "Усі")
                loans = loans.Where(l => l.Status == statusFilter);

            // --- Пошук ---
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                loans = loans.Where(l =>
                    (l.User != null && l.User.Name.ToLower().Contains(search)) ||
                    (l.UserName != null && l.UserName.ToLower().Contains(search)) ||
                    l.Copy.Book.Title.ToLower().Contains(search));
            }

            return View("~/Views/Loan/Index.cshtml", loans.ToList());
        }

        // --- Деталі ---
        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            var loan = _context.Loans
                .Include(l => l.User)
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null) return NotFound();
            return View("~/Views/Loan/Details.cshtml", loan);
        }

        // --- Створення позики (POST) ---
        // GET: /loans/create
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.Users = _context.Users.ToList();
            ViewBag.Books = _context.Books
                .Where(b => b.Copies.Any(c => c.Status == "Доступна"))
                .ToList();

            return View();
        }

        // POST: /loans/create
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int UserID, int selectedBookId)
        {
            var user = _context.Users.Include(u => u.Membership).FirstOrDefault(u => u.UserID == UserID);
            if (user == null)
            {
                TempData["Error"] = "Оберіть користувача.";
                return RedirectToAction("Create");
            }

            if (user.Membership == null || user.Membership.Status != "Активне")
            {
                TempData["Error"] = $"У користувача '{user.Name}' немає активного членства.";
                TempData["UserIdWithoutMembership"] = user.UserID;
                return RedirectToAction("CreateMembershipPrompt");
            }

            var loanStart = DateTime.Now;
            var loanEnd = loanStart.AddDays(14);

            // Отримуємо всі доступні примірники обраної книги
            var availableCopies = _context.Copies
                .Include(c => c.Book)
                .Where(c => c.BookID == selectedBookId && c.Status == "Доступна")
                .ToList();

            Copy? copyToLoan = null;

            foreach (var copy in availableCopies)
            {
                var reservation = _context.Reservations
                    .Include(r => r.User)
                    .FirstOrDefault(r =>
                        r.InventoryNum == copy.InventoryNum &&
                        loanStart <= r.EndDate && r.StartDate <= loanEnd);

                if (reservation == null)
                {
                    // Примірник вільний — можна видати
                    copyToLoan = copy;
                    break;
                }
                else if (reservation.UserID == user.UserID)
                {
                    // Резервація на того ж користувача — видаляємо та беремо цей примірник
                    _context.Reservations.Remove(reservation);
                    copyToLoan = copy;
                    break;
                }
                // Інакше примірник заброньований іншим — пропускаємо
            }

            if (copyToLoan == null)
            {
                TempData["Error"] = "Усі примірники цієї книги зарезервовані іншими користувачами на обраний період.";
                return RedirectToAction("Create");
            }

            // Створюємо позику
            var loan = new Loan
            {
                UserID = UserID,
                UserName = user.Name,
                InventoryNum = copyToLoan.InventoryNum,
                Status = "Активна",
                StartDate = loanStart,
                EndDate = loanEnd
            };

            copyToLoan.Status = "Позичена";

            _context.Loans.Add(loan);
            _context.SaveChanges();

            TempData["Success"] = $"Позика для книги '{copyToLoan.Book.Title}' створена!";
            return RedirectToAction("Index");
        }

        // --- Повернення (GET) ---
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("return/{id}")]
        public IActionResult Return(int id)
        {
            var loan = _context.Loans
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Include(l => l.User)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null)
            {
                TempData["Error"] = "Бронювання не знайдене";
                return RedirectToAction("Index");
            }

            return View("~/Views/Loan/Return.cshtml", loan);
        }

        // --- Повернення (POST) ---
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("return/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult ReturnConfirmed(int id, DateTime returnDate, string condition)
        {
            var loan = _context.Loans
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Include(l => l.User)
                .FirstOrDefault(l => l.LoanID == id);

            if (loan == null)
            {
                TempData["Error"] = "Бронювання не знайдене.";
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

            // --- Створення платежу за штраф (одразу оплачений) ---
            if (fine > 0)
            {
                if (loan.UserID != null)
                {
                    var payment = new Payment
                    {
                        UserID = loan.UserID.Value,  // тепер безпечно
                        UserName = loan.User?.Name,
                        LoanID = loan.LoanID,
                        Amount = (decimal)fine,
                        Date = DateTime.Now,
                        Type = "Штраф",
                        Status = "Оплачено"
                    };

                    _context.Payments.Add(payment);
                    _context.SaveChanges();

                    TempData["Warning"] = $"Книга '{loan.Copy.Book.Title}' повернена із штрафом {fine} грн (оплачено).";
                }
                else
                {
                    TempData["Warning"] = $"Книга '{loan.Copy.Book.Title}' повернена із штрафом {fine} грн (користувача вже немає в системі, платіж не створено).";
                }
            }
            else
            {
                TempData["Success"] = $"Книга '{loan.Copy.Book.Title}' успішно повернена без штрафу.";
            }

            return RedirectToAction("Index");
        }


        // --- Видалення ---
        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var loan = _context.Loans.Include(l => l.Copy).FirstOrDefault(l => l.LoanID == id);
            if (loan == null) return NotFound();

            loan.Copy.Status = "Доступна";
            _context.Loans.Remove(loan);
            _context.SaveChanges();

            TempData["Success"] = "Бронювання успішно видалене!";
            return RedirectToAction("Index");
        }

        // --- Сторінка пропозиції створення членства ---
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("create-membership")]
        public IActionResult CreateMembershipPrompt()
        {
            var userId = TempData["UserIdWithoutMembership"] as int?;
            if (userId == null) return RedirectToAction("Index");

            var user = _context.Users.Find(userId.Value);
            if (user == null) return RedirectToAction("Create");

            ViewBag.User = user;
            ViewBag.Error = TempData["Error"];

            return View("~/Views/Membership/CreatePrompt.cshtml");
        }
    }
}
