using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Controllers
{
    [Route("membership-types")]
    public class MembershipTypeController : Controller
    {
        private readonly LibraryContext _context;

        public MembershipTypeController(LibraryContext context)
        {
            _context = context;
        }

        // --- Список типів ---
        [HttpGet("")]
        public IActionResult Index()
        {
            var types = _context.MembershipTypes.ToList();
            return View(types);
        }

        // --- Create GET ---
        [Authorize(Roles = "Admin")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("Create", new MembershipType());
        }

        // --- Create POST ---
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MembershipType model)
        {
            if (!ModelState.IsValid) return View("Upsert", model);

            _context.MembershipTypes.Add(model);
            _context.SaveChanges();
            TempData["Success"] = $"Тип членства '{model.Name}' створено!";
            return RedirectToAction("Index");
        }

        // --- Edit GET ---
        [Authorize(Roles = "Admin")]
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var type = _context.MembershipTypes.Find(id);
            if (type == null) return NotFound();
            return View("Edit", type);
        }

        // --- Edit POST ---
        [Authorize(Roles = "Admin")]
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, MembershipType model)
        {
            if (!ModelState.IsValid) return View("Edit", model);

            var existing = _context.MembershipTypes.Find(id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Price = model.Price;
            existing.DurationMonths = model.DurationMonths;

            _context.SaveChanges();
            TempData["Success"] = $"Тип членства '{model.Name}' оновлено!";
            return RedirectToAction("Index");
        }

        // --- Delete ---
        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var type = _context.MembershipTypes.Find(id);
            if (type == null) return NotFound();

            bool inUse = _context.Memberships.Any(m => m.MembershipTypeID == id);
            if (inUse)
            {
                TempData["Error"] = $"Тип членства '{type.Name}' не може бути видалений, оскільки він використовується у поточних членствах.";
                return RedirectToAction("Index");
            }

            _context.MembershipTypes.Remove(type);
            _context.SaveChanges();

            TempData["Success"] = $"Тип членства '{type.Name}' видалено!";
            return RedirectToAction("Index");
        }
    }
}
