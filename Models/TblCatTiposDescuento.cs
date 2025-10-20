using System.Text.Json.Serialization;

namespace WebColegio.Models
{
    public class TblCatTiposDescuento
    {
        public int IdTipoDescuento { get; set; }
        public string NombreDescuento { get; set; }
        public decimal Porcentaje { get; set; }
        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualizo { get; set; }

        public DateTime? FechaActualizo { get; set; }
       
    }
}
