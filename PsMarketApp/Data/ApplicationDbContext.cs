using Microsoft.EntityFrameworkCore; // Sadece bu lazım, Identity değil.
using PsMarketApp.Models;

namespace PsMarketApp.Data
{
    // Burası IdentityDbContext DEĞİL, sadece DbContext olmalı.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        // Yeni eklediğimiz Slider tablosu:
        public DbSet<Slider> Sliders { get; set; }
    }
}