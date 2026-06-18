using System.Security.Claims;
using WebColegio.Models;

namespace WebColegio.Services
{
    public interface IJwtTokenService
    {
        string CreateToken(TblUsuarios usuario, string nombreRol);

        /// <summary>Regenera JWT desde la cookie de sesión del usuario (respaldo si falla la sesión).</summary>
        string? TryCreateTokenFromUser(ClaimsPrincipal? user);
    }
}
