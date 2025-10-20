using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblPagos
{
    public int IdPago { get; set; }

    public int? IdAlumno { get; set; }

    public decimal Monto { get; set; }

    //public int Concepto { get; set; } 
    public string MesPagado { get; set; }

    public int? Mora { get; set; }

    public int IdMetodoPago { get; set; } 

    public int IdTipoMovimiento { get; set; }


    public int IdTipoRecibo { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualizo { get; set; }

    public DateTime? FechaActualizo { get; set; }
    [JsonIgnore]
    public virtual TblAlumno? IdAlumnoNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual TblMetodoPago? IdMetodoPagoNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual TblCatTipoMovimiento? IdTipomovimientoNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual TblCatTipoRecibo? IdTipoReciboNavigation { get; set; } = null!;
    [JsonIgnore]

    public virtual ICollection<TblCatDetallePagos> TblCatDetallePagos { get; set; }
    //[JsonIgnore]
    //public virtual ICollection<TblRecibosCaja> TblRecibosCajas { get; set; } = new List<TblRecibosCaja>();

    //public virtual TblAlumno? tblAlumno { get; set; }
}
