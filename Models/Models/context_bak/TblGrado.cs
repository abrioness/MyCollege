using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblGrado
{
    public int IdGrado { get; set; }

    public string NombreGrado { get; set; } = null!;

    public string? NivelEducativo { get; set; }

    public bool Activo { get; set; }

    public int UsuarioResgistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual ICollection<TblCatAsignacionProfesorMaterium> TblCatAsignacionProfesorMateria { get; set; } = new List<TblCatAsignacionProfesorMaterium>();
}
