using PsMarketApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql; // PostgreSQL için eklendi

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
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

// 🚀 POSTGRESQL AYARI (KALICI ÇÖZÜM)
// Bu kod, Connection String'i Render'ın Environment Variables'ından (Ortam Değişkenleri) çeker.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Render'da kullanılan PostgreSQL URL'i, Entity Framework'ün beklediği formata çevrilir.
// Eğer PostgreSQL bağlantı adresi "postgres://" ile başlıyorsa (Render'ın verdiği format), bu çevrim gereklidir.
var databaseUrl = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgres://"))
{
    var uri = new Uri(databaseUrl);
    connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Database = uri.AbsolutePath.Trim('/'),
        Username = uri.UserInfo.Split(':')[0],
        Password = uri.UserInfo.Split(':')[1],
        SslMode = SslMode.Prefer,
        TrustServerCertificate = true // Güvenli bağlantı ayarı
    }.ToString();
}

// 1. VERİTABANI AYARI (PostgreSQL ile Güncellendi)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // UseNpgsql kullanıldı

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
        // Uygulama başlarken veritabanını PostgreSQL'de oluşturur/günceller
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "PostgreSQL veritabanı oluşturulurken bir hata çıktı.");
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

// Uygulamanın çalışacağı Port'u belirle (Bulut sisteminden gelen PORT veya varsayılan 8080)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// Uygulamayı tüm ağlara (0.0.0.0) açarak başlat
app.Run($"http://0.0.0.0:{port}");