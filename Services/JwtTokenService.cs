using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebColegio.Configuration;
using WebColegio.Helpers;
using WebColegio.Models;

namespace WebColegio.Services
{
    /// <summary>Mismo formato de claims que Api_Colegio.Services.JwtTokenService + nombre de rol para [Authorize(Roles)].</summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string CreateToken(TblUsuarios usuario, string nombreRol)
        {
            ValidateSecretKey();
            var rolNombre = AuthRoleHelper.ResolverNombreRol(nombreRol, usuario.IdRol);
            return BuildToken(usuario, rolNombre);
        }

        public string? TryCreateTokenFromUser(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario))
                return null;

            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var idRol = 0;
            foreach (var r in roles)
            {
                if (int.TryParse(r, out idRol))
                    break;
            }

            var nombreRol = roles.FirstOrDefault(r => !int.TryParse(r, out _) && !string.Equals(r, "Usuario", StringComparison.OrdinalIgnoreCase))
                ?? string.Empty;

            var usuario = new TblUsuarios
            {
                IdUsuario = idUsuario,
                IdRol = idRol,
                NombreUsuario = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                NombreCompleto = user.FindFirstValue("nombre_completo") ?? string.Empty,
                Cedula = user.FindFirstValue("cedula") ?? user.FindFirstValue("Cedula")
            };

            if (int.TryParse(user.FindFirstValue("id_recinto"), out var idRecinto))
                usuario.IdRecinto = idRecinto;

            try
            {
                return CreateToken(usuario, nombreRol);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private string BuildToken(TblUsuarios usuario, string rolNombre)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario ?? string.Empty),
                new(ClaimTypes.Role, usuario.IdRol.ToString()),
                new(ClaimTypes.Role, rolNombre),
                new("nombre_rol", rolNombre),
                new("nombre_completo", usuario.NombreCompleto ?? string.Empty),
            };

            if (usuario.IdRecinto.HasValue)
                claims.Add(new Claim("id_recinto", usuario.IdRecinto.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(usuario.Cedula))
                claims.Add(new Claim("cedula", usuario.Cedula.Trim()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void ValidateSecretKey()
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey) || _settings.SecretKey.Length < 32)
                throw new InvalidOperationException(
                    "Jwt:SecretKey no está configurada o es demasiado corta (mínimo 32 caracteres). " +
                    "Debe ser la MISMA clave que en la API (appsettings.Development.json).");
        }
    }
}
