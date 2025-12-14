using System.Text.Json.Serialization;

namespace WebColegio.Models
{
    public class TblAlumno
    {


        public int IdAlumno { get; set; }

        public string? Nombre { get; set; }
        public string? Apellido { get; set; }

        public DateTime? FechaNacimiento { get; set; }
        public int? Edad { get; set; }

        public string? CodigoMINED { get; set; }
        public string? CodigoAlumno { get; set; }
        public string? CodigoUnico { get; set; }
        public string? Cedula { get; set; }
        public string? GrupoEtnico { get; set; }
        public bool? BecaCompleta { get; set; }
        public bool? MediaBeca { get; set; }

        public int? IdGrado { get; set; }
        public int? IdTurno { get; set; }
        public int? IdModalidad { get; set; }
        public int? IdRecinto { get; set; }
        public int? IdSexo { get; set; }
        public int? IdGrupo { get; set; }
        public int? IdPeriodo { get; set; }

        public string? Direccion { get; set; }
        public string? Barrio { get; set; }
        public string? Telefono { get; set; }

        public string? NombreMadre { get; set; }
        public string? CedulaMadre { get; set; }
        public string? TelefonoMadre { get; set; }
        public string? NombrePadre { get; set; }
        public string? CedulaPadre { get; set; }
        public string? TelefonoPadre { get; set; }
        public string? NombreTutor { get; set; }
        public string? CedulaTutor { get; set; }
        public string? ContactoTutor { get; set; }
        public string? Correo { get; set; }
        public string? Observaciones { get; set; }
        public string? Departamento { get; set; }
        public string? Municipio { get; set; }
        public decimal? Peso { get; set; }
        public string? Talla { get; set; }
        public string? TipoEstudiante { get; set; }
        public int? IdDiscapacidad { get; set; }

        public bool? Activo { get; set; }

        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }
        public DateTime? FechaActualiza { get; set; }


    }
}
