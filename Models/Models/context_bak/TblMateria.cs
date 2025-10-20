using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblMateria
{
    public int IdMateria { get; set; }

    public string NombreMateria { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual ICollection<TblCatAsignacionProfesorMaterium> TblCatAsignacionProfesorMateria { get; set; } = new List<TblCatAsignacionProfesorMaterium>();
}
