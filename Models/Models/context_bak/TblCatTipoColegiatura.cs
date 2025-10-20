using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblCatTipoColegiatura
{
    public int IdTipoColegiatura { get; set; }

    public string NombreConcepto { get; set; } = null!;

    public decimal MontoBase { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual ICollection<TblFacturaColegiatura> TblFacturaColegiaturas { get; set; } = new List<TblFacturaColegiatura>();
}
