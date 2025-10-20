using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblRecibosCaja
{
    public int IdRecibo { get; set; }

    //public int? IdPago { get; set; }
    public decimal Monto { get; set; }

    public string RecibeDe { get; set; } = null!;

    public string Serie { get; set; } = null!;

    public int NumeroRecibo { get; set; }
    public string Concepto { get; set; } = null!;
    public int IdGrado { get; set; }
    public string TipoMovimiento { get; set; } = null!;

    public bool Activo { get; set; }

    public int? UsuarioRegistro { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public int UsuarioActualizo { get; set; }

    public DateTime FechaActualizo { get; set; }
    //[JsonIgnore]
    //public virtual TblPagos? IdPagoNavigation { get; set; }
    [JsonIgnore]
    public virtual TblGrado? IdGradoNavigation { get; set; }

}
