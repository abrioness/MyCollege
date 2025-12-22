//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;

//namespace WebColegio.Controllers
//{
//    public class LoginController : Controller
//    {
//        // ... otros métodos ...

//        /// <summary>
//        /// Método para cerrar sesión correctamente
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            // Cerrar la autenticación de cookies
//            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
//            // Limpiar todas las cookies de sesión
//            foreach (var cookie in Request.Cookies.Keys)
//            {
//                Response.Cookies.Delete(cookie);
//            }

//            // Limpiar la sesión
//            HttpContext.Session.Clear();

//            // Redirigir a la página de login
//            return RedirectToAction("Index", "Login");
//        }

//        /// <summary>
//        /// Método GET para logout (alternativa si no usas POST)
//        /// </summary>
//        [HttpGet]
//        [Authorize]
//        public async Task<IActionResult> Logout()
//        {
//            // Cerrar la autenticación de cookies
//            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
//            // Limpiar todas las cookies de sesión
//            foreach (var cookie in Request.Cookies.Keys)
//            {
//                Response.Cookies.Delete(cookie);
//            }

//            // Limpiar la sesión
//            HttpContext.Session.Clear();

//            // Redirigir a la página de login
//            return RedirectToAction("Index", "Login");
//        }
//    }
//}

