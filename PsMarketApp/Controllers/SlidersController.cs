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
            // 🛠️ DÜZELTME BURADA YAPILDI:
            // .Include(s => s.Products) ekleyerek, listeleme sırasında ürün sayılarını da çekiyoruz.
            // Artık "3 Adet Ürün" gibi sayaçlar çalışacak.
            return View(await _context.Sliders.Include(s => s.Products).ToListAsync());
        }

        // GET: Sliders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);

            if (slider == null) return NotFound();

            return View(slider);
        }

        // GET: Sliders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sliders/Create
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

        // ⭐️⭐️⭐️ EDIT METOTLARI (DÜZENLEME) ⭐️⭐️⭐️

        // GET: Sliders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null) return NotFound();

            return View(slider);
        }

        // POST: Sliders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Baslik")] Slider slider)
        {
            if (id != slider.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(slider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderExists(slider.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        // GET: Sliders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var slider = await _context.Sliders
                .Include(s => s.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (slider == null) return NotFound();

            ViewData["UrunSayisi"] = slider.Products?.Count ?? 0;

            return View(slider);
        }

        // POST: Sliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _context.Sliders
                                   .Include(s => s.Products)
                                   .FirstOrDefaultAsync(m => m.Id == id);

            if (slider == null) return RedirectToAction(nameof(Index));

            // 1. KONTROL: Slider'a bağlı ürün var mı?
            if (slider.Products != null && slider.Products.Any())
            {
                TempData["HataMesaji"] = $"HATA: Bu slider'ı silemezsiniz. Önce bağlı {slider.Products.Count} ürünü taşıyın veya silin.";
                return RedirectToAction(nameof(Delete), new { id = slider.Id });
            }

            try
            {
                _context.Sliders.Remove(slider);
                await _context.SaveChangesAsync();
                TempData["BasariMesaji"] = $"{slider.Baslik} silindi.";
            }
            catch (Exception)
            {
                TempData["HataMesaji"] = "Veritabanı hatası oluştu.";
                return RedirectToAction(nameof(Delete), new { id = slider.Id });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        {
            return _context.Sliders.Any(e => e.Id == id);
        }
    }
}