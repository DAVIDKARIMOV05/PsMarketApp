using PsMarketApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. VERİTABANI BAĞLANTISI (LINUX VE RENDER UYUMLU)
// ============================================================
var dbPath = Path.Combine(AppContext.BaseDirectory, "Market.db");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
// ============================================================
// ============================================================

builder.Services.AddControllersWithViews();

// Oturum Açma (Cookie) Ayarları
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login";
        config.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// ============================================================
// 2. OTOMATİK TABLO OLUŞTURMA (LOCAL İÇİN)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Veritabanı yoksa oluşturur, tabloları basar
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata oluştu.");
    }
}

// Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection(); // Güvenli bağlantı
app.UseStaticFiles();      // wwwroot klasörünü aç

app.UseRouting();

app.UseAuthentication(); // Giriş yapma
app.UseAuthorization();  // Yetki kontrolü

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Render port ayarını sildik, Visual Studio kendi ayarlarını kullanacak.
Console.WriteLine("DEBUG - Sitenin okuduğu klasör: " + AppContext.BaseDirectory);
app.Run();