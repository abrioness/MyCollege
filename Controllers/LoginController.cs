using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebColegio.Services;
using WebColegio.Models;
using Microsoft.AspNetCore.Authorization;
using WebColegio.Helpers;

namespace WebColegio.Controllers
{
    public class LoginController : Controller
    {
        // GET: LoginController
        private readonly IServicesApi _IService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IApiTokenAccessor _apiTokenAccessor;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginController(
            IServicesApi iservices,
            IJwtTokenService jwtTokenService,
            IApiTokenAccessor apiTokenAccessor,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _IService = iservices;
            _jwtTokenService = jwtTokenService;
            _apiTokenAccessor = apiTokenAccessor;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>Diagnóstico: abra /Login/EstadoApi en el navegador para ver si la Web alcanza la API.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> EstadoApi()
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "(no configurada)";
            var host = _configuration["ApiSettings:Host"] ?? "(no configurado)";
            var allowInvalid = _configuration["ApiSettings:AllowInvalidCertificate"] ?? "false";
            var entorno = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "(no definido)";
            string resultado;
            try
            {
                using var client = _httpClientFactory.CreateClient("ColegioApi");
                client.Timeout = TimeSpan.FromSeconds(15);
                var response = await client.GetAsync("api/Health");
                var body = await response.Content.ReadAsStringAsync();
                resultado = $"HTTP {(int)response.StatusCode} — {body.Substring(0, Math.Min(400, body.Length))}";
            }
            catch (Exception ex)
            {
                resultado = $"Error: {ex.Message}";
            }

            return Content(
                $"Entorno: {entorno}\n" +
                $"ApiSettings:BaseUrl: {baseUrl}\n" +
                $"ApiSettings:Host: {host}\n" +
                $"ApiSettings:AllowInvalidCertificate: {allowInvalid}\n" +
                $"Prueba GET api/Health: {resultado}\n\n" +
                "Nota: JwtTokenService NO afecta esta prueba. El login usa api/Usuarios/obtenerUsuario (sin JWT).",
                "text/plain; charset=utf-8");
        }
        
        [HttpPost]
        [AllowAnonymous]  // Debe ser AllowAnonymous porque el usuario aún no está autenticado
        public async Task<IActionResult> Login(string NombreUsuario, string Password)
        {
            // Buscar usuario por cédula
            var usuario = await _IService.GetLogin(NombreUsuario);
            if (usuario == null)
            {
                var detalleApi = _IService.LastApiError ?? string.Empty;
                if (detalleApi.Contains("404", StringComparison.OrdinalIgnoreCase))
                    TempData["Mensaje"] = $"La URL de la API no es correcta (404). {detalleApi} En el mismo servidor IIS use ApiSettings__BaseUrl=http://127.0.0.1/ColSanFranciscoTest_Api/ y ApiSettings__Host=colegioparroquialsanfranciscojavier.com";
                else if (detalleApi.Contains("500", StringComparison.OrdinalIgnoreCase) || detalleApi.Contains("503", StringComparison.OrdinalIgnoreCase) || detalleApi.Contains("base_datos", StringComparison.OrdinalIgnoreCase))
                    TempData["Mensaje"] = "La API no puede leer la base de datos. En IIS, edite el web.config de ColSanFranciscoTest_Api y configure ConnectionStrings__Conexion con la cadena SQL correcta.";
                else if (detalleApi.Contains("401", StringComparison.OrdinalIgnoreCase))
                    TempData["Mensaje"] = "La API rechazó la solicitud (401). Verifique JWT_SECRET_KEY igual en Web y API.";
                else if (detalleApi.Contains("ConnectionError", StringComparison.OrdinalIgnoreCase) || detalleApi.Contains("No such host", StringComparison.OrdinalIgnoreCase))
                    TempData["Mensaje"] = $"La Web no alcanza la API. {detalleApi} En el mismo servidor use ApiSettings__BaseUrl=http://localhost/ColSanFranciscoTest_Api/ y ApiSettings__Host=colegioparroquialsanfranciscojavier.com (no use la URL pública HTTPS desde el servidor).";
                else if (detalleApi.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                    TempData["Mensaje"] = $"La Web no alcanza la API. {detalleApi} Verifique que la API esté iniciada en IIS y que ApiSettings__Host coincida con el sitio.";
                else if (detalleApi.Contains("DeserializeError", StringComparison.OrdinalIgnoreCase))
                    TempData["Mensaje"] = "La API respondió pero el formato del usuario no es válido. Contacte al administrador.";
                else
                    TempData["Mensaje"] = $"No se pudo validar el usuario con la API. {detalleApi}";
                TempData["Tipo"] = "warning";
                return View("Login");
            }
            var idrol = usuario.IdRol;
            var roles = await _IService.GetRol(idrol);
            var nombreRol = AuthRoleHelper.ResolverNombreRol(roles?.NombreRol, idrol);
            
            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario) || usuario.Password == null || usuario.Password.Length == 0)
            {
                TempData["Mensaje"] = "El usuario no tiene credenciales válidas en el sistema. Contacte al administrador.";
                TempData["Tipo"] = "warning";
                return View("Login");
            }
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
                new Claim(ClaimTypes.Role, nombreRol),
                new Claim(ClaimTypes.Role, "Usuario"),
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim("nombre_completo", usuario.NombreCompleto ?? string.Empty),
                new Claim("nombre_rol", nombreRol)
            };
            if (usuario.IdRecinto.HasValue)
                claims.Add(new Claim("id_recinto", usuario.IdRecinto.Value.ToString()));
            if (!string.IsNullOrWhiteSpace(usuario.Cedula))
            {
                claims.Add(new Claim("Cedula", usuario.Cedula.Trim()));
                claims.Add(new Claim("cedula", usuario.Cedula.Trim()));
            }

            try
            {
                var apiToken = _jwtTokenService.CreateToken(usuario, nombreRol);
                _apiTokenAccessor.SetToken(apiToken);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Mensaje"] = ex.Message;
                TempData["Tipo"] = "warning";
                return View("Login");
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true
                });

            await HttpContext.Session.CommitAsync();

            // Guardar datos en sesión
            HttpContext.Session.SetString("UsuarioCedula", NombreUsuario);
            HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario);
            HttpContext.Session.SetInt32("RolUsuario", usuario.IdRol);// si tienes Id
            if (usuario.IdRol == 1 || usuario.IdRol==6)
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

            if (usuario.IdRol == 4)
            {
                var cedulaTutor = usuario.Cedula?.Trim();
                if (string.IsNullOrEmpty(cedulaTutor))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["Mensaje"] = "El usuario tutor no tiene cédula registrada. Solicite al administrador que actualice su perfil.";
                    TempData["Tipo"] = "warning";
                    return View("Login");
                }

                var alumnoVinculado = await _IService.V_alumnoNotas(cedulaTutor);
                if (alumnoVinculado == null || alumnoVinculado.IdAlumno <= 0)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["Mensaje"] = "No hay alumno vinculado a la cédula de este tutor. Verifique los datos en administración.";
                    TempData["Tipo"] = "warning";
                    return View("Login");
                }

                var pagos = await _IService.GetPagosAsync() ?? new List<TblPago>();
                var periodos = await _IService.GetPeriodoAsync() ?? new List<CatPeriodo>();
                var idPeriodoActual = periodos.FirstOrDefault(p => p.Activo && p.Actual)?.IdPeriodo;

                if (!MensualidadTutorHelper.TieneMensualidadMesActual(pagos, alumnoVinculado.IdAlumno, idPeriodoActual))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["Mensaje"] = "Para consultar las notas del alumno debe estar al día con la mensualidad del mes en curso.";
                    TempData["Tipo"] = "warning";
                    return View("Login");
                }

                return RedirectToAction("DetailsNotas", "Notas", new { cedulatutor = cedulaTutor });
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
        [Authorize]
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
        /// <summary>
        /// Método GET para logout (alternativa si no usas POST)
        /// </summary>
        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> Logout()
        //{
        //    // Cerrar la autenticación de cookies
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //    // Limpiar todas las cookies de sesión
        //    foreach (var cookie in Request.Cookies.Keys)
        //    {
        //        Response.Cookies.Delete(cookie);
        //    }

        //    // Limpiar la sesión
        //    HttpContext.Session.Clear();

        //    // Redirigir a la página de login
        //    return RedirectToAction("Index", "Login");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // 1. Cerrar la autenticación de cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Limpiar todas las cookies de sesión
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // 3. Limpiar la sesión y el JWT de la API
            _apiTokenAccessor.ClearToken();
            HttpContext.Session.Clear();

            // 4. Redirigir a la página de login
            return RedirectToAction("Login", "Login");
        }

    }
}
