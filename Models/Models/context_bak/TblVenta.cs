using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblVenta
{
    public int IdVenta { get; set; }

    public int IdAlumno { get; set; }

    public DateTime FechaVenta { get; set; }

    public decimal TotalBruto { get; set; }

    public decimal TotalDescuento { get; set; }

    public decimal TotalImpuesto { get; set; }

    public decimal TotalNeto { get; set; }

    public string EstadoVenta { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual TblAlumno IdAlumnoNavigation { get; set; } = null!;

    public virtual ICollection<TblCatDetalleVenta> TblCatDetalleVenta { get; set; } = new List<TblCatDetalleVenta>();
}
