using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PsMarketApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer zaten giriş yapılmışsa, direkt Admin Paneline gönder
            if (User.Identity.IsAuthenticated)
            {
                //   Admin/Index
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string kadi, string sifre)
        {
            // Kullanıcı adı ve şifre kontrolü
            if (kadi == "admin" && sifre == "bozok123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kadi)
                };

                var useridentity = new ClaimsIdentity(claims, "CookieAuth");
                ClaimsPrincipal principal = new ClaimsPrincipal(useridentity);

                // Giriş yap
                await HttpContext.SignInAsync("CookieAuth", principal);

                // DÜZELTME 2: Başarılı girişten sonra Admin/Index sayfasına yönlendir
                return RedirectToAction("Index", "Admin");
            }

            // Başarısız giriş denemesi
            ViewData["HataMesaji"] = "Kullanıcı adı veya şifre yanlış.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // Çıkış yap
            await HttpContext.SignOutAsync("CookieAuth");

            // Çıkış yapınca Ana Sayfaya atsın
            return RedirectToAction("Index", "Home");
        }
    }
}