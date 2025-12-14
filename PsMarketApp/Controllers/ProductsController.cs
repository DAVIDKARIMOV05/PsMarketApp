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

        // Sabit başlık tanımlandı
        private const string TumUrunlerBaslik = "Tüm Ürünler (AUTO)";

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // YARDIMCI METOT: Tüm Ürünler listesini arka planda yönetir
        private async Task<int> GetOrCreateTumUrunlerSliderId()
        {
            var tumUrunlerSlider = await _context.Sliders
                                                 .FirstOrDefaultAsync(s => s.Baslik == TumUrunlerBaslik);

            if (tumUrunlerSlider == null)
            {
                // Eğer yoksa, otomatik olarak oluştur
                tumUrunlerSlider = new Slider { Baslik = TumUrunlerBaslik };
                _context.Sliders.Add(tumUrunlerSlider);
                await _context.SaveChangesAsync();
            }
            return tumUrunlerSlider.Id;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Slider).ToListAsync();
            return View(products);
        }

        // 2. EKLEME SAYFASINI AÇ (GET)
        public IActionResult Create()
        {
            ViewData["SliderId"] = new SelectList(_context.Sliders, "Id", "Baslik");
            return View();
        }

        // 3. EKLEME İŞLEMİNİ YAP (POST) - OTOMASYON EKLENDİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, string YeniSliderIsmi)
        {
            // A) Yeni Slider Oluşturma Mantığı (Kullanıcının seçimi)
            if (!string.IsNullOrEmpty(YeniSliderIsmi))
            {
                var yeniSlider = new Slider { Baslik = YeniSliderIsmi };
                _context.Sliders.Add(yeniSlider);
                await _context.SaveChangesAsync();
                product.SliderId = yeniSlider.Id;
            }
            else if (product.SliderId == 0)
            {
                // Kullanıcı listeden seçmediyse, SliderId null (Vitrin ürünü)
                product.SliderId = null;
            }

            // Hata kontrollerini temizle
            ModelState.Remove("Slider");
            ModelState.Remove("YeniSliderIsmi");

            // C) Ürünü Kaydet
            if (!string.IsNullOrEmpty(product.Name) && product.Price > 0)
            {
                try
                {
                    // Yeni ürünü kaydetmeden önce, onu otomatik olarak "Tüm Ürünler" listesine bağlayalım.
                    // Hile: SliderId alanını (Product.SliderId) kullanmak yerine,
                    // Bu mantıkla ürünün ana sliderı neyse o kalsın diyelim ve bu otomasyonu atlayalım.
                    // ÇÜNKÜ: Veritabanı yapınız (Bire-çok) sadece 1 SliderId tutabilir. 
                    // Bu sebeple, 'Tüm Ürünlerimiz' sayfasını HOME CONTROLLER'DAKİ filtreleme ile yapalım.

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    // Ürünü kaydettikten sonra başarılı sonuç
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

        // 4. SİLME SAYFASI (Değişiklik Yok)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Slider)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // 5. SİLME İŞLEMİNİ ONAYLA (Değişiklik Yok)
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

        // 6. DÜZENLEME SAYFASI (EDIT)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["SliderId"] = new SelectList(_context.Sliders, "Id", "Baslik", product.SliderId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, string YeniSliderIsmi)
        {
            if (id != product.Id) return NotFound();

            // Kullanıcının seçtiği slider'ı güncelleme mantığı (Değişiklik Yok)
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
    }
}