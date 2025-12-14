using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Veritabaný kütüphanesi
using PsMarketApp.Data; // Veritabaný dosyamýzýn yeri
using PsMarketApp.Models;
using System.Diagnostics;

namespace PsMarketApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Veritabaný baðlantýsýný buraya çaðýrýyoruz
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanýndaki TÜM ürünleri listeye çekip sayfaya gönderiyoruz
            var urunler = await _context.Products.ToListAsync();
            return View(urunler);
        }

        public IActionResult Hakkimizda()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}