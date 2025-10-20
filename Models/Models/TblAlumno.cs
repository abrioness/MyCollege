using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblAlumno
{

   
    public int IdAlumno { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public int Edad { get; set; }
    public string? CodigoMINED { get; set; }

    public string? CodigoAlumno { get; set; }

    public int? CodigoUnico { get; set; }
    public string? Cedula { get; set; }

    public int IdGrado { get; set; }

    public int IdTurno { get; set; }

    public int IdModalidad { get; set; }

    public int? IdRecinto { get; set; }
    public int? IdSexo { get; set; }
    public int IdGrupo { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string NombreMadre { get; set; } = null!;

    public string? NombrePadre { get; set; } 

    public string? NombreTutor { get; set; } 
    public string? CedulaTutor { get; set; }

    public string? ContactoTutor { get; set; }
    public string? Correo { get; set; }
    public string? Observaciones { get; set; }
    public int? IdDiscapacidad { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }
    [JsonIgnore]
    public virtual TblGrado? IdGradoNavigation { get; set; } 
    [JsonIgnore]
    public virtual TblCatGrupo? IdGrupoNavigation { get; set; } 
    [JsonIgnore]
    public virtual TblCatSexo? IdSexoNavigation { get; set; } 
    [JsonIgnore]
    public virtual TblCatModalidad? IdModalidadNavigation { get; set; } 
    [JsonIgnore]
    public virtual TblCatRecinto? IdRecintoNavigation { get; set; }
    [JsonIgnore]
    public virtual TblCatTurno? IdTurnoNavigation { get; set; }
    [JsonIgnore]
    public virtual TblCatDiscapacidad? IdDiscapacidadNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<TblFacturaColegiatura> TblFacturaColegiaturas { get; set; } = new List<TblFacturaColegiatura>();
    [JsonIgnore]
    public virtual ICollection<TblPagos> TblPagos { get; set; } = new List<TblPagos>();

    [JsonIgnore]
    public virtual ICollection<TblVenta> TblVenta { get; set; } = new List<TblVenta>();
    [JsonIgnore]
    public virtual ICollection<TblNota> TblNotas{ get; set; } = new List<TblNota>();
    [JsonIgnore]
    public virtual ICollection<TblCatMesesPendientes> TblMesesPendientes { get; set; } = new List<TblCatMesesPendientes>();

}
