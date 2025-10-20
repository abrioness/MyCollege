using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblCatDetalleVenta
{
    public int IdDetaVentas { get; set; }

    public int IdVenta { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUniVenta { get; set; }

    public decimal? SubtoralLinea { get; set; }

    public decimal? DescuentoLinea { get; set; }

    public decimal? ImpuestoLinea { get; set; }

    public decimal? TotalLinea { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
    [JsonIgnore]
    public virtual TblCatProducto IdProductoNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual TblVenta IdVentaNavigation { get; set; } = null!;
}
