# JWT en producción — Web + API

La aplicación web usa **cookies** para la sesión del navegador y un **JWT** (guardado en sesión) en cada llamada HTTP a la API (`Authorization: Bearer`).

La API debe validar el mismo JWT con la **misma clave, Issuer y Audience**.

---

## 1. Web (este proyecto) — ya configurado

| Componente | Ubicación |
|------------|-----------|
| Generación del token al iniciar sesión | `LoginController` + `JwtTokenService` |
| Envío del token a la API | `ApiBearerTokenHandler` + `ServicesApi` |
| Configuración | `appsettings.json` → sección `Jwt` |

### Variables de entorno en el servidor web (obligatorio en producción)

No dejar `SecretKey` en archivos publicados. En IIS / hosting:

```
Jwt__SecretKey=<clave-secreta-minimo-32-caracteres-aleatorios>
Jwt__Issuer=ColegioWeb
Jwt__Audience=ColegioApi
ApiSettings__BaseUrl=https://colegioparroquialsanfranciscojavier.com/ColSanFranciscoTest_Api/
ASPNETCORE_ENVIRONMENT=Production
```

Generar clave (PowerShell):

```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

### Desarrollo local

```powershell
cd WebColegio
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "DEV_SOLO_LOCAL_cambiar_en_prod_32chars_min"
```

---

## 2. API — cambios necesarios (proyecto `ColSanFrancisco_Api`)

Copiar los archivos de referencia en `docs/ApiJwtReferencia/` al proyecto API o integrar manualmente.

### Paquetes NuGet en la API

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
```

### appsettings de la API (mismos valores que la Web)

```json
"Jwt": {
  "Issuer": "ColegioWeb",
  "Audience": "ColegioApi",
  "SecretKey": "",
  "ExpirationMinutes": 480
}
```

En producción: **la misma** `Jwt__SecretKey` que en la Web.

### Program.cs de la API (resumen)

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey requerida");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Proteger controladores

En cada controlador de la API (o global):

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlumnosController : ControllerBase { ... }
```

Endpoints públicos (solo login si lo agrega la API):

```csharp
[AllowAnonymous]
[HttpPost("login")]
public IActionResult Login(...) { ... }
```

### CORS (si Web y API están en dominios distintos)

```csharp
builder.Services.AddCors(o => o.AddPolicy("Web", p => p
    .WithOrigins("https://colegioparroquialsanfranciscojavier.com")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));
// app.UseCors("Web");
```

---

## 3. Orden de despliegue recomendado

1. Generar **una** clave JWT y guardarla en un gestor seguro.
2. Publicar la **API** con JWT habilitado y `Jwt__SecretKey` configurada.
3. Probar un endpoint con Postman: header `Authorization: Bearer <token>`.
4. Publicar la **Web** con la **misma** clave.
5. Iniciar sesión en la Web y verificar que listados (alumnos, pagos) cargan sin 401.

### Si la API sigue sin JWT temporalmente

Los endpoints seguirán respondiendo sin token hasta que agregue `[Authorize]`. La Web ya envía el Bearer; cuando active JWT en la API, no hará falta cambiar la Web.

---

## 4. Seguridad adicional en producción

| Medida | Web | API |
|--------|-----|-----|
| HTTPS obligatorio | `UseHsts`, cookie `Secure` | Certificado TLS |
| Clave JWT ≥ 32 caracteres | Variable de entorno | Igual que Web |
| No exponer `obtenerUsuario` sin auth | — | Proteger o quitar GET con password |
| CORS restringido | — | Solo dominio del colegio |

---

## 5. Solución de problemas

| Síntoma | Causa probable |
|---------|----------------|
| Login: mensaje de SecretKey | Falta `Jwt:SecretKey` en Web |
| Listas vacías / error silencioso | API devuelve 401; revisar logs API |
| 401 en todos los endpoints | Issuer/Audience/Secret distintos entre Web y API |
| Funciona en test, falla en prod | Claves o URLs distintas por entorno |
