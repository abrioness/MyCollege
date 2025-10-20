using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblColegiatura
{
    public int IdColegiatura { get; set; }

    public decimal MontoMensual { get; set; }

    public int IdGrado { get; set; }

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateOnly? FechaActualiza { get; set; }

    public int? IdusuarioActualiza { get; set; }
}
