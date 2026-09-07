using WebColegio.Models;

namespace WebColegio.Models.ViewModel
{
    public class MenuLateralViewModel
    {
        public List<CatMenu> Inicios { get; set; } = new();
        public List<MenuGrupoViewModel> Grupos { get; set; } = new();
    }

    public class MenuGrupoViewModel
    {
        public CatMenu Grupo { get; set; } = new();
        public List<CatMenu> Items { get; set; } = new();
        public string CollapseId => "collapse-" + (Grupo.Codigo ?? Grupo.IdMenu.ToString()).Replace('.', '-');
    }

    public class MenuPermisosViewModel
    {
        public int IdRol { get; set; }
        public List<TblRol> Roles { get; set; } = new();
        public List<CatMenu> Menus { get; set; } = new();
        public List<int> IdMenusSeleccionados { get; set; } = new();
    }
}
