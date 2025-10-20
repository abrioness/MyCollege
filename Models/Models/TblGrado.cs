using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
    [JsonIgnore]
    public virtual ICollection<TblCatAsignacionProfesorMaterium>? TblCatAsignacionProfesorMateria { get; set; } = new List<TblCatAsignacionProfesorMaterium>();
    [JsonIgnore]
    public virtual ICollection<TblAlumno>? TblAlumno { get; set; } = new List<TblAlumno>();
    [JsonIgnore]
    public virtual ICollection<TblRecibosCaja>? _tblCaja { get; set; } = new List<TblRecibosCaja>();
    [JsonIgnore]
    public virtual ICollection<TblNota>? _tblNotas{ get; set; } = new List<TblNota>();
}
