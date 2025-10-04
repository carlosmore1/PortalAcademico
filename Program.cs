using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// 🔹 CONFIGURACIÓN DE SERVICIOS
// -----------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=app.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity + Roles (login sin confirmación de correo para el examen)
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// 🔹 Redis (cache distribuido + sesión)
//    Soporta variable de entorno (Redis__ConnectionString) o appsettings.json (Redis:ConnectionString)
var redisConn =
    builder.Configuration["Redis__ConnectionString"] ??
    builder.Configuration.GetSection("Redis")["ConnectionString"] ??
    "localhost:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
});

// Sesión (respaldada por el cache distribuido)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// -----------------------------
// 🔹 CONFIGURACIÓN DEL PIPELINE
// -----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 🔹 Habilitar sesión (obligatorio para leer/escribir HttpContext.Session)
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// -----------------------------
// 🔹 SEED DE DATOS INICIALES
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    await SeedData.RunAsync(scope.ServiceProvider);
}

app.Run();
