using Microsoft.EntityFrameworkCore;
using PsMarketApp.Models;
using System.Collections.Generic;

namespace PsMarketApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}