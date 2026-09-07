using Microsoft.AspNetCore.Mvc;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.ViewComponents
{
    public class MenuLateralViewComponent : ViewComponent
    {
        private readonly IMenuPermisoService _menuPermiso;

        public MenuLateralViewComponent(IMenuPermisoService menuPermiso)
        {
            _menuPermiso = menuPermiso;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new MenuLateralViewModel();
            var idRol = IMenuPermisoService.ResolverIdRol(HttpContext.User, HttpContext.Session);
            if (idRol is null or <= 0)
                return View(vm);

            var menus = (await _menuPermiso.ObtenerPorRolAsync(idRol.Value))
                .Where(m => m.Activo)
                .OrderBy(m => m.Orden)
                .ToList();

            vm.Inicios = menus
                .Where(m => string.Equals(m.Tipo, "Inicio", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var grupos = menus
                .Where(m => string.Equals(m.Tipo, "Grupo", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var items = menus
                .Where(m => string.Equals(m.Tipo, "Item", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var grupo in grupos)
            {
                var hijos = items
                    .Where(i => i.IdMenuPadre == grupo.IdMenu)
                    .OrderBy(i => i.Orden)
                    .ToList();
                if (hijos.Count == 0)
                    continue;
                vm.Grupos.Add(new MenuGrupoViewModel { Grupo = grupo, Items = hijos });
            }

            var idsGrupo = grupos.Select(g => g.IdMenu).ToHashSet();
            var huerfanos = items.Where(i => i.IdMenuPadre is int p && !idsGrupo.Contains(p)).ToList();
            if (huerfanos.Count > 0)
            {
                vm.Grupos.Add(new MenuGrupoViewModel
                {
                    Grupo = new Models.CatMenu
                    {
                        Codigo = "otros",
                        Titulo = "Otros",
                        IconoCss = "bi bi-list",
                        Tipo = "Grupo"
                    },
                    Items = huerfanos
                });
            }

            return View(vm);
        }
    }
}
