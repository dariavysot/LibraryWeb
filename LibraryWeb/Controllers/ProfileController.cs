using LibraryWeb.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Authorize]
    [Route("user")]
    public class ProfileController : Controller
    {
        private readonly LibraryContext _context;

        public ProfileController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Index()
        {
            // Отримуємо ID користувача з claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Якщо користувач неавторизований — відправляємо на сторінку логіну
                return RedirectToAction("Index", "Login");
            }

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users
                .Include(u => u.Membership)
                .Include(u => u.Loans)
                .Include(u => u.Payments)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                // Якщо користувача не знайдено — знищуємо сесію
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Success = TempData["Success"] as string;

            return View("~/Views/User/Profile.cshtml", user);
        }
    }
}
