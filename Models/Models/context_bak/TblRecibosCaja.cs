using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblRecibosCaja
{
    public int IdRecibo { get; set; }

    public int IdPago { get; set; }

    public string RecibeDe { get; set; } = null!;

    public string Serie { get; set; } = null!;

    public int NumeroRecibo { get; set; }

    public long? Activo { get; set; }

    public int? UsuarioRegistro { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public int UsuarioActualizo { get; set; }

    public DateTime FechaActualizo { get; set; }

    public virtual TblPago IdPagoNavigation { get; set; } = null!;
}
