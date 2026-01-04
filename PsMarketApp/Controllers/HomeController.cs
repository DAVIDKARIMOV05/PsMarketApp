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

        // GÜNCEL Index Metodu: Sayfa Sayısı (page) eklendi
        public async Task<IActionResult> Index(string SearchTerm, string Filter, int page = 1)
        {
            int pageSize = 12; // Her sayfada kaç oyun görünsün?

            // 1. ADIM: Veriyi hemen çekme! Sorguyu hazırla (IQueryable)
            // ToListAsync() YERİNE AsQueryable() kullanıyoruz.
            var productsQuery = _context.Products.Include(p => p.Slider).AsQueryable();

            // Slider'lar menü için her zaman lazım (Bunlar az olduğu için List olabilir)
            var allSliders = await _context.Sliders.Include(s => s.Products).ToListAsync();
            var slidersForViewModel = allSliders;

            // ===================================
            // 2. ARAMA (SearchTerm) KONTROLÜ
            // ===================================
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string term = SearchTerm.ToLower().Trim();
                // Veritabanında SQL 'LIKE' sorgusu çalışır
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(term));

                slidersForViewModel = new List<Slider>(); // Arama varken sliderları gizle
                ViewData["SearchActive"] = true;
                ViewData["SearchTerm"] = SearchTerm;
            }
            // ===================================
            // 3. FİLTRE KONTROLÜ
            // ===================================
            else if (!string.IsNullOrEmpty(Filter))
            {
                ViewData["Filter"] = Filter;
                slidersForViewModel = new List<Slider>(); // Filtre varken sliderları gizle

                if (Filter.StartsWith("Slider_"))
                {
                    if (int.TryParse(Filter.Replace("Slider_", ""), out int sliderId))
                    {
                        productsQuery = productsQuery.Where(p => p.SliderId == sliderId);
                        var currentSlider = allSliders.FirstOrDefault(s => s.Id == sliderId);
                        ViewData["FiltreBaslik"] = currentSlider?.Baslik;
                    }
                }
                else
                {
                    switch (Filter.ToLower())
                    {
                        case "all":
                            // Zaten hepsi seçili, bir şey yapma
                            ViewData["FiltreBaslik"] = "Tüm Ürün Kataloğu";
                            break;
                        case "new":
                            // Sadece sıralama değiştirilir, sayfalama aşağıda yapılacak
                            productsQuery = productsQuery.OrderByDescending(p => p.Id);
                            ViewData["FiltreBaslik"] = "Yeni Eklenen Ürünler";
                            break;
                        case "discount":
                            productsQuery = productsQuery.Where(p => p.OldPrice.HasValue && p.OldPrice > 0);
                            ViewData["FiltreBaslik"] = "İndirimdeki Ürünler";
                            break;
                    }
                }
            }
            else
            {
                // Varsayılan Görünüm: Sadece Slider'a bağlı olmayan (Vitrin) ürünler mi?
                // Yoksa hepsi mi? Senin mantığına göre SliderId == null olanlar:
                productsQuery = productsQuery.Where(p => p.SliderId == null);
            }

            // ===================================
            // 4. SAYFALAMA (En Kritik Yer) 🚀
            // ===================================

            // Toplam kayıt sayısını filtrelerden SONRA hesapla
            var totalGames = await productsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalGames / (double)pageSize);

            // Veriyi Çek (Sadece o sayfadaki 12 taneyi getirir)
            var pagedProducts = await productsQuery
                                      .OrderByDescending(p => p.Id) // Yeniden eskiye sırala
                                      .Skip((page - 1) * pageSize)  // Öncekileri atla
                                      .Take(pageSize)               // 12 tane al
                                      .ToListAsync();               // ŞİMDİ veritabanına git!

            // ViewBag ile Sayfa bilgisini View'a gönder
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // ===================================
            // 5. MODELİ HAZIRLA VE GÖNDER
            // ===================================
            var viewModel = new HomeViewModel
            {
                Products = pagedProducts, // Sadece 12 ürün gider
                Sliders = slidersForViewModel
            };

            return View(viewModel);
        }

        public IActionResult Hakkimizda()
        {
            return View();
        }

        // Error metodu aynen kalabilir...
    }
}