using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 
using System.IO;

namespace LibraryWeb.Controllers
{
    [Route("books")]
    public class BookController : Controller
    {
        private readonly LibraryContext _context;

        public BookController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index(string? search, string? genre, int? year)
        {
            var books = _context.Books
                .Include(b => b.Copies)
                .AsQueryable();

            // --- Пошук ---
            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search));
            }

            // --- Фільтр за жанром ---
            if (!string.IsNullOrWhiteSpace(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }

            // --- Фільтр за роком ---
            if (year != null)
            {
                books = books.Where(b => b.PublicationYear == year);
            }

            // --- Дані для фільтрів ---
            ViewBag.Search = search;
            ViewBag.Genre = genre;
            ViewBag.Year = year;
            ViewBag.Genres = _context.Books
                .Select(b => b.Genre)
                .Where(g => g != null && g != "")
                .Distinct()
                .OrderBy(g => g)
                .ToList();
            ViewBag.Years = _context.Books
                .Select(b => b.PublicationYear)
                .Where(y => y != null)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            return View("~/Views/Book/Index.cshtml", books.ToList());
        }

        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.BookID == id);

            if (book == null)
                return NotFound();

            ViewBag.CopyCount = book.Copies.Count;
            return View("~/Views/Book/Details.cshtml", book);
        }

        // --- Додавання книги ---
        [Authorize(Roles = "Admin")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            var model = new Book();
            return View("~/Views/Book/Create.cshtml");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book model, int copyCount, IFormFile? CoverImage)
        {
            // Серверна валідація
            if (model.PublicationYear != null && model.PublicationYear > DateTime.Now.Year)
                ModelState.AddModelError("PublicationYear", "Рік видання не може бути більшим за поточний.");

            if (copyCount < 1)
                ViewBag.CopyCountError = "Кількість примірників має бути не менше 1.";

            if (!ModelState.IsValid || copyCount < 1)
                return View("~/Views/Book/Create.cshtml", model);

            // --- Збереження обкладинки ---
            if (CoverImage != null && CoverImage.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/covers");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(CoverImage.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    CoverImage.CopyTo(stream);
                }

                model.CoverImagePath = $"/images/covers/{fileName}";
            }

            model.DateAdded = DateTime.Now;
            _context.Books.Add(model);
            _context.SaveChanges();

            // Додаємо копії
            for (int i = 0; i < copyCount; i++)
            {
                _context.Copies.Add(new Copy
                {
                    BookID = model.BookID,
                    Condition = "Нова",
                    Status = "Доступна",
                });
            }

            _context.SaveChanges();
            TempData["Success"] = $"Книгу '{model.Title}' додано ({copyCount} примірників).";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.BookID == id);

            if (book == null)
                return NotFound();

            ViewBag.CopyCount = book.Copies.Count;
            return View("~/Views/Book/Edit.cshtml", book);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Book model, int copyCount, IFormFile? CoverImage)
        {
            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.BookID == id);

            if (book == null)
                return NotFound();

            if (model.PublicationYear != null && model.PublicationYear > DateTime.Now.Year)
                ModelState.AddModelError("PublicationYear", "Рік видання не може бути більшим за поточний.");

            if (copyCount < book.Copies.Count) // не дозволяємо зменшувати
                ModelState.AddModelError("CopyCount", "Не можна зменшити кількість копій, які вже позичені.");

            if (!ModelState.IsValid)
                return View("~/Views/Book/Edit.cshtml", book);

            // --- Збереження нової обкладинки ---
            if (CoverImage != null && CoverImage.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/covers");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(CoverImage.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    CoverImage.CopyTo(stream);
                }

                book.CoverImagePath = $"/images/covers/{fileName}";
            }

            // --- Оновлення полів книги ---
            book.Title = model.Title;
            book.Author = model.Author;
            book.Type = model.Type;
            book.ISBN = model.ISBN;
            book.Language = model.Language;
            book.PublishingHouse = model.PublishingHouse;
            book.PublicationYear = model.PublicationYear;
            book.Genre = model.Genre;
            book.Description = model.Description;

            // --- Додавання нових копій, якщо потрібно ---
            int currentCount = book.Copies.Count;
            if (copyCount > currentCount)
            {
                for (int i = 0; i < copyCount - currentCount; i++)
                {
                    _context.Copies.Add(new Copy { BookID = book.BookID, Condition = "Нова", Status = "Доступна" });
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Книгу успішно оновлено!";
            return RedirectToAction("Index");
        }
    }
}
