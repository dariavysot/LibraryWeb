using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.ViewComponents
{
    public class LoanListViewComponent : ViewComponent
    {
        private readonly LibraryContext _context;

        public LoanListViewComponent(LibraryContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke(string statusFilter)
        {
            var loans = _context.Loans
                .Include(l => l.User)
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .AsQueryable();

            loans = statusFilter switch
            {
                "Active" => loans.Where(l => l.Status == "Активна"),
                "Overdue" => loans.Where(l => l.Status == "Прострочена"),
                "Returned" => loans.Where(l => l.Status == "Повернено"),
                _ => loans
            };

            return View(loans.ToList());
        }
    }
}
