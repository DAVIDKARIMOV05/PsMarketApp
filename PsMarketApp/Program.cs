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

// 1. AYARLARI TEMİZLE VE YÜKLE
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// ============================================================
// 🛠️ BAĞLANTIYI BULMA VE OLUŞTURMA (GÜÇLENDİRİLMİŞ)
// ============================================================
string connectionString = null;

// Adım 1: Render Environment Variable Kontrolü (Senin eklediğin)
var envVar = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Adım 2: Eğer o yoksa, Render'ın bazen otomatik verdiği isme bak
if (string.IsNullOrEmpty(envVar))
{
    envVar = Environment.GetEnvironmentVariable("DATABASE_URL");
}

// Adım 3: Eğer hala yoksa, appsettings.json'dan (Local) okumayı dene
if (string.IsNullOrEmpty(envVar))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
else
{
    // Render'dan gelen URL'i Npgsql formatına çevir
    try
    {
        // "postgres://" ile başlıyorsa çevirme işlemi yap
        if (envVar.StartsWith("postgres://"))
        {
            var databaseUri = new Uri(envVar);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builderDb = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true // Sertifika hatasını engelle
            };
            connectionString = builderDb.ToString();
        }
        else
        {
            // postgres:// ile başlamıyorsa direkt kullan
            connectionString = envVar;
        }
    }
    catch (Exception ex)
    {
        // Hata varsa uygulama başlarken patlasın ki loglarda görelim
        throw new Exception($"KRİTİK HATA: Render URL'i çevrilemedi! Gelen Veri: {envVar} - Hata: {ex.Message}");
    }
}

// SON KONTROL: Eğer connectionString hala boşsa hata ver
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("KRİTİK HATA: Bağlantı adresi (Connection String) BULUNAMADI! Render Environment Variable'larını kontrol et.");
}

// Veritabanı Servisini Ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================================

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login";
    });

var app = builder.Build();

// OTOMATİK TABLO OLUŞTURMA (MIGRATE)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Tabloları oluştur
    }
    catch (Exception ex)
    {
        // Eğer veritabanı bağlantısı yanlışsa burada hata verir
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı Migrate işlemi başarısız oldu.");
    }
}

// Hata Yönetimi (Detayları görmek için Developer Page'i zorluyoruz)
app.UseDeveloperExceptionPage();

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

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");