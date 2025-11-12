using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
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
        public IActionResult Index(string? statusFilter, string search = "", string? inventoryFilter = "", string? userFilter = "", DateTime? dateFilter = null)
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
            ViewBag.StatusFilter = statusFilter ?? "Усі";

            ViewBag.Search = search;
            ViewBag.InventoryFilter = inventoryFilter;
            ViewBag.UserFilter = userFilter;
            ViewBag.DateFilter = dateFilter?.ToString("yyyy-MM-dd");

            var loans = _context.Loans
                .Include(l => l.User)
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Усі")
                loans = loans.Where(l => l.Status == statusFilter);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                loans = loans.Where(l =>
                    (l.User != null && (
                        l.User.Name.ToLower().Contains(search) ||
                        l.User.Login.ToLower().Contains(search)
                    )) ||
                    (l.UserName != null && l.UserName.ToLower().Contains(search)) ||
                    l.Copy.Book.Title.ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(inventoryFilter))
            {
                if (int.TryParse(inventoryFilter, out int inventoryNum))
                {
                    loans = loans.Where(l => l.InventoryNum == inventoryNum);
                }
            }

            if (!string.IsNullOrWhiteSpace(userFilter))
            {
                userFilter = userFilter.ToLower();
                loans = loans.Where(l =>
                    (l.User != null && (
                        l.User.Name.ToLower().Contains(userFilter) ||
                        l.User.Login.ToLower().Contains(userFilter)
                    )) ||
                    (l.UserName != null && l.UserName.ToLower().Contains(userFilter))
                );
            }

            if (dateFilter.HasValue)
            {
                loans = loans.Where(l => l.StartDate <= dateFilter && l.EndDate >= dateFilter);
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

            ViewBag.PrefilledUserLogin = TempData["PrefilledUserLogin"];
            ViewBag.PrefilledBookTitle = TempData["PrefilledBookTitle"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        // POST: /loans/create
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string UserLogin, string selectedBookTitle, DateTime EndDate)
        {
            var debug = new List<string>();
            
            var user = _context.Users
                .Include(u => u.Membership)
                .FirstOrDefault(u => u.Login == UserLogin);

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

            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.Title == selectedBookTitle);

            if (book == null)
            {
                TempData["Error"] = "Книга не знайдена.";
                return RedirectToAction("Create");
            }

            var loanStart = DateTime.Now.Date;
            var loanEnd = EndDate.Date;

            if (loanEnd <= loanStart)
            {
                TempData["Error"] = "Дата закінчення повинна бути пізніше за сьогодні.";
                return RedirectToAction("Create");
            }

            //debug.Add($"▶ Перевіряємо книгу '{book.Title}' для користувача '{user.Name}' ({user.Login})");
            //debug.Add($"Дати позики: {loanStart:yyyy-MM-dd} → {loanEnd:yyyy-MM-dd}");

            Copy? selectedCopy = null;
            Reservation? conflictingReservation = null;

            var copies = book.Copies.ToList();

            foreach (var copy in copies)
            {
                //debug.Add($"Перевіряємо копію InventoryNum={copy.InventoryNum}, статус='{copy.Status}'");

                // Беремо всі активні (оплачені) резервації на цю копію
                var reservations = _context.Reservations
                    .Include(r => r.User)
                    .Where(r => r.InventoryNum == copy.InventoryNum && r.Status == "Активна")
                    .ToList();

                bool hasConflict = false;
                Reservation? upcomingReservation = null;

                foreach (var res in reservations)
                {
                   // debug.Add($"🔹 Резервація ID={res.ReservationID}: {res.StartDate:yyyy-MM-dd} → {res.EndDate:yyyy-MM-dd}, користувач={res.User?.Name ?? "?"}");

                    // Перевірка перетину дат
                    if (res.StartDate <= loanEnd && res.EndDate >= loanStart)
                    {
                        hasConflict = true;
                        conflictingReservation = res;
                        //debug.Add($"❌ Конфлікт: резервація перетинається з датами позики");
                        break;
                    }

                    var daysUntilStart = (res.StartDate - DateTime.Now.Date).TotalDays;
                    if (daysUntilStart > 0 && daysUntilStart <= 10)
                    {
                        upcomingReservation = res;
                        //debug.Add($"⚠️ Резервація починається через {daysUntilStart:F0} днів ({res.StartDate:yyyy-MM-dd})");
                    }

                }

                // Якщо є конфлікт або копія не доступна
                if (hasConflict || copy.Status != "Доступна")
                {
                   // debug.Add($"❌ Копія {copy.InventoryNum} недоступна через статус або активну резервацію");
                    continue;
                }

                if (upcomingReservation != null)
                {
                    TempData["Debug"] = string.Join("<br>", debug);

                    return RedirectToAction("ConfirmLoanWarning", new
                    {
                        userLogin = UserLogin,
                        bookTitle = selectedBookTitle,
                        reservationUser = upcomingReservation.User?.Name ?? "інший користувач",
                        reservationStart = upcomingReservation.StartDate.ToString("yyyy-MM-dd"),
                        reservationEnd = upcomingReservation.EndDate.ToString("yyyy-MM-dd")
                    });
                }


                // Якщо немає конфлікту і копія доступна
                selectedCopy = copy;
                //debug.Add($"✅ Копія {copy.InventoryNum} вільна — вибрана для позики.");
                break;
            }

            // --- 4. Якщо немає вільних копій, але є конфліктна резервація ---
            // --- Якщо немає вільних копій, але є конфліктна резервація ---
            if (selectedCopy == null && conflictingReservation != null)
            {
                //TempData["Debug"] = string.Join("<br>", debug);

                return RedirectToAction("ConfirmLoanWarning", new
                {
                    userLogin = UserLogin,
                    bookTitle = selectedBookTitle,
                    reservationUser = conflictingReservation.User?.Name ?? "інший користувач",
                    reservationStart = conflictingReservation.StartDate.ToString("yyyy-MM-dd"),
                    reservationEnd = conflictingReservation.EndDate.ToString("yyyy-MM-dd")
                });
            }


            // --- 5. Якщо є вільна копія — створюємо позику ---
            if (selectedCopy != null)
            {
                var loan = new Loan
                {
                    UserID = user.UserID,
                    UserName = user.Name,
                    InventoryNum = selectedCopy.InventoryNum,
                    Status = "Активна",
                    StartDate = loanStart,
                    EndDate = loanEnd
                };

                selectedCopy.Status = "Позичена";

                _context.Loans.Add(loan);
                _context.SaveChanges();

                // Видаляємо резервацію користувача на цю книгу, якщо є
                var existingReservation = _context.Reservations
                    .FirstOrDefault(r => r.UserID == user.UserID &&
                                         r.Copy.BookID == book.BookID);

                if (existingReservation != null)
                {
                    _context.Reservations.Remove(existingReservation);
                    _context.SaveChanges();
                }

                TempData["Success"] = $"Позика для книги '{book.Title}' створена!";
                TempData["Debug"] = string.Join("<br>", debug);
                return RedirectToAction("Index");
            }

            // --- 6. Якщо нічого не підходить (резервацій і копій) ---
            TempData["Error"] = "Усі примірники цієї книги зараз позичені або зарезервовані.";
            TempData["Debug"] = string.Join("<br>", debug);
            return RedirectToAction("Create");
        }


        // --- підтвердження попередження ---
        [HttpGet("confirm-loan-warning")]
        public IActionResult ConfirmLoanWarning(
            string userLogin,
            string bookTitle,
            string reservationUser,
            string reservationStart,
            string reservationEnd)
        {
            ViewBag.UserLogin = userLogin;
            ViewBag.BookTitle = bookTitle;
            ViewBag.ReservationUser = reservationUser;
            ViewBag.ReservationStart = reservationStart;
            ViewBag.ReservationEnd = reservationEnd;

            return View("~/Views/Loan/ConfirmLoanWarning.cshtml");
        }


        [HttpPost("confirm-loan-warning")]
        public IActionResult ConfirmLoanWarningPost(string UserLogin, string BookTitle)
        {
            TempData["PrefilledUserLogin"] = UserLogin;
            TempData["PrefilledBookTitle"] = BookTitle;

            // повертаємо на сторінку створення позики
            return RedirectToAction("Create");
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
