namespace WebColegio.Models
{
    public class CatPeriodo
    {
        public int IdPeriodo { get; set; }

        public int Periodo { get; set; }

        public bool Actual { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }


    }
}
