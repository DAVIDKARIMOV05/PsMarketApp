using PsMarketApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration; // Bu eklediğimiz yeni kütüphane

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
    // SuppressStatusMessages satırı kaldırıldı, çünkü hata veriyordu.
});

// 🛠️ KRİTİK DÜZELTME: Linux inotify (Status 139) Hatası İçin
// Varsayılan yapılandırma izleyicilerini temizleyip, "izlemesiz" (reloadOnChange: false) olarak yeniden ekliyoruz.
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// 1. GÜVENLİK SERVİSİNİ EKLE
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login";
    });

// 1. VERİTABANI AYARI (SQLite)
// Canlıya atarken Market.db dosyasının kopyalanmadığından emin olun (Copy to Output: Do not copy)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=Market.db"));

// 2. Servisleri ekle
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. OTOMATİK VERİTABANI OLUŞTURUCU
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata çıktı.");
    }
}

// 4. Diğer Ayarlar
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Avif resim desteği
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();
app.UseStaticFiles(); // wwwroot erişimi için
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();