using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsMarketApp.Data;
using PsMarketApp.Models;
using System.Diagnostics;

namespace PsMarketApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GÜNCEL Index Metodu: Arama Terimi VE Filtre parametresini alýyor.
        public async Task<IActionResult> Index(string SearchTerm, string Filter)
        {
            // Veritabanýndan tüm ürünleri ve tüm sliderlarý çekiyoruz.
            var allProducts = await _context.Products.Include(p => p.Slider).ToListAsync();

            // Menü ve carousel için tüm slider listesini tutuyoruz.
            var allSliders = await _context.Sliders.Include(s => s.Products).ToListAsync();

            var filteredProducts = allProducts.AsEnumerable(); // Filtrelemeye hazýr ürün listesi
            var slidersForViewModel = allSliders; // ViewModel'e gidecek slider listesi (Varsayýlan: Tümü)

            // ===================================
            // 1. ARAMA (SearchTerm) KONTROLÜ
            // ===================================
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string term = SearchTerm.ToLower().Trim();

                // Ürün listesini ara
                filteredProducts = filteredProducts.Where(p => p.Name.ToLower().Contains(term));

                // Arama aktifken dikey menüdeki dinamik filtreler kalsýn, ama carouseller gizlensin
                slidersForViewModel = new List<Slider>(); // Sadece ürün carouselleri gizlenir

                ViewData["SearchActive"] = true;
                ViewData["SearchTerm"] = SearchTerm;
            }

            // ===================================
            // 2. FÝLTRE KONTROLÜ
            // ===================================
            else if (!string.IsNullOrEmpty(Filter))
            {
                ViewData["Filter"] = Filter;

                // Filtre aktifken Carouselleri gizlemek için boþ bir liste ayarlýyoruz.
                // Bu, finalde ViewModel'e gidecek.
                slidersForViewModel = new List<Slider>();

                if (Filter.StartsWith("Slider_"))
                {
                    // Dinamik Slider Filtresi
                    if (int.TryParse(Filter.Replace("Slider_", ""), out int sliderId))
                    {
                        filteredProducts = filteredProducts.Where(p => p.SliderId == sliderId);
                        var currentSlider = await _context.Sliders.FindAsync(sliderId);
                        ViewData["FiltreBaslik"] = currentSlider?.Baslik;
                    }
                }
                else
                {
                    // Sabit Filtreler (All, New ve Discount)
                    switch (Filter.ToLower())
                    {
                        case "all":
                            filteredProducts = allProducts; // Tüm ürünleri göster
                            ViewData["FiltreBaslik"] = "Tüm Ürün Kataloðu";
                            break;
                        case "new":
                            // Yeni Eklenenler (Son 20 ürün)
                            filteredProducts = filteredProducts.OrderByDescending(p => p.Id).Take(20);
                            ViewData["FiltreBaslik"] = "Yeni Eklenen Ürünler";
                            break;
                        case "discount":
                            // Ýndirimdekiler (Eski fiyatý olanlar)
                            filteredProducts = filteredProducts.Where(p => p.OldPrice.HasValue && p.OldPrice > 0);
                            ViewData["FiltreBaslik"] = "Ýndirimdeki Ürünler";
                            break;
                        default:
                            break;
                    }
                }
            }


            // ===================================
            // 3. MODELÝ HAZIRLAMA VE GÖNDERME
            // ===================================
            var viewModel = new HomeViewModel();

            // Arama/Filtre aktif deðilse
            if (string.IsNullOrEmpty(SearchTerm) && string.IsNullOrEmpty(Filter))
            {
                // Normal durumda sadece vitrin ürünlerini (SliderId == null) göster
                viewModel.Products = allProducts.Where(p => p.SliderId == null).ToList();
                viewModel.Sliders = allSliders; // Tüm slider carousellerini göster
            }
            else
            {
                // Arama veya filtre aktifse:
                viewModel.Products = filteredProducts.ToList();

                // ÖNEMLÝ DÜZELTME: Filtreleme sýrasýnda menünün kaybolmamasý için 
                // View'a giden Model.Sliders'ý, filtrelenmiþ carouseller yerine (ki onlar boþ),
                // dikey filtre menüsünü göstermek için kullanýlan *TÜM SLIDERLAR* listesiyle dolduruyoruz.
                // Carousel gizleme View'da yapýlacak.
                viewModel.Sliders = allSliders;
            }

            return View(viewModel);
        }

        // ... (Kalan metodlar) ...


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