﻿using System.Text.Json.Serialization;

namespace WebColegio.Models
{
    public class TblAlumno
    {


        public int IdAlumno { get; set; }

        public string Nombre { get; set; } = null!;

        public string Apellido { get; set; } = null!;

        public DateOnly FechaNacimiento { get; set; }

        public int Edad { get; set; }
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

        public string? Correo { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
        
    

}
}
