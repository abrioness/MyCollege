using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblCatModalidad
{
    public int IdModalidad { get; set; }

    public string Modalidad { get; set; } = null!;

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
