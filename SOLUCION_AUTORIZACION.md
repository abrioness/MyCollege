# Solución: Proteger Acciones del Controlador

## Problema
Después de cerrar sesión, el menú se oculta pero el contenido principal (como la lista de alumnos) sigue siendo accesible.

## Causa
Las acciones de los controladores no están protegidas con el atributo `[Authorize]`, por lo que aunque el usuario cierre sesión, puede seguir accediendo directamente a las URLs.

## Solución

### Opción 1: Proteger Controladores Individuales (Recomendado)

Agrega el atributo `[Authorize]` a cada controlador que necesites proteger:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    [Authorize]  // Protege todas las acciones de este controlador
    public class AlumnosController : Controller
    {
        // Todas las acciones aquí requieren autenticación
        
        public IActionResult Index()
        {
            // Solo usuarios autenticados pueden acceder
            return View();
        }
        
        // Si alguna acción debe ser pública, usa [AllowAnonymous]
        [AllowAnonymous]
        public IActionResult PublicAction()
        {
            return View();
        }
    }
    
    [Authorize]
    public class NotasController : Controller
    {
        // Todas las acciones protegidas
    }
    
    [Authorize]
    public class PagosController : Controller
    {
        // Todas las acciones protegidas
    }
    
    // ... etc para todos los controladores que necesites proteger
}
```

### Opción 2: Filtro Global de Autorización (Más Seguro)

Protege TODAS las acciones por defecto y marca explícitamente las públicas:

#### En Program.cs (ASP.NET Core 6+):

```csharp
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ... otras configuraciones ...

// Agregar autorización global
builder.Services.AddControllersWithViews(options =>
{
    // Agregar filtro de autorización global
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// O más simple:
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

// ... resto de la configuración ...
```

#### En Startup.cs (ASP.NET Core 5 o anterior):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews(options =>
    {
        // Agregar filtro de autorización global
        options.Filters.Add(new AuthorizeFilter());
    });
    
    // ... resto de la configuración ...
}
```

Luego, marca las acciones públicas con `[AllowAnonymous]`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    [AllowAnonymous]  // Este controlador es público
    public class LoginController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Lógica de login
            return View();
        }
        
        // El Logout puede requerir autenticación
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Lógica de logout
            return RedirectToAction("Index");
        }
    }
    
    // Todos los demás controladores estarán protegidos automáticamente
    public class AlumnosController : Controller
    {
        // No necesitas [Authorize] aquí si usas filtro global
        public IActionResult Index()
        {
            return View();
        }
    }
}
```

### Opción 3: Proteger por Roles (Más Granular)

Si necesitas control por roles:

```csharp
[Authorize(Roles = "Admin")]
public class AlumnosController : Controller
{
    // Solo usuarios con rol "Admin" pueden acceder
}

[Authorize(Roles = "Admin,Cajero")]
public class PagosController : Controller
{
    // Solo usuarios con rol "Admin" o "Cajero" pueden acceder
}

[Authorize(Roles = "Docente")]
public class NotasController : Controller
{
    // Solo usuarios con rol "Docente" pueden acceder
}
```

## Verificar que el Logout Funciona Correctamente

Asegúrate de que tu método Logout esté correctamente implementado:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize]
public async Task<IActionResult> Logout()
{
    // 1. Cerrar la autenticación
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    // 2. Limpiar cookies
    foreach (var cookie in Request.Cookies.Keys)
    {
        Response.Cookies.Delete(cookie);
    }
    
    // 3. Limpiar sesión
    HttpContext.Session.Clear();
    
    // 4. Redirigir
    return RedirectToAction("Index", "Login");
}
```

## Controladores que Deben Estar Protegidos

Basándome en tu layout, estos controladores deben tener `[Authorize]`:

- ✅ `AlumnosController` - [Authorize]
- ✅ `NotasController` - [Authorize] (o [Authorize(Roles = "Docente,Secretaria,Admin")])
- ✅ `PagosController` - [Authorize(Roles = "Admin")]
- ✅ `PagoCajaController` - [Authorize(Roles = "Cajero,Admin")]
- ✅ `ReciboEgresoController` - [Authorize(Roles = "Cajero,Admin")]
- ✅ `ProductosController` - [Authorize(Roles = "Secretaria,Cajero,Admin")]
- ✅ `ArqueoDiarioController` - [Authorize(Roles = "Cajero,Admin,Secretaria")]
- ✅ `UsuariosController` - [Authorize(Roles = "UserSystem")]
- ❌ `LoginController` - [AllowAnonymous] (excepto Logout que puede requerir [Authorize])

## Prueba

1. Inicia sesión
2. Accede a `/Alumnos/Index` - Debe funcionar
3. Cierra sesión
4. Intenta acceder directamente a `/Alumnos/Index` - Debe redirigir a Login
5. Intenta acceder a `/Login/Index` - Debe funcionar (es público)

## Recomendación

**Usa la Opción 2 (Filtro Global)** porque:
- Es más seguro (protege todo por defecto)
- Menos código repetitivo
- Más fácil de mantener
- Solo necesitas marcar explícitamente lo que es público

