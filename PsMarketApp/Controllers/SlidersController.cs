using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PsMarketApp.Data;
using PsMarketApp.Models;
using Microsoft.AspNetCore.Authorization;
namespace PsMarketApp.Controllers
{
    [Authorize]
    public class SlidersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SlidersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sliders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders.ToListAsync());
        }

        // GET: Sliders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // GET: Sliders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sliders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Baslik")] Slider slider)
        {
            if (ModelState.IsValid)
            {
                _context.Add(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        // ... (Diğer tüm metotlar aynı kalacak)

        // GET: Sliders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Slider'ı ve ona bağlı ÜRÜN sayısını öğrenmek için Include kullan
            var slider = await _context.Sliders
                .Include(s => s.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (slider == null)
            {
                return NotFound();
            }

            // Ürün sayısını View'a gönderelim (Opsiyonel ama bilgilendirici)
            ViewData["UrunSayisi"] = slider.Products?.Count ?? 0;

            return View(slider);
        }

        // POST: Sliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Silmeden önce bağlı ürünleri kontrol etmek için Products'ı dahil et
            var slider = await _context.Sliders
                                       .Include(s => s.Products)
                                       .FirstOrDefaultAsync(m => m.Id == id);

            if (slider == null)
            {
                // Silinmeye çalışılan slider zaten yoksa, sorun yok.
                return RedirectToAction(nameof(Index));
            }

            // 1. KONTROL: Slider'a bağlı ürün var mı?
            if (slider.Products != null && slider.Products.Any())
            {
                // Hata Mesajı Gönder: Yöneticiye bilgi veriyoruz.
                TempData["HataMesaji"] = $"HATA: Bu slider'ı silemezsiniz. Lütfen önce '{slider.Baslik}' başlığına bağlı olan {slider.Products.Count} adet ürünü başka bir slider'a taşıyın veya silin.";

                // Silme sayfasına geri dön, böylece hata mesajı görünür.
                return RedirectToAction(nameof(Delete), new { id = slider.Id });
            }

            // 2. KONTROL BAŞARILI: Ürün yok, silme işlemine devam et.
            try
            {
                _context.Sliders.Remove(slider);
                await _context.SaveChangesAsync();
                TempData["BasariMesaji"] = $"{slider.Baslik} slider'ı başarıyla silindi.";
            }
            catch (Exception)
            {
                // Eğer burada bir hata olursa (örneğin veritabanı hatası)
                TempData["HataMesaji"] = "Beklenmedik bir veritabanı hatası oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Delete), new { id = slider.Id });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        // ... (private SliderExists metodu ve dosyanın sonu aynı kalacak)
        {
            return _context.Sliders.Any(e => e.Id == id);
        }
    }
}
