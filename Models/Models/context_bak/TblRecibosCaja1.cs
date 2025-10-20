using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblRecibosCaja1
{
    public int IdRecibo { get; set; }

    public int IdAlumno { get; set; }

    public int IdGrado { get; set; }

    public string? TipoMovimiento { get; set; }

    public string Cantidad { get; set; } = null!;

    public string? Concepto { get; set; }

    public string? Serie { get; set; }

    public int IdCajero { get; set; }

    public long Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateOnly? FechaActualiza { get; set; }
}
