using Microsoft.EntityFrameworkCore; 
using PsMarketApp.Models;

namespace PsMarketApp.Data
{
    
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        //  Slider tablosu:
        public DbSet<Slider> Sliders { get; set; }
    }
}