using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PsMarketApp.Controllers // DİKKAT: Burası senin proje isminle aynı olsun!
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string kadi, string sifre)
        {
            // BURASI ŞİFRENİN OLDUĞU YER
            if (kadi == "admin" && sifre == "123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kadi)
                };

                var useridentity = new ClaimsIdentity(claims, "Login");
                ClaimsPrincipal principal = new ClaimsPrincipal(useridentity);

                await HttpContext.SignInAsync(principal);

                // Giriş başarılıysa Ürün Ekleme sayfasına git
                return RedirectToAction("Create", "Products");
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}