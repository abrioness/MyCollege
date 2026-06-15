namespace WebColegio.Services
{
    /// <summary>Almacena el JWT de la API en la sesión del usuario autenticado.</summary>
    public interface IApiTokenAccessor
    {
        void SetToken(string token);
        string? GetToken();
        void ClearToken();
    }
}
