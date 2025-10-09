using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebColegio.Models;
using WebColegio.Services;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace APP_WEB_HISTORIANIC.Controllers
{
    public class LoginController : Controller
    {

        private readonly IServicesApi _IService;

        public LoginController(IServicesApi iservices)
        {
            _IService = iservices;
        }
        // GET: LoginController
        private bool ValidateUser(string cedula, string password)
        {
            // Aquí deberías verificar contra tu base de datos
            if( cedula == "2812910810011m" && password == "123456")
            {
                return true;
            }
            return false;
        }
        public IActionResult Index()
        {
            //var cedula = HttpContext.Session.GetString("UsuarioCedula");

            //if (string.IsNullOrEmpty(cedula))
            //{
            //    return RedirectToAction("Login");
            //}

            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(string Login,byte Password)
        {
            // Buscar usuario por cédula
            var usuario = await _IService.GetLogin(Login);

            if (usuario== null)
            {
                TempData["Mensaje"]="Usuario o Contraseña Icorrecta!";
                return RedirectToAction("Index","Login");
            }

            // Convertir contraseña guardada en byte[] a string (hash)
            string storedHash = Encoding.UTF8.GetString(usuario.Password);
            // Verificar contraseña
            bool esValido = BCrypt.Net.BCrypt.Verify(Password, storedHash);

            if (!esValido)
            {
                TempData["Mensaje"] = "Password Incorrecta";
                return RedirectToAction("Index", "Login");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Login),
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString())  // 👈 Aquí se asigna el rol desde BD   
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });
            // Guardar datos en sesión
            HttpContext.Session.SetString("UsuarioCedula", Login);
            HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario); // si tienes Id
            return RedirectToAction("Index", "Estadisticas");


         }

        //public async Task<string> buscarusuarioLogin(string Login)
        //{
        //    var usuario = await _IService.GetLogin(Login);
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, usuario.Login),
        //        new Claim(ClaimTypes.Role, usuario.IdRol.ToString())  // 👈 Aquí se asigna el rol desde BD   
        //    };

        //    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        //    await HttpContext.SignInAsync(
        //       CookieAuthenticationDefaults.AuthenticationScheme,
        //       new ClaimsPrincipal(claimsIdentity),
        //       new AuthenticationProperties { IsPersistent = true });
        //    // Guardar datos en sesión
        //    HttpContext.Session.SetString("UsuarioCedula", Login);
        //    HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario); // si tienes Id

        //    return User.Identity.Name;
        //}

        [HttpGet]
        public async Task<IActionResult> CambiarPassword(string Login)
        {

            var usuario = await _IService.GetLogin(Login);
            string usuarioLogin = User.Identity.Name;//TempData["Login"] as string;
            if (string.IsNullOrEmpty(usuarioLogin))
            {
                TempData["Mensaje"] = "Debe ingresar su Usuario";
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Login = usuarioLogin;
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> CambiarPassword(string Login, string NuevaPassword, string ConfirmarPassword)
        //{
            

        //    if (string.IsNullOrEmpty(NuevaPassword) || NuevaPassword != ConfirmarPassword)
        //    {
        //        ViewBag.Error = "Las contraseñas no coinciden o son inválidas.";
        //        ViewBag.Login = Login;
        //        return View();
        //    }

        //    var usuario = await _IService.GetLogin(Login);
        //    if (usuario == null)
        //    {
        //        TempData["Mensaje"] = "El usuario no existe!";
        //        return RedirectToAction("Index", "Login");
        //    }
        //    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(NuevaPassword);
        //    byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);
        //    usuario.Password = passwordBytes;
        //    usuario.CambioClave = false;
        //    await _IService.PutUsuario(usuario);

        //    TempData["Mensaje"] = "Contraseña actualizada correctamente. Inicie sesión.";
        //    return RedirectToAction("Index", "Login");
        //}


        //[HttpPost]
        public async Task<IActionResult> Logout()
      {
         await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
         
            // Limpiar toda la sesión manualmente
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
     }

    }
}
