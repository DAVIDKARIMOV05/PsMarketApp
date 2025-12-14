using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PsMarketApp.Controllers
{
    [Authorize] // Sadece giriş yapmış adminler görebilir
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}