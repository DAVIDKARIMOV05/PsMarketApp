using PsMarketApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// 1. AYARLARI YÜKLE
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// 2. VERİTABANI BAĞLANTISI (RENDER & LOCAL UYUMLU)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Eğer Render'dan bir URL geliyorsa, onu Npgsql formatına çevir
if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgres://"))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var builderDb = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = userInfo[0],
            Password = userInfo[1],
            SslMode = SslMode.Prefer,
            TrustServerCertificate = true,
            Pooling = true // Performans için
        };
        connectionString = builderDb.ToString();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"HATA: Render URL'i çevrilemedi: {ex.Message}");
    }
}

// PostgreSQL Servisini Ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. DİĞER SERVİSLER
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login";
    });

var app = builder.Build();

// 4. OTOMATİK TABLO OLUŞTURMA (MIGRATION)
// Bu kısım veritabanı boşsa tabloları oluşturur.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("Veritabanı tabloları kontrol ediliyor...");
        context.Database.Migrate();
        Console.WriteLine("Veritabanı tabloları başarıyla güncellendi/oluşturuldu.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        // Hatayı hem konsola hem loga basar
        Console.WriteLine($"KRİTİK HATA: Veritabanı oluşturulamadı: {ex.Message}");
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata çıktı.");
    }
}

// 5. HATA YÖNETİMİ VE HTTPS AYARLARI
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Render zaten HTTPS yaptığı için bu satırı KAPATIYORUZ (Hata sebebini engellemek için)
    // app.UseHsts(); 
}

// Render'da sonsuz döngüye girmemesi için bunu da KAPATIYORUZ
// app.UseHttpsRedirection(); 

// Statik dosyalar (css, js, img)
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

// PORT AYARI (Render için zorunlu)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");