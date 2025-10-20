using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblMetodoPago
{
    public int IdMetodoPago { get; set; }

    public string MetodoPago { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateOnly FehaRegistro { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateOnly? FechaActualiza { get; set; }

    public int? IdUsuarioActualiza { get; set; }

    public virtual ICollection<TblPagoColegiatura> TblPagoColegiaturas { get; set; } = new List<TblPagoColegiatura>();

    public virtual ICollection<TblPago> TblPagos { get; set; } = new List<TblPago>();
}
