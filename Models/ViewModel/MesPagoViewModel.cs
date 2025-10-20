namespace WebColegio.Models.ViewModel
{
    public class MesPagoViewModel
    {
        public string Mes { get; set; } // "Enero", "Febrero", etc.
        public int Anio { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal MontoConDescuento { get; set; }
        public bool Seleccionado { get; set; }
        public bool EsAbono { get; set; }
    }
}
