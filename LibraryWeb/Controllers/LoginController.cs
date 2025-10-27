using LibraryWeb.Data;
using LibraryWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace LibraryWeb.Controllers
{
    [Route("user")]
    public class LoginController : Controller
    {
        private readonly LibraryContext _context;

        public LoginController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("login")]
        public IActionResult Index()
        {
            return View("~/Views/User/Login.cshtml");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Index(string login, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            if (user == null)
            {
                ViewBag.Error = "Невірний логін або пароль.";
                return View("~/Views/User/Login.cshtml");
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.UserPassword, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Невірний логін або пароль.";
                return View("~/Views/User/Login.cshtml");
            }

            // Створюємо Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                });

            return RedirectToAction("Index", "Profile");
        }
        
        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View("~/Views/User/AccessDenied.cshtml");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }       
    }
}
