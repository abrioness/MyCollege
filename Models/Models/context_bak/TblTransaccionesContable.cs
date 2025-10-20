using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblTransaccionesContable
{
    public int IdTransaccion { get; set; }

    public DateOnly FechaTransaccion { get; set; }

    public string Descripcion { get; set; } = null!;

    public string? TipoDocOrigen { get; set; }

    public int? IdDocOrigen { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
}
