namespace WebColegio.Models
{
    public class CatDiscapacidad
    {
        public int Id_Discapacidad { get; set; }
        public string Discapacidad { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
