namespace WebColegio.Models.ViewModel
{
    /// <summary>Reporte estado de cuenta: mensualidad por grado/recinto/modalidad según período lectivo actual; matrícula aparte.</summary>
    public class EstadoCuentaViewModel
    {
        public int AnioPeriodoReferencia { get; set; }
        public string? MensajePeriodo { get; set; }
        /// <summary>Mes calendario que debe estar pagado (mes anterior al actual).</summary>
        public int MesMensualidadRequerido { get; set; }
        public string? NombreMesMensualidadRequerido { get; set; }
        public List<EstadoCuentaFilaAlumno> Filas { get; set; } = new();
    }

    public class EstadoCuentaFilaAlumno
    {
        public int IdAlumno { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string? NombreGrado { get; set; }
        public string? Recinto { get; set; }
        public int AnioPeriodo { get; set; }
        /// <summary>Tarifa mensual del catálogo para grado/recinto/modalidad y período.</summary>
        public decimal MensualidadReferencia { get; set; }
        public decimal MatriculaReferencia { get; set; }
        public decimal TotalPagadoMatricula { get; set; }
        public decimal SaldoMatricula { get; set; }
        public bool MatriculaCancelada { get; set; }
        public EstadoCuentaMesCelda[] Meses { get; set; } = Array.Empty<EstadoCuentaMesCelda>();
        public decimal TotalSaldoMensualidades { get; set; }
        public decimal GranTotalPendiente { get; set; }
        public int? IdPagoParaEnlace { get; set; }
        public bool SinTarifaMensualidad { get; set; }
        /// <summary>Fila sin grado/recinto/modalidad; no se calculan tarifas.</summary>
        public bool DatosIncompletos { get; set; }
        public string? MotivoIncompleto { get; set; }
        /// <summary>Solvente / Insolvente según mensualidad del mes anterior al actual.</summary>
        public string? EstadoPagoMensualidad { get; set; }
        public int MesMensualidadRequerido { get; set; }
        public string? NombreMesMensualidadRequerido { get; set; }
        /// <summary>Meses con saldo pendiente (anteriores al mes actual).</summary>
        public string? MesesPendientesSolvencia { get; set; }
        /// <summary>Último mes del año lectivo con mensualidad pagada (para mostrar en pantalla).</summary>
        public int? UltimoMesPagado { get; set; }
        public string? NombreUltimoMesPagado { get; set; }
    }

    public class EstadoCuentaMesCelda
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; } = "";
        public decimal MontoEsperado { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal Saldo { get; set; }
        public bool Cancelado { get; set; }
    }
}
