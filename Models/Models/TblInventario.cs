using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblInventario
{
    public int IdInventario { get; set; }

    //public int IdMovimiento { get; set; }

    public int IdProducto { get; set; }

    public int TipoMovimiento { get; set; } 

    public int Cantidad { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string? ReferenciaDocumento { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
    [JsonIgnore]
    public virtual TblCatProducto? IdProductoNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual TblCatMovimientoInventario? IdMovInventarioNavigation { get; set; } = null!;
}
