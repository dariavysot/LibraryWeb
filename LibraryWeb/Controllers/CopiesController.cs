using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Controllers
{
    [Route("copies")]
    public class CopiesController : Controller
    {
        private readonly LibraryContext _context;

        public CopiesController(LibraryContext context)
        {
            _context = context;
        }

        // Список примірників для конкретної книги
        [HttpGet("book/{bookId}")]
        public IActionResult Index(int bookId)
        {
            var book = _context.Books
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.BookID == bookId);

            if (book == null)
                return NotFound();

            ViewBag.BookTitle = book.Title;
            ViewBag.BookId = bookId;

            return View("~/Views/Copies/Index.cshtml", book.Copies);
        }

        // Видалення окремого примірника
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var copy = _context.Copies.Include(c => c.Book).FirstOrDefault(c => c.InventoryNum == id);
            if (copy == null)
                return NotFound();

            int bookId = copy.BookID;

            _context.Copies.Remove(copy);
            _context.SaveChanges();

            // Перевіряємо, чи залишилися ще копії книги
            bool hasCopies = _context.Copies.Any(c => c.BookID == bookId);
            if (!hasCopies)
            {
                var book = _context.Books.FirstOrDefault(b => b.BookID == bookId);
                if (book != null)
                {
                    _context.Books.Remove(book);
                    _context.SaveChanges();
                    TempData["Success"] = "Останній примірник видалено. Книга теж була видалена.";
                    return RedirectToAction("Index", "Book");
                }
            }

            TempData["Success"] = "Примірник успішно видалено!";
            return RedirectToAction("Index", new { bookId });
        }

    }
}
