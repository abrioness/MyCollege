namespace WebColegio.Models.ViewModel
{
    public class PagoMensualidadViewModel
    {
        public int IdAlumno { get; set; }
        public string NombreEstudiante { get; set; }
        public decimal MontoMensual { get; set; } = 640.00m; // Monto base
        public List<MesPagoViewModel> MesesDisponibles { get; set; } = new List<MesPagoViewModel>();
        public string TipoDescuento { get; set; } // "Ninguno", "Beca", "MediaBeca", "Abono"
        public decimal PorcentajeDescuento { get; set; }
        public decimal MontoTotal { get; set; }
    }
}
