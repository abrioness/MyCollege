using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblEstadoPago
{
    public int IdEstadoPago { get; set; }

    public string EstadoPago { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateOnly FecharRegistra { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateOnly? FecharActualiza { get; set; }

    public int? IdUsuarioActualiza { get; set; }
}
