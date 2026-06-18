# Publicación en producción (IIS) — WebColegio + Api_Colegio

Guía para el servidor **colegioparroquialsanfranciscojavier.com**.

---

## Causa del error de login en producción

El mensaje *"El usuario ó la Contraseña no Existen!"* **no siempre** significa credenciales incorrectas. En producción suele indicar que **la Web no pudo leer el usuario desde la API** (URL incorrecta, API caída, JWT/BD mal configurados). La Web recibía un objeto vacío y mostraba ese mensaje.

Tras los cambios recientes, si la API no responde verá un mensaje más claro sobre la conexión con la API.

---

## 1. Arquitectura en el servidor

| Aplicación | Carpeta IIS sugerida | URL pública |
|------------|----------------------|-------------|
| **WebColegio** | sitio principal | `https://colegioparroquialsanfranciscojavier.com/` |
| **Api_Colegio** | aplicación virtual | `https://colegioparroquialsanfranciscojavier.com/ColSanFranciscoTest_Api/` |

**Importante:** la URL en `ApiSettings:BaseUrl` de la Web **debe coincidir exactamente** con la ruta publicada de la API (incluyendo `/` final).

---

## 2. Generar clave JWT (una sola para Web y API)

En PowerShell del servidor:

```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

Guarde esa clave en un lugar seguro. **Debe ser idéntica** en Web y API.

---

## 3. Variables de entorno en IIS

### API (`ColSanFrancisco_Api`)

En IIS → sitio → **ColSanFrancisco_Api** → **Configuración** → **Variables de entorno** (o editar `web.config`):

```xml
<environmentVariables>
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  <environmentVariable name="JWT_SECRET_KEY" value="PEGAR_AQUI_LA_CLAVE_GENERADA" />
  <environmentVariable name="ConnectionStrings__Conexion" value="Data Source=SERVIDOR_SQL;Initial Catalog=MyCollege;User ID=...;Password=...;Encrypt=True;TrustServerCertificate=True;" />
</environmentVariables>
```

Valores JWT que deben coincidir (ya están en `appsettings.Production.json`):

| Clave | Valor |
|-------|--------|
| Issuer | `ColegioWeb` |
| Audience | `ColegioApi` |

`Api:PathBase` en la API debe ser `/ColSanFrancisco_Api` (ya configurado en `appsettings.Production.json`).

### Web (sitio principal)

```xml
<environmentVariables>
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  <environmentVariable name="JWT_SECRET_KEY" value="LA_MISMA_CLAVE_QUE_EN_LA_API" />
  <environmentVariable name="ApiSettings__BaseUrl" value="https://colegioparroquialsanfranciscojavier.com/ColSanFranciscoTest_Api/" />
</environmentVariables>
```

---

## 4. Orden de publicación

1. Publicar **Api_Colegio** en Release → carpeta IIS `ColSanFrancisco_Api`.
2. Configurar variables de entorno de la API y **cadena de conexión SQL**.
3. Reiniciar el pool de la API.
4. Probar en el navegador:  
   `https://colegioparroquialsanfranciscojavier.com/ColSanFrancisco_Api/swagger`  
   (si Swagger está deshabilitado en prod, probar un endpoint anónimo):  
   `https://colegioparroquialsanfranciscojavier.com/ColSanFrancisco_Api/api/Usuarios/obtenerUsuario?login=USUARIO_EXISTENTE`  
   Debe devolver **200** con JSON del usuario o **404** si no existe (no HTML de error IIS).
5. Publicar **WebColegio** en Release.
6. Configurar variables de entorno de la Web (misma `JWT_SECRET_KEY`).
7. Reiniciar pool de la Web e iniciar sesión.

---

## 5. Publicar desde Visual Studio

### API

- Configuración: **Release**
- Perfil: carpeta del servidor o FTP
- Variable de entorno en el servidor: `ASPNETCORE_ENVIRONMENT=Production`

### Web

- Configuración: **Release**
- Perfil existente: `Properties/PublishProfiles/FolderProfile.pubxml`
- En el servidor IIS: `ASPNETCORE_ENVIRONMENT=Production`

---

## 6. Checklist de errores frecuentes

| Síntoma | Qué revisar |
|---------|-------------|
| Login falla para **todos** | `ApiSettings__BaseUrl` incorrecta; API detenida; ruta `/ColSanFrancisco_Api` distinta entre Web y API |
| Login OK pero listas vacías | `JWT_SECRET_KEY` distinta entre Web y API; Issuer/Audience distintos |
| API no arranca | Falta `ConnectionStrings__Conexion` o `JWT_SECRET_KEY` (&lt; 32 caracteres) |
| 401 en endpoints | En producción la API exige JWT; el login Web genera el token si la clave está bien |
| Certificado SSL | En producción `AllowInvalidCertificate` debe ser **false** (ya en `appsettings.Production.json`) |

---

## 7. Archivos de ejemplo

- `docs/web.config.ejemplo-API.xml` — plantilla IIS para la API
- `docs/web.config.ejemplo-Web.xml` — plantilla IIS para la Web
- `docs/JWT-PRODUCCION.md` — detalle técnico JWT

---

## 8. Si usa entorno de pruebas (`ColSanFranciscoTest_Api`)

Si la API está en otra ruta, cambie **los dos** lados:

- API `appsettings.Production.json` → `"PathBase": "/ColSanFranciscoTest_Api"`
- Web variable `ApiSettings__BaseUrl` → `https://colegioparroquialsanfranciscojavier.com/ColSanFranciscoTest_Api/`

No mezcle Test en la API y producción en la Web.
