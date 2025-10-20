using LibraryWeb.Data;
using LibraryWeb.Models;
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
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Book/Create.cshtml");
        }

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
                    ReturnDate = null
                };
                _context.Copies.Add(copy);
            }

            _context.SaveChanges();

            TempData["Success"] = $"Книгу '{model.Title}' додано ({copyCount} примірників).";
            return RedirectToAction("Index");
        }
    }
}
