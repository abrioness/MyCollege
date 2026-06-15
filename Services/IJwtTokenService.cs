using WebColegio.Models;

namespace WebColegio.Services
{
    public interface IJwtTokenService
    {
        string CreateToken(TblUsuarios usuario, string nombreRol);
    }
}
