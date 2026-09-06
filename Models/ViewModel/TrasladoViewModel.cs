using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class TrasladoViewModel
    {
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; } = "";
        public int IdPeriodo { get; set; }
        public int IdRecintoOrigen { get; set; }
        public string NombreRecintoOrigen { get; set; } = "";
        public int IdRecintoDestino { get; set; }
        public int IdGrado { get; set; }
        public int? IdModalidad { get; set; }
        public decimal TarifaOrigen { get; set; }
        public decimal TarifaDestino { get; set; }
        public decimal MontoMatricula { get; set; }
        public bool CobraMatricula { get; set; }
        /// <summary>Mes en que ingresa al colegio destino (1-12). Se cobra ese mes y los siguientes.</summary>
        public int MesIngreso { get; set; }
        public decimal TotalACobrar { get; set; }
        public string MesesACobrarCsv { get; set; } = "";
        public List<TrasladoMesFila> Meses { get; set; } = new();
        public List<SelectListItem> RecintosDestino { get; set; } = new();
        public List<SelectListItem> Grados { get; set; } = new();
        public List<SelectListItem> Modalidades { get; set; } = new();
    }

    public class TrasladoMesFila
    {
        public int IdMes { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Pagado { get; set; }
        public bool Respetado { get; set; }
        public bool NoCorresponde { get; set; }
        public decimal SaldoACobrar { get; set; }
    }
}
