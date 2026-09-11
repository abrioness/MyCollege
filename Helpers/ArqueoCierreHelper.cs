using WebColegio.Models;

namespace WebColegio.Helpers
{
    /// <summary>
    /// El corte del arqueo lo marca el cierre de caja, no la hora a la que
    /// el administrador genera el reporte.
    /// Caja puede seguir cobrando después del cierre; esos recibos entran
    /// al siguiente cierre (al día siguiente).
    /// </summary>
    public static class ArqueoCierreHelper
    {
        public const int HoraCierreReferencia = 17;
        public const int MaxDiasDesdeCorteAnterior = 3;

        public static DateTime CierreReferencia(DateTime dia) =>
            dia.Date.AddHours(HoraCierreReferencia);

        public static bool EsCerrado(string? estado) =>
            string.Equals(estado, TblCierreCaja.EstadoCerrado, StringComparison.OrdinalIgnoreCase);

        public sealed class VentanaArqueo
        {
            public DateTime Inicio { get; init; }
            public DateTime Fin { get; init; }
            public TblArqueoDiario? ArqueoDelDia { get; init; }
            public TblCierreCaja? CierreDelDia { get; init; }
            public bool InicioEsCorteAnterior { get; init; }
            public bool HayCierreDelDia => CierreDelDia != null;
        }

        public static VentanaArqueo ResolverVentana(
            IEnumerable<TblArqueoDiario>? arqueos,
            IEnumerable<TblCierreCaja>? cierres,
            int? idRecinto,
            DateTime fechaReporte,
            DateTime ahora)
        {
            fechaReporte = fechaReporte.Date;
            var listaArqueos = (arqueos ?? Enumerable.Empty<TblArqueoDiario>())
                .Where(a => a.Activo && string.Equals(a.Serie, "A", StringComparison.OrdinalIgnoreCase))
                .Where(a => !idRecinto.HasValue || idRecinto.Value <= 0 || a.IdRecinto == idRecinto)
                .ToList();
            var listaCierres = (cierres ?? Enumerable.Empty<TblCierreCaja>())
                .Where(c => c.Activo && EsCerrado(c.EstadoCierre))
                .Where(c => !idRecinto.HasValue || idRecinto.Value <= 0 || c.IdRecinto == idRecinto)
                .ToList();

            var cierreDelDia = listaCierres
                .Where(c => c.FechaCierre.Date == fechaReporte)
                .OrderByDescending(c => c.FechaCierre)
                .FirstOrDefault();
            var arqueoDelDia = listaArqueos
                .Where(a => a.FechaRegistro.Date == fechaReporte)
                .OrderByDescending(a => a.FechaRegistro)
                .FirstOrDefault();

            DateTime fin;
            if (cierreDelDia != null)
                fin = cierreDelDia.FechaCierre;
            else if (fechaReporte == ahora.Date)
                fin = ahora;
            else if (arqueoDelDia != null)
                fin = arqueoDelDia.FechaRegistro;
            else
                fin = fechaReporte.AddDays(1).AddTicks(-1);

            var cierreAnterior = listaCierres
                .Where(c => c.FechaCierre < fin && c.FechaCierre.Date < fechaReporte)
                .OrderByDescending(c => c.FechaCierre)
                .FirstOrDefault();
            var arqueoAnterior = listaArqueos
                .Where(a => a.FechaRegistro < fin && a.FechaRegistro.Date < fechaReporte)
                .OrderByDescending(a => a.FechaRegistro)
                .FirstOrDefault();

            DateTime inicioFallback = CierreReferencia(fechaReporte.AddDays(-1));
            DateTime inicio = inicioFallback;
            bool inicioEsCorteAnterior = false;

            if (cierreAnterior != null && DiasEnRango(fechaReporte, cierreAnterior.FechaCierre.Date))
            {
                inicio = cierreAnterior.FechaCierre;
                inicioEsCorteAnterior = true;
            }
            else if (arqueoAnterior != null && DiasEnRango(fechaReporte, arqueoAnterior.FechaRegistro.Date))
            {
                inicio = arqueoAnterior.FechaRegistro;
                inicioEsCorteAnterior = true;
            }

            if (inicio > fin)
                inicio = inicioFallback;

            return new VentanaArqueo
            {
                Inicio = inicio,
                Fin = fin,
                ArqueoDelDia = arqueoDelDia,
                CierreDelDia = cierreDelDia,
                InicioEsCorteAnterior = inicioEsCorteAnterior
            };
        }

        public static bool EstaEnVentana(DateTime fechaPago, VentanaArqueo ventana)
        {
            if (fechaPago > ventana.Fin)
                return false;
            return ventana.InicioEsCorteAnterior ? fechaPago > ventana.Inicio : fechaPago >= ventana.Inicio;
        }

        public static TblCierreCaja CrearCierre(int idUsuario, int idRecinto, DateTime apertura, DateTime cierre)
        {
            return new TblCierreCaja
            {
                IdUsuarioCaja = idUsuario,
                IdRecinto = idRecinto,
                FechaApertura = apertura,
                FechaCierre = cierre,
                MontoIniCaja = 0,
                MontoFinCaja = 0,
                DiferenciaEfectivo = 0,
                TotalVentasEfectivo = 0,
                TotalVentasTarjetas = 0,
                TotalVentasOtros = 0,
                Descripcion = "Cierre de turno de caja",
                EstadoCierre = TblCierreCaja.EstadoCerrado,
                Activo = true,
                UsuarioRegistro = idUsuario,
                FechaRegistro = cierre
            };
        }

        private static bool DiasEnRango(DateTime fechaReporte, DateTime fechaCorte) =>
            (fechaReporte - fechaCorte.Date).TotalDays is >= 1 and <= MaxDiasDesdeCorteAnterior;
    }
}
