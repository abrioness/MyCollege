using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblPago
{
    public int IdPago { get; set; }

    public int IdAlumno { get; set; }

    public decimal? MontoPagado { get; set; }

    public int IdMetodoPago { get; set; }

    public string? Referencia { get; set; }

    public string? Observaciones { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime? FechaActualizo { get; set; }

    public int? UsuarioActualizo { get; set; }

    public virtual TblAlumno IdAlumnoNavigation { get; set; } = null!;

    public virtual TblMetodoPago IdMetodoPagoNavigation { get; set; } = null!;

    public virtual ICollection<TblRecibosCaja> TblRecibosCajas { get; set; } = new List<TblRecibosCaja>();
}
