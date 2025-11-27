using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebColegio.Services;
using WebColegio.Models;

namespace WebColegio.Controllers
{
    public class LoginController : Controller
    {
        // GET: LoginController
        private readonly IServicesApi _IService;

        public LoginController(IServicesApi iservices)
        {
            _IService = iservices;
        }
        // GET: LoginController
        //private bool ValidateUser(string cedula, string password)
        //{
        //    // Aquí deberías verificar contra tu base de datos
        //    if( cedula == "2812910810011m" && password == "123456")
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        // GET: /Login/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string NombreUsuario, string Password)
        {
            // Buscar usuario por cédula
            var usuario = await _IService.GetLogin(NombreUsuario);
            if (usuario == null)
            {
                TempData["Mensaje"] = "Usuario o Contraseña Incorrecta!";
                TempData["Tipo"] = "warning";
                return View("Login");
            }
            var idrol = usuario.IdRol;
            var roles = await _IService.GetRol(idrol);
            

            // Convertir contraseña guardada en byte[] a string (hash)
            string storedHash = Encoding.UTF8.GetString(usuario.Password);
            // Verificar contraseña
            bool esValido = BCrypt.Net.BCrypt.Verify(Password, storedHash);

            if (!esValido)
            {
                TempData["Mensaje"] = "Password Incorrecta";
                TempData["Tipo"] = "warning";
                return View("Login");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                 new Claim(ClaimTypes.Role, roles.NombreRol),
                new Claim(ClaimTypes.Role, "Usuario"), // Todos son usuarios base                
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString()),  // 👈 Aquí se asigna el rol desde BD   
                 new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true // mantiene la sesión
                });
           
            // Guardar datos en sesión
            HttpContext.Session.SetString("UsuarioCedula", NombreUsuario);
            HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario);
            HttpContext.Session.SetInt32("RolUsuario", usuario.IdRol);// si tienes Id
            if (usuario.IdRol == 1)
            {
                return RedirectToAction("Index", "Pagos");
            }
            if (usuario.IdRol == 2)
            {
                return RedirectToAction("EstadoCuenta", "PagoCaja");
            }
            if (usuario.IdRol == 3)
            {
                return RedirectToAction("Index", "Notas");
            }

            if (usuario.IdRol==4)
            {
                //var validarTipoUsuario = await _IService.GetValidarTipoUsuario();
               var notasPorUsuario= await _IService.GetNotasPorUsuario(NombreUsuario);
               List<TblPago> pagosMensualidad = await _IService.GetPagosAsync();
                //int valirMensualidad=await _IService.ValidarMesesPendientes(pagosMensualidad, DateTime.Now.Month);
               
                if (notasPorUsuario.Count>0)
                {
                    return RedirectToAction("DetailsNotas", "Notas" ,new { cedulatutor=usuario.Cedula});
                }
                else
                {
                    TempData["Mensaje"] = "Usuario del Tutor no Existe!";
                    TempData["Tipo"] = "warning";

                    return View("Login");
                }
            }
            if (usuario.IdRol == 5)
            {
                return RedirectToAction("Index", "Inventario");
            }
            return RedirectToAction("Login");



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
            return RedirectToAction("Login");
        }
    }
}
