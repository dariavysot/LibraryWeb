using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Index()
        {
            var books = _context.Books.Include(b => b.Copies).ToList();
            return View("~/Views/Book/Index.cshtml", books);
        }

        [HttpGet("details/{id}")]
        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.BookID == id);

            if (book == null)
                return NotFound();

            return View("~/Views/Book/Details.cshtml", book);
        }

        // --- Додавання книги ---
        [Authorize(Roles = "Admin")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Book/Create.cshtml");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book model, int copyCount)
        {
            // Серверна валідація
            if (model.PublicationYear != null && model.PublicationYear > DateTime.Now.Year)
            {
                ModelState.AddModelError("PublicationYear", "Рік видання не може бути більшим за поточний.");
            }

            if (copyCount < 1)
            {
                ViewBag.CopyCountError = "Кількість примірників має бути не менше 1.";
            }

            if (!ModelState.IsValid || copyCount < 1)
                return View("~/Views/Book/Create.cshtml", model);


            model.DateAdded = DateTime.Now;
            _context.Books.Add(model);
            _context.SaveChanges();

            // Додаємо копії
            for (int i = 0; i < copyCount; i++)
            {
                var copy = new Copy
                {
                    BookID = model.BookID,
                    Condition = "Нова",
                    Status = "Доступна",
                };
                _context.Copies.Add(copy);
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
        public IActionResult Edit(int id, Book model, int copyCount)
        {
            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.BookID == id);

            if (book == null)
                return NotFound();

            if (model.PublicationYear != null && model.PublicationYear > DateTime.Now.Year)
            {
                ModelState.AddModelError("PublicationYear", "Рік видання не може бути більшим за поточний.");
            }

            if (copyCount < 1)
            {
                ViewBag.CopyCountError = "Кількість примірників має бути не менше 1.";
            }

            if (!ModelState.IsValid || copyCount < 1)
                return View("~/Views/Book/Edit.cshtml", model);

            // Оновлюємо поля книги
            book.Title = model.Title;
            book.Author = model.Author;
            book.Type = model.Type;
            book.ISBN = model.ISBN;
            book.Language = model.Language;
            book.PublishingHouse = model.PublishingHouse;
            book.PublicationYear = model.PublicationYear;
            book.Genre = model.Genre;

            // Кількість примірників
            int currentCount = book.Copies.Count;

            if (copyCount > currentCount)
            {
                // Додаємо нові копії
                for (int i = 0; i < copyCount - currentCount; i++)
                {
                    _context.Copies.Add(new Copy
                    {
                        BookID = book.BookID,
                        Condition = "Нова",
                        Status = "Доступна"
                    });
                }
            }
            else if (copyCount < currentCount)
            {
                // Видаляємо зайві (тільки доступні)
                var removableCopies = book.Copies
                    .Where(c => c.Status == "Доступна")
                    .Take(currentCount - copyCount)
                    .ToList();

                if (removableCopies.Count < (currentCount - copyCount))
                {
                    ModelState.AddModelError("", "Не можна зменшити кількість, бо деякі примірники позичені.");
                    ViewBag.CopyCount = book.Copies.Count;
                    return View("~/Views/Book/Edit.cshtml", book);
                }

                _context.Copies.RemoveRange(removableCopies);
            }

            _context.SaveChanges();

            TempData["Success"] = "Книгу успішно оновлено!";
            return RedirectToAction("Index");
        }
    }
}
