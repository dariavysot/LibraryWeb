using LibraryWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;

        public AdminController(LibraryContext context)
        {
            _context = context;
        }

        // --- Головна сторінка адміна ---
        [HttpGet("")]
        public IActionResult Dashboard()
        {
            // Можемо передати статистику для відображення на панелі
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.BookCount = _context.Books.Count();
            ViewBag.MembershipCount = _context.MembershipTypes.Count();

            return View("~/Views/Admin/Dashboard.cshtml");
        }
    }
}
