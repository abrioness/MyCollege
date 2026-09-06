using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblCatProveedore
{
    public int IdProveedor { get; set; }

    public string NombreProveedor { get; set; } = null!;

    public string? ContactoPrincipal { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    [JsonIgnore]
    public virtual ICollection<TblCatProducto> TblCatProductos { get; set; } = new List<TblCatProducto>();
}
