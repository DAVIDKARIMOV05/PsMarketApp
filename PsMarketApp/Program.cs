using PsMarketApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// 1. GÜVENLÝK SERVÝSÝNÝ EKLE
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login"; // Giriþ yapmayan buraya gitsin
    });

// 1. VERÝTABANI AYARI (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=Market.db"));

// 2. Servisleri ekle
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. OTOMATÝK VERÝTABANI OLUÞTURUCU (Hata Çözücü Kýsým)
// Bu kýsým, site açýlýrken tablolarýn oluþmasýný garanti eder.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Veritabanýný oluþturur veya günceller
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabaný oluþturulurken bir hata çýktý.");
    }
}

// 4. Diðer Ayarlar
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Avif resim desteði (Senin kodundan)
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();