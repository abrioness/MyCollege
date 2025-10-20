using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblInventario
{
    public int IdInventario { get; set; }

    public int IdMovimiento { get; set; }

    public int IdProducto { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public int Cantidad { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string? ReferenciaDocumento { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual TblCatProducto IdProductoNavigation { get; set; } = null!;
}
