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
//bu site RENDER DA YAYINLANDI
//SİTE FOTOĞRAFLARI CLOUDİNARY BULUTA KAYDEDİLİYOR
//BU SİTE UPTİMEROBOT TARAFINDAN 5 DAKİKADA BİR DÜRTÜLÜYOR
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// 1. AYARLARI YÜKLE
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// ============================================================
// 🛠️ BAĞLANTIYI OLUŞTURMA (HEM LOCAL HEM RENDER UYUMLU)
// ============================================================
string connectionString = "";

try
{
    // Adım 1: Önce Render Environment'a bak
    var rawConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    // Adım 2: Render boşsa, appsettings.json'a bak (Local)
    if (string.IsNullOrEmpty(rawConnectionString))
    {
        rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }

    //  Dönüştürme işlemi 
    if (!string.IsNullOrEmpty(rawConnectionString))
    {
        // Eğer adres "postgres://" veya "postgresql://" ile başlıyorsa parçala ve düzelt
        if (rawConnectionString.StartsWith("postgres://") || rawConnectionString.StartsWith("postgresql://"))
        {
            Console.WriteLine("--> Veritabanı URL formatı algılandı, dönüştürülüyor...");

            var databaseUri = new Uri(rawConnectionString);
            var userInfo = databaseUri.UserInfo.Split(new[] { ':' }, 2);

            var builderDb = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true // Localde SSL hatası almamak için 
            };
            connectionString = builderDb.ToString();
            Console.WriteLine("--> Bağlantı başarıyla dönüştürüldü.");
        }
        else
        {
            // Eğer zaten düzgün formatta geldiyse (Host=... gibi) olduğu gibi kullan
            connectionString = rawConnectionString;
        }
    }
    else
    {
        throw new Exception("Bağlantı adresi (Connection String) ne Render'da ne de appsettings.json'da bulunamadı!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"KRİTİK HATA: Bağlantı adresi işlenemedi! Hata: {ex.Message}");
    // Hatanın devam etmesine izin veriyoruz ki uygulama dursun ve logu görelim
    throw;
}

// 3. Veritabanı Servisini Ekle
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

// OTOMATİK MIGRATION
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
        logger.LogError(ex, "Veritabanı oluşturulurken hata çıktı.");
    }
}

// Hata Sayfası Ayarları
app.UseDeveloperExceptionPage();

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