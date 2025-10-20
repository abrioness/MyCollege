using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblCatAsignacionProfesorMaterium
{
    public int IdAsignacion { get; set; }

    public int IdProfesor { get; set; }

    public int IdMateria { get; set; }

    public int IdGrado { get; set; }

    public int AnyoAcademico { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
    [JsonIgnore]
    public virtual TblGrado IdGradoNavigation { get; set; } = null!;
    [JsonIgnore]    
    public virtual TblMateria IdMateriaNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual TblProfesore IdProfesorNavigation { get; set; } = null!;
    //[JsonIgnore]
    //public virtual ICollection<TblNota> TblNota { get; set; } = new List<TblNota>();
}
