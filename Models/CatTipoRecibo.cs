namespace WebColegio.Models
{
    public class CatTipoRecibo
    {
        public int IdTipoRecibo { get; set; }
        public string TipoRecibo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
