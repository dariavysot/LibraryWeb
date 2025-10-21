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
                .FirstOrDefault(m => m.UserID == userId);

            return View(membership);
        }

        // --- Створення членства (GET) ---
        [HttpGet("create")]
        public IActionResult Create()
        {
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

            var existingMembership = _context.Memberships.FirstOrDefault(m => m.UserID == userId);
            if (existingMembership != null)
            {
                TempData["Error"] = "У вас вже є активне членство.";
                return RedirectToAction("Index");
            }

            // Розрахунок дат
            model.UserID = userId;
            model.StartDate = DateTime.Now;
            model.EndDate = DateTime.Now.AddMonths(1);
            model.Status = "Активне";

            // Ціна за типом
            model.Price = model.Type switch
            {
                "Premium" => 200,
                "Standard" => 100,
                _ => 50
            };

            _context.Memberships.Add(model);
            _context.SaveChanges();

            TempData["Success"] = $"Членство '{model.Type}' створено успішно!";
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
    }
}
