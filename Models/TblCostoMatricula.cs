namespace WebColegio.Models
{
    public class TblCostoMatricula
    {
        public int IdCostoMatricula { get; set; }
        public int CostoMatricula { get; set; }
        public int IdPeriodo { get; set; }
        public int IdRecinto { get; set; }
        public int IdModalidad { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
