using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PsMarketApp.Data;
using PsMarketApp.Models;
using Microsoft.AspNetCore.Authorization;
using PsMarketApp.Helpers; // ImageUploader için gerekli

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
            var products = await _context.Products.Include(p => p.Slider).ToListAsync();
            return View(products);
        }

        // 2. DETAY SAYFASI
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Slider)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // 3. EKLEME SAYFASI (GET)
        public IActionResult Create()
        {
            ViewData["SliderId"] = new SelectList(_context.Sliders, "Id", "Baslik");
            return View();
        }

        // 4. EKLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, string YeniSliderIsmi, IFormFile file)
        {
            // --- RESİM YÜKLEME KISMI ---
            if (file != null)
            {
                var uploader = new ImageUploader();
                string resimLinki = uploader.UploadImage(file);

                if (resimLinki != null)
                {
                    product.ImageUrl = resimLinki; // Cloudinary linkini kaydet
                }
            }
            // ---------------------------

            // A) Yeni Slider Mantığı
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
                    // HATAYI DETAYLI GÖSTERME KISMI (Burayı Güçlendirdik)
                    string mesaj = "Kayıt Başarısız: " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        mesaj += " | DETAY: " + ex.InnerException.Message;
                    }
                    ModelState.AddModelError("", mesaj);
                }
            }

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
        public async Task<IActionResult> Edit(int id, Product product, string YeniSliderIsmi, IFormFile file)
        {
            if (id != product.Id) return NotFound();

            // --- RESİM GÜNCELLEME KISMI ---
            if (file != null)
            {
                // Yeni resim seçildiyse yükle ve linki değiştir
                var uploader = new ImageUploader();
                string yeniLink = uploader.UploadImage(file);
                if (yeniLink != null)
                {
                    product.ImageUrl = yeniLink;
                }
            }
            else
            {
                // Yeni resim SEÇİLMEDİYSE, eski resim linkini veritabanından bulup koru
                var eskiUrun = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (eskiUrun != null)
                {
                    product.ImageUrl = eskiUrun.ImageUrl;
                }
            }
            // ------------------------------

            // Slider Mantığı
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
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex) // Burayı da güçlendirdik
                {
                    if (ex is DbUpdateConcurrencyException && !_context.Products.Any(e => e.Id == product.Id))
                    {
                        return NotFound();
                    }

                    string mesaj = "Güncelleme Başarısız: " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        mesaj += " | DETAY: " + ex.InnerException.Message;
                    }
                    ModelState.AddModelError("", mesaj);
                }
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