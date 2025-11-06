using LibraryWeb;
using LibraryWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Підключення бази даних
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC
builder.Services.AddControllersWithViews();

// Додаємо підтримку cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/user/login";           // якщо користувач не авторизований
        options.AccessDeniedPath = "/user/access-denied"; // якщо доступ заборонений
        options.ExpireTimeSpan = TimeSpan.FromHours(1);  // час життя cookie
        options.SlidingExpiration = true;                // автоматичне продовження сесії
    });

var app = builder.Build();

// Налаштування пайплайну
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  Додаємо автентифікацію та авторизацію
app.UseAuthentication(); // повинно бути перед UseAuthorization
app.UseAuthorization();

// Маршрути
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
