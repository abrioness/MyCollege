namespace WebColegio.Models
{
    public class CatMenu
    {
        public int IdMenu { get; set; }
        public int? IdMenuPadre { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Controlador { get; set; }
        public string? Accion { get; set; }
        public string? IconoCss { get; set; }
        public string Tipo { get; set; } = "Item";
        public int Orden { get; set; }
        public string? Target { get; set; }
        public bool UsaFechaHoy { get; set; }
        public bool Activo { get; set; } = true;
        public bool Asignado { get; set; }
    }
}
