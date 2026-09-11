using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
