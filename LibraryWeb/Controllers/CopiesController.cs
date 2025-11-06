using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin,Employee")]
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
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // Знаходимо примірник за InventoryNum
            var copy = _context.Copies.Include(c => c.Book).FirstOrDefault(c => c.InventoryNum == id);
            if (copy == null)
            {
                TempData["Error"] = "Примірник не знайдено.";
                return RedirectToAction("Index");
            }

            // Перевірка статусу
            if (copy.Status == "Позичена")
            {
                TempData["Error"] = "Неможливо видалити примірник, бо він зараз позичений.";
                return RedirectToAction("Index", new { bookId = copy.BookID });
            }

            try
            {
                int bookId = copy.BookID;

                // Видаляємо примірник
                _context.Copies.Remove(copy);
                _context.SaveChanges();

                // Перевіряємо, чи залишилися копії книги
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
            catch (DbUpdateException ex)
            {
                // Ловимо помилки FK
                Console.WriteLine(ex.Message); // або використати ILogger
                TempData["Error"] = "Неможливо видалити примірник через наявність позик.";
                return RedirectToAction("Index", new { bookId = copy.BookID });
            }
        }


    }
}
