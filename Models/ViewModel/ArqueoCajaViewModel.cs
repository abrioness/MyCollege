namespace WebColegio.Models.ViewModel
{
    public class ArqueoCajaViewModel
    {

        public string Colegio { get; set; }
        public string Serie { get; set; }
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }

        public List<IngresoDto> Ingresos { get; set; } = new();
        public List<EgresoDto> Egresos { get; set; } = new();

        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal TotalEfectivo { get; set; }

        public List<DetalleCordoba> DetalleCordobas { get; set; } = new();
        public List<DetalleDolar> DetalleDolares { get; set; } = new();

        public decimal TotalCordobas { get; set; }
        public decimal TotalDolares { get; set; }
        public decimal EquivalenteCordobas { get; set; }

        public string TotalEnLetras { get; set; }
    }

        public class IngresoDto
        {
            public string Concepto { get; set; }
            public string Recibo { get; set; }
            public int Cantidad { get; set; }
            public decimal Monto { get; set; }
        }

        public class EgresoDto
        {
            public string Detalle { get; set; }
            public decimal Monto { get; set; }
        }

        public class DetalleCordoba
        {
            public int Cantidad { get; set; }
            public decimal Denominacion { get; set; }
            public decimal Monto { get; set; }
        }

        public class DetalleDolar
        {
            public int Cantidad { get; set; }
            public decimal Denominacion { get; set; }
            public decimal Monto { get; set; }
        }

    
}
