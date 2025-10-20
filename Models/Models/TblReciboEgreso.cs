namespace Api_Colegio.Models
{
    public class TblReciboEgreso
    {
        public int IdReciboEgreso { get; set; }
        public string Serie { get; set; }
        public string Pagar { get; set; }
        public decimal Monto { get;set ; }
        public string Concepto { get;set; }
        
      public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int UsuarioActualizo { get;set; }
        public DateTime FechaActualizo { get;set; }
    }
}
