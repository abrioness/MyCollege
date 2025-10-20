using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblDetalleTranContable
{
    public int IdDetTranContable { get; set; }

    public int IdTransaccion { get; set; }

    public int IdCuenta { get; set; }

    public string TipoMoviento { get; set; } = null!;

    public decimal Monto { get; set; }

    public string? Descripcion { get; set; }

    public int UsuarioRegistro { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
