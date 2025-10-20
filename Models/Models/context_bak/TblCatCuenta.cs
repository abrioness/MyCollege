using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblCatCuenta
{
    public int IdCuenta { get; set; }

    public string NombreCuenta { get; set; } = null!;

    public int NumeroCuenta { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
