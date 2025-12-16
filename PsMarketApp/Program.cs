using PsMarketApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// 1. AYARLARI YÜKLE
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// ============================================================
// 🛠️ DATABASE BAĞLANTISI AYARLAMA (RENDER İÇİN ÖZEL)
// ============================================================
var connectionString = "";

// 1. Önce Render'dan gelen "ConnectionStrings__DefaultConnection" değişkenine bak.
var renderDbUrl = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (!string.IsNullOrEmpty(renderDbUrl))
{
    // Render URL'i "postgres://" ile başlıyorsa onu parçalayalım
    try
    {
        var databaseUri = new Uri(renderDbUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        var builderDb = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Prefer, // Render SSL gerektirir
            TrustServerCertificate = true // Sertifika hatası vermemesi için
        };
        connectionString = builderDb.ToString();
        Console.WriteLine("--> Render PostgreSQL bağlantısı başarıyla oluşturuldu.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Render URL dönüştürme hatası: {ex.Message}");
    }
}
else
{
    // Render değişkeni yoksa (Localde çalışıyorsak) appsettings.json'a bak
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// Son kontrol: Eğer connectionString hala boşsa veya null ise varsayılan bir değer ata (uygulama çökmesin diye)
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("--> UYARI: Bağlantı dizesi bulunamadı!");
}

// Veritabanı Servisini Ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================================

// Diğer Servisler
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login";
    });

var app = builder.Build();

// OTOMATİK MIGRATION (Tabloları Oluşturma)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Veritabanını oluşturur
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Veritabanı Migrate Hatası: {ex.Message}");
    }
}

// Hata Yönetimi
if (!app.Environment.IsDevelopment())
{
    // Hata detaylarını görmek için Development modunda olmasak bile hatayı açabiliriz (geçici olarak)
    app.UseDeveloperExceptionPage();
    // app.UseExceptionHandler("/Home/Error"); 
}
else
{
    app.UseDeveloperExceptionPage();
}

// Statik Dosyalar
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

// Port Ayarı
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");