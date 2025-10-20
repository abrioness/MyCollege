using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblPagoColegiatura
{
    public int IdPago { get; set; }

    public int IdFactura { get; set; }

    public DateTime FechaPago { get; set; }

    public decimal MontoPagado { get; set; }

    public int IdMetodoPago { get; set; }

    public string? ReferenciaPago { get; set; }

    public string? Descripcion { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual TblFacturaColegiatura IdFacturaNavigation { get; set; } = null!;

    public virtual TblMetodoPago IdMetodoPagoNavigation { get; set; } = null!;
}
