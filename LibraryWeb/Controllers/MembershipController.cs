using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Route("membership")]
    public class MembershipController : Controller
    {
        private readonly LibraryContext _context;

        public MembershipController(LibraryContext context)
        {
            _context = context;
        }

        // --- Перегляд поточного членства користувача ---
        [HttpGet("")]
        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Не вдалося визначити користувача.";
                return RedirectToAction("Index", "Home");
            }

            int userId = int.Parse(userIdClaim.Value);

            var membership = _context.Memberships
               .Include(m => m.MembershipType)
               .Include(m => m.Payment)
               .OrderByDescending(m => m.MembershipID)
               .FirstOrDefault(m => m.UserID == userId);

            return View(membership);
        }

        // --- Створення членства (GET) ---
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.MembershipTypes = _context.MembershipTypes.ToList();
            return View();
        }

        // --- Створення членства (POST) ---
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int MembershipTypeID)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Не вдалося визначити користувача.";
                return RedirectToAction("Index", "Home");
            }

            int userId = int.Parse(userIdClaim.Value);

            if (_context.Memberships.Any(m => m.UserID == userId && m.Status == "Активне"))
            {
                TempData["Error"] = "У вас вже є активне членство.";
                return RedirectToAction("Index");
            }

            var type = await _context.MembershipTypes.FindAsync(MembershipTypeID);
            if (type == null)
            {
                TempData["Error"] = "Тип членства не знайдено.";
                return RedirectToAction("Create");
            }

            var membership = new Membership
            {
                UserID = userId,
                MembershipTypeID = type.MembershipTypeID,
                Type = type.Name,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(type.DurationMonths),
                Price = type.Price,
                Status = "Очікує оплату"
            };
            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();

            var payment = new Payment
            {
                UserID = userId,
                MembershipID = membership.MembershipID,
                Amount = type.Price,
                Date = DateTime.Now,
                Type = "Членство",
                Status = "Очікує оплату"
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            membership.Payment = payment;  // EF автоматично оновить PaymentID
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Членство '{type.Name}' створено. Очікує оплату!";
            return RedirectToAction("Index");
        }

        // --- Редагування членства (GET) ---
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var membership = _context.Memberships.Find(id);
            if (membership == null) return NotFound();

            return View(membership);
        }

        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            var membership = _context.Memberships
                .Include(m => m.User)
                .Include(m => m.Payment)
                .Include(m => m.MembershipType)
                .FirstOrDefault(m => m.MembershipID == id);

            if (membership == null) return NotFound();

            return View(membership);
        }

        // --- Видалення членства ---
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var membership = _context.Memberships.Find(id);
            if (membership == null) return NotFound();

            _context.Memberships.Remove(membership);
            _context.SaveChanges();

            TempData["Success"] = "Членство видалено.";
            return RedirectToAction("Index");
        }


        // GET: /membership/createforuser/{userId}
        [HttpGet("createforuser/{userId}")]
        public IActionResult CreateForUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return RedirectToAction("Index", "Loan");

            ViewBag.User = user;
            ViewBag.MembershipTypes = _context.MembershipTypes.ToList();

            return View();
        }

        // POST: /membership/createforuser/{userId}
        [HttpPost("createforuser/{userId}")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateForUser(int userId, int membershipTypeId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return RedirectToAction("Index", "Loan");

            // Перевірка на активне членство
            if (_context.Memberships.Any(m => m.UserID == userId && m.Status == "Активне"))
            {
                TempData["Error"] = "У користувача вже є активне членство.";
                return RedirectToAction("Index", "Loan");
            }

            var type = _context.MembershipTypes.Find(membershipTypeId);
            if (type == null)
            {
                TempData["Error"] = "Тип членства не знайдено.";
                return RedirectToAction("CreateForUser", new { userId });
            }

            // --- Створюємо членство одразу активним ---
            var membership = new Membership
            {
                UserID = userId,
                MembershipTypeID = type.MembershipTypeID,
                Type = type.Name,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(type.DurationMonths),
                Price = type.Price,
                Status = "Активне" // одразу активне
            };
            _context.Memberships.Add(membership);
            _context.SaveChanges(); // тут генерується MembershipID

            // --- Створюємо Payment одразу оплачений ---
            var payment = new Payment
            {
                UserID = userId,
                MembershipID = membership.MembershipID,
                Amount = type.Price,
                Date = DateTime.Now,
                Type = "Членство",
                Status = "Оплачено"
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();

            // --- Зв'язуємо Membership з Payment ---
            membership.Payment = payment;
            _context.Memberships.Update(membership);
            _context.SaveChanges();

            TempData["Success"] = $"Членство '{type.Name}' створено для користувача {user.Name} і одразу активоване.";
            return RedirectToAction("Index", "Loan");
        }

    }
}
