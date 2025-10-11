namespace WebColegio.Models
{
    public class CatTipoMovimiento
    {
        public int IdTipoMovimiento { get; set; }
        public string Concepto { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
