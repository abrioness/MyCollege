using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblCatSexo
{
    public int IdSexo { get; set; }

    public string Sexo { get; set; } = null!;

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
