using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Route("my-loans")]
    public class ReaderLoanController : Controller
    {
        private readonly LibraryContext _context;

        public ReaderLoanController(LibraryContext context)
        {
            _context = context;
        }

        // --- Список позик читача ---
        [HttpGet("")]
        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                TempData["Error"] = "Unable to identify the user.";
                return RedirectToAction("Index", "Home");
            }

            int userId = int.Parse(userIdClaim.Value);

            var loans = _context.Loans
                .Where(l => l.UserID == userId)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Book)
                .OrderByDescending(l => l.StartDate)
                .ToList();

            return View("~/Views/Loan/ReaderLoans.cshtml", loans);
        }
    }
}
