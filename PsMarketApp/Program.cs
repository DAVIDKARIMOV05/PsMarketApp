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
// 🛠️ BAĞLANTIYI OLUŞTURMA (RENDER URL DÜZELTİCİ - FİNAL)
// ============================================================
string connectionString = "";

try
{
    var renderUrl = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    if (string.IsNullOrEmpty(renderUrl))
    {
        renderUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    }

    // DÜZELTME: Hem "postgres://" hem de "postgresql://" kabul ediliyor
    if (!string.IsNullOrEmpty(renderUrl) && (renderUrl.StartsWith("postgres://") || renderUrl.StartsWith("postgresql://")))
    {
        Console.WriteLine("--> Render URL'i algılandı, dönüştürülüyor...");

        var databaseUri = new Uri(renderUrl);
        var userInfo = databaseUri.UserInfo.Split(new[] { ':' }, 2);

        var builderDb = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Prefer,
            TrustServerCertificate = true
        };

        connectionString = builderDb.ToString();
    }
    else
    {
        // Render URL'i yoksa, yerel ayarlar
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"KRİTİK HATA: URL Çevrilemedi! Hata: {ex.Message}");
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