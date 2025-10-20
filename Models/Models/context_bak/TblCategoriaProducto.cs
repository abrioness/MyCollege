using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblCategoriaProducto
{
    public int IdCateProducto { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual ICollection<TblCatProducto> TblCatProductos { get; set; } = new List<TblCatProducto>();
}
