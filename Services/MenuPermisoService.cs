using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using WebColegio.Models;

namespace WebColegio.Services
{
    public interface IMenuPermisoService
    {
        Task<List<CatMenu>> ObtenerPorRolAsync(int idRol);
        Task<List<CatMenu>> ObtenerCatalogoAsync();
        Task<bool> TieneAsync(int idRol, string codigo);
        void InvalidarCache();
        static int? ResolverIdRol(ClaimsPrincipal? user, ISession? session)
        {
            var fromSession = session?.GetInt32("RolUsuario");
            if (fromSession is > 0)
                return fromSession;

            if (user is null)
                return null;

            foreach (var claim in user.FindAll(ClaimTypes.Role))
            {
                if (int.TryParse(claim.Value, out var id) && id > 0)
                    return id;
            }

            return null;
        }
    }

    public class MenuPermisoService : IMenuPermisoService
    {
        private const string CachePrefix = "menu.rol.";
        private readonly IServicesApi _api;
        private readonly IMemoryCache _cache;

        public MenuPermisoService(IServicesApi api, IMemoryCache cache)
        {
            _api = api;
            _cache = cache;
        }

        public async Task<List<CatMenu>> ObtenerPorRolAsync(int idRol)
        {
            var key = CachePrefix + Version + "." + idRol;
            if (_cache.TryGetValue(key, out List<CatMenu>? cached) && cached is not null)
                return cached;

            var lista = await _api.GetMenusPorRolAsync(idRol) ?? new List<CatMenu>();
            _cache.Set(key, lista, TimeSpan.FromMinutes(2));
            return lista;
        }

        public Task<List<CatMenu>> ObtenerCatalogoAsync()
            => _api.GetMenusAsync();

        public async Task<bool> TieneAsync(int idRol, string codigo)
        {
            var menus = await ObtenerPorRolAsync(idRol);
            return menus.Any(m =>
                string.Equals(m.Codigo, codigo, StringComparison.OrdinalIgnoreCase) && m.Activo);
        }

        public void InvalidarCache()
        {
            _cache.Set("menu.version", Guid.NewGuid().ToString("N"));
        }

        private string Version => _cache.GetOrCreate("menu.version", _ => "v1") ?? "v1";
    }
}
