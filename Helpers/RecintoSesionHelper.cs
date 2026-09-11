using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Services;

namespace WebColegio.Helpers
{
    /// <summary>
    /// El cajero solo opera y consulta el recinto asignado a su usuario.
    /// Admin y UserSystem siguen viendo todos los colegios.
    /// </summary>
    public static class RecintoSesionHelper
    {
        public const string ClaimIdRecinto = "id_recinto";

        public static bool EsCajero(ClaimsPrincipal user) =>
            user.IsInRole("Cajero") && !user.IsInRole("Admin") && !user.IsInRole("UserSystem");

        /// <summary>
        /// null = sin filtro. 0 = cajero sin recinto (no ve ninguno). Mayor que 0 = solo ese recinto.
        /// </summary>
        public static async Task<int?> ResolverFiltroRecintoAsync(ClaimsPrincipal user, IServicesApi services)
        {
            if (!EsCajero(user))
                return null;

            var raw = user.FindFirstValue(ClaimIdRecinto);
            if (int.TryParse(raw, out var idClaim) && idClaim > 0)
                return idClaim;

            var rawUser = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(rawUser, out var idUser) && idUser > 0)
            {
                var usuario = await services.GetUsuarioIdAsync(idUser);
                if (usuario?.IdRecinto is > 0)
                    return usuario.IdRecinto;
            }

            return 0;
        }

        public static List<T> Filtrar<T>(IEnumerable<T>? origen, int? filtro, Func<T, int?> idRecinto)
        {
            var list = origen?.ToList() ?? new List<T>();
            if (!filtro.HasValue)
                return list;
            if (filtro.Value <= 0)
                return new List<T>();
            return list.Where(x => idRecinto(x) == filtro.Value).ToList();
        }

        public static List<Recintos> RecintosVisibles(IEnumerable<Recintos>? recintos, int? filtro)
        {
            var list = (recintos ?? Enumerable.Empty<Recintos>()).ToList();
            if (!filtro.HasValue)
                return list.Where(r => r.Activo).ToList();
            if (filtro.Value <= 0)
                return new List<Recintos>();
            return list.Where(r => r.IdRecinto == filtro.Value).ToList();
        }

        public static List<SelectListItem> ToSelectList(IEnumerable<Recintos>? recintos, int? filtro, int? seleccionado = null)
        {
            var visibles = RecintosVisibles(recintos, filtro);
            int? sel = seleccionado ?? (filtro is > 0 ? filtro : null);
            return visibles.Select(r => new SelectListItem
            {
                Value = r.IdRecinto.ToString(),
                Text = r.Recinto,
                Selected = sel.HasValue && r.IdRecinto == sel.Value
            }).ToList();
        }

        public static int? ForzarAlGuardar(int? idFormulario, int? filtro) =>
            filtro is > 0 ? filtro.Value : idFormulario;
    }
}
