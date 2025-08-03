﻿namespace WebColegio.Models
{
    public class PeriodoEvaluacion
    {
        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = null!;

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public int AnyoAcademico { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
