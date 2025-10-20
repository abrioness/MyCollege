using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblPlanCuenta
{
    public int IdCuenta { get; set; }

    public string CodigoCuenta { get; set; } = null!;

    public string NombreCuenta { get; set; } = null!;

    public string TipoCuenta { get; set; } = null!;

    public string? SubTipoCuenta { get; set; }

    public int? IdCuentaPadre { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
