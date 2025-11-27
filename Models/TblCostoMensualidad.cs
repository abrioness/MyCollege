namespace WebColegio.Models
{
    public class TblCostoMensualidad
    {
        public int IdMensualidad { get; set; }
        public int CostoMensualidad { get; set; }
        public int IdPeriodo { get; set; }
        public int IdRecinto { get; set; }
        public int IdGrado { get; set; }
        public int IdModalidad { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
