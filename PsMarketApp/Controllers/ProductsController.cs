using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PsMarketApp.Data;
using PsMarketApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace PsMarketApp.Controllers
{
    [Authorize] // Sadece Admin girebilir
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME (INDEX)
        public async Task<IActionResult> Index()
        {
            // Slider bilgisini de (Include) çekiyoruz ki listede adı görünsün
            var products = await _context.Products.Include(p => p.Slider).ToListAsync();
            return View(products);
        }

        // ⭐️ 2. DETAY SAYFASI (EKSİK OLAN KISIM EKLENDİ)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Slider) // Slider başlığını detay sayfasında göstermek için şart
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // 3. EKLEME SAYFASINI AÇ (GET)
        public IActionResult Create()
        {
            ViewData["SliderId"] = new SelectList(_context.Sliders, "Id", "Baslik");
            return View();
        }

        // 4. EKLEME İŞLEMİNİ YAP (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, string YeniSliderIsmi)
        {
            // A) Yeni Slider Oluşturma Mantığı
            if (!string.IsNullOrEmpty(YeniSliderIsmi))
            {
                var yeniSlider = new Slider { Baslik = YeniSliderIsmi };
                _context.Sliders.Add(yeniSlider);
                await _context.SaveChangesAsync();
                product.SliderId = yeniSlider.Id;
            }
            else if (product.SliderId == 0)
            {
                // Kullanıcı listeden seçmediyse, SliderId null (Listesiz/Vitrin ürünü)
                product.SliderId = null;
            }

            // Hata kontrollerini temizle (Slider nesnesi ve yeni isim alanı zorunlu değil)
            ModelState.Remove("Slider");
            ModelState.Remove("YeniSliderIsmi");

            // C) Ürünü Kaydet
            if (!string.IsNullOrEmpty(product.Name) && product.Price > 0)
            {
                try
                {
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Kayıt hatası: " + ex.Message);
                }
            }

            // Hata varsa sayfayı tekrar yükle
            ViewData["SliderId"] = new SelectList(_context.Sliders, "Id", "Baslik", product.SliderId);
            return View(product);
        }

        // 5. DÜZENLEME SAYFASI (EDIT GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["SliderId"] = new SelectList(_context.Sliders, "Id", "Baslik", product.SliderId);
            return View(product);
        }

        // 6. DÜZENLEME İŞLEMİ (EDIT POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, string YeniSliderIsmi)
        {
            if (id != product.Id) return NotFound();

            // Yeni Slider Ekleme Mantığı (Edit içinde de geçerli)
            if (!string.IsNullOrEmpty(YeniSliderIsmi))
            {
                var yeniSlider = new Slider { Baslik = YeniSliderIsmi };
                _context.Sliders.Add(yeniSlider);
                await _context.SaveChangesAsync();
                product.SliderId = yeniSlider.Id;
            }
            else if (product.SliderId == 0)
            {
                product.SliderId = null;
            }

            ModelState.Remove("Slider");
            ModelState.Remove("YeniSliderIsmi");

            if (!string.IsNullOrEmpty(product.Name) && product.Price > 0)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SliderId"] = new SelectList(_context.Sliders, "Id", "Baslik", product.SliderId);
            return View(product);
        }

        // 7. SİLME SAYFASI (DELETE GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Slider)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // 8. SİLME İŞLEMİNİ ONAYLA (DELETE POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}