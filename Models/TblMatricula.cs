namespace WebColegio.Models
{
    public class TblMatricula
    {
        public const string EstadoReserva = "Reserva";
        public const string EstadoInscrito = "Inscrito";
        public const string EstadoActivo = "Activo";
        public const string EstadoRetirado = "Retirado";

        public int IdMatricula { get; set; }
        public int IdAlumno { get; set; }
        public int IdPeriodo { get; set; }
        public int IdGrado { get; set; }
        public int IdModalidad { get; set; }
        public int IdRecinto { get; set; }
        public int? IdTurno { get; set; }
        public int? IdGrupo { get; set; }
        public string Estado { get; set; } = EstadoReserva;
        public bool Continuidad { get; set; }
        public bool BecaCompleta { get; set; }
        public bool MediaBeca { get; set; }
        public string? Repitente { get; set; }
        public string? TipoEstudiante { get; set; }
        public DateTime? FechaMatricula { get; set; }
        public string? Observaciones { get; set; }
        public int? IdPagoReferencia { get; set; }
        public bool Activo { get; set; } = true;
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
