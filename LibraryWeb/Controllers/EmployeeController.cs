using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryWeb.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [Route("employee")]
    public class EmployeeController : Controller
    {
        

    }
}
