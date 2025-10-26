using LibraryWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace LibraryWeb.ViewComponents
{
    public class MembershipListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<MembershipType> types, User user)
        {
            var model = Tuple.Create(types, user);
            return View(model); // підключає Default.cshtml
        }
    }
}
