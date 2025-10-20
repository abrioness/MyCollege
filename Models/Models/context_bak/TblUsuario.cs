using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblUsuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Correo { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public bool Bloqueo { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FecharRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
