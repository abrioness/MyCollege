using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblFacturaColegiatura
{
    public int IdFactura { get; set; }

    public int IdAlumno { get; set; }

    public int IdTipoColegiatura { get; set; }

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public decimal MontoTotal { get; set; }

    public int IdEstado { get; set; }

    public string MesFacturado { get; set; } = null!;

    public int AnyoFacturado { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual TblAlumno IdAlumnoNavigation { get; set; } = null!;

    public virtual TblCatTipoColegiatura IdTipoColegiaturaNavigation { get; set; } = null!;

    public virtual ICollection<TblPagoColegiatura> TblPagoColegiaturas { get; set; } = new List<TblPagoColegiatura>();
}
