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
        public IActionResult Create(Membership model)
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

            var type = _context.MembershipTypes.Find(model.MembershipTypeID);
            if (type == null)
            {
                TempData["Error"] = "Обраний тип членства не знайдено.";
                return RedirectToAction("Create");
            }

            model.UserID = userId;
            model.StartDate = DateTime.Now;
            model.EndDate = DateTime.Now.AddMonths(type.DurationMonths);
            model.Price = type.Price;
            model.Status = "Активне";

            _context.Memberships.Add(model);
            _context.SaveChanges();

            TempData["Success"] = $"Членство '{type.Name}' створено успішно!";
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

        // --- Редагування членства (POST) ---
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Membership model)
        {
            var membership = _context.Memberships.Find(id);
            if (membership == null) return NotFound();

            membership.Type = model.Type;
            membership.Price = model.Price;
            membership.Status = model.Status;
            membership.EndDate = model.EndDate;

            _context.SaveChanges();

            TempData["Success"] = "Членство оновлено.";
            return RedirectToAction("Index");
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

            var membership = new Membership
            {
                UserID = userId,
                MembershipTypeID = type.MembershipTypeID,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(type.DurationMonths),
                Price = type.Price,
                Status = "Активне"
            };

            _context.Memberships.Add(membership);
            _context.SaveChanges();

            TempData["Success"] = $"Членство '{type.Name}' створено для користувача {user.Name}!";
            return RedirectToAction("Create", "Loan");
        }
    }
}
