using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblPago1
{
    public int IdPago { get; set; }

    public int? IdAlumno { get; set; }

    public decimal Monto { get; set; }

    public string Concepto { get; set; } = null!;

    public string IdMetodoPago { get; set; } = null!;

    public long Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
