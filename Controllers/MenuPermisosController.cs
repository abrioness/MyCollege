using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    [Authorize(Roles = "Admin,UserSystem")]
    public class MenuPermisosController : Controller
    {
        private readonly IServicesApi _api;
        private readonly IMenuPermisoService _menuPermiso;

        public MenuPermisosController(IServicesApi api, IMenuPermisoService menuPermiso)
        {
            _api = api;
            _menuPermiso = menuPermiso;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? idRol)
        {
            var roles = (await _api.GetRolAsync() ?? new())
                .OrderBy(r => r.NombreRol)
                .ToList();
            if (roles.Any(r => r.Activo != 0))
                roles = roles.Where(r => r.Activo != 0).ToList();

            var seleccionado = idRol ?? roles.FirstOrDefault()?.IdRol ?? 0;
            var menus = seleccionado > 0
                ? await _api.GetMenusAdminRolAsync(seleccionado)
                : await _api.GetMenusAsync();

            return View(new MenuPermisosViewModel
            {
                IdRol = seleccionado,
                Roles = roles,
                Menus = menus.OrderBy(m => m.Orden).ThenBy(m => m.Titulo).ToList(),
                IdMenusSeleccionados = menus.Where(m => m.Asignado).Select(m => m.IdMenu).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int idRol, int[] idMenus)
        {
            if (idRol <= 0)
            {
                TempData["Mensaje"] = "Seleccione un rol.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var (ok, error) = await _api.GuardarPermisosMenuAsync(idRol, idMenus ?? Array.Empty<int>());
            if (ok)
            {
                _menuPermiso.InvalidarCache();
                TempData["Mensaje"] = "Permisos de menú guardados. El usuario de ese rol verá el cambio al recargar.";
                TempData["Tipo"] = "success";
            }
            else
            {
                TempData["Mensaje"] = error ?? "No se pudieron guardar los permisos.";
                TempData["Tipo"] = "error";
            }

            return RedirectToAction(nameof(Index), new { idRol });
        }
    }
}
