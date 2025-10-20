using LibraryWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Controllers
{
    [Route("user")]
    public class ProfileController : Controller
    {
        private readonly LibraryContext _context;

        public ProfileController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Index", "Login");

            var user = _context.Users
                .Include(u => u.Membership)
                .Include(u => u.Loans)
                .Include(u => u.Payments)
                .FirstOrDefault(u => u.UserID == userId);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Success = TempData["Success"] as string;

            return View("~/Views/User/Profile.cshtml", user);
        }
    }
}
