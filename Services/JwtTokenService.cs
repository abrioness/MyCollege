using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebColegio.Configuration;
using WebColegio.Models;

namespace WebColegio.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string CreateToken(TblUsuarios usuario, string nombreRol)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey) || _settings.SecretKey.Length < 32)
                throw new InvalidOperationException(
                    "Jwt:SecretKey no está configurada o es demasiado corta (mínimo 32 caracteres). " +
                    "Use User Secrets o variable de entorno Jwt__SecretKey en producción.");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario ?? string.Empty),
                new(ClaimTypes.Role, nombreRol),
                new(ClaimTypes.Role, usuario.IdRol.ToString())
            };

            if (!string.IsNullOrWhiteSpace(usuario.Cedula))
                claims.Add(new Claim("Cedula", usuario.Cedula.Trim()));

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
    }
}
