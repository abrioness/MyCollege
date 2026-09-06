using WebColegio.Models;

namespace WebColegio.Helpers
{
    /// <summary>
    /// El arqueo se cierra al guardarlo (habitualmente 3–4 p.m., antes de las 5 p.m.).
    /// Los cobros posteriores a esa hora pertenecen al arqueo del día siguiente.
    /// Si no hay arqueo reciente, el turno no se abre desde meses atrás: arranca
    /// a las 5 p.m. del día anterior (o del último cierre hábil, p. ej. viernes → lunes).
    /// </summary>
    public static class ArqueoCierreHelper
    {
        /// <summary>Cierre de referencia cuando no hay arqueo el día anterior (antes de las 5 p.m.).</summary>
        public const int HoraCierreReferencia = 17;

        /// <summary>Máximo de días hacia atrás para tomar el último arqueo (cubre viernes → lunes).</summary>
        public const int MaxDiasDesdeArqueoAnterior = 3;

        public static DateTime CierreReferencia(DateTime dia) =>
            dia.Date.AddHours(HoraCierreReferencia);

        public static (DateTime Inicio, DateTime Fin, TblArqueoDiario? ArqueoDelDia, bool InicioEsArqueoAnterior) ResolverVentana(
            IEnumerable<TblArqueoDiario>? arqueos,
            int? idRecinto,
            DateTime fechaReporte,
            DateTime ahora)
        {
            fechaReporte = fechaReporte.Date;
            var lista = (arqueos ?? Enumerable.Empty<TblArqueoDiario>())
                .Where(a => a.Activo && string.Equals(a.Serie, "A", StringComparison.OrdinalIgnoreCase))
                .Where(a => !idRecinto.HasValue || idRecinto.Value <= 0 || a.IdRecinto == idRecinto)
                .ToList();

            var arqueoDelDia = lista
                .Where(a => a.FechaRegistro.Date == fechaReporte)
                .OrderByDescending(a => a.FechaRegistro)
                .FirstOrDefault();

            DateTime fin = arqueoDelDia != null
                ? arqueoDelDia.FechaRegistro
                : (fechaReporte == ahora.Date
                    ? ahora
                    : fechaReporte.AddDays(1).AddTicks(-1));

            var anterior = lista
                .Where(a => a.FechaRegistro < fin && a.FechaRegistro.Date < fechaReporte)
                .OrderByDescending(a => a.FechaRegistro)
                .FirstOrDefault();

            DateTime inicioFallback = CierreReferencia(fechaReporte.AddDays(-1));
            DateTime inicio;
            bool inicioEsArqueoAnterior = false;

            if (anterior != null)
            {
                var dias = (fechaReporte - anterior.FechaRegistro.Date).TotalDays;
                if (dias >= 1 && dias <= MaxDiasDesdeArqueoAnterior)
                {
                    inicio = anterior.FechaRegistro;
                    inicioEsArqueoAnterior = true;
                }
                else
                {
                    inicio = inicioFallback;
                }
            }
            else
            {
                inicio = inicioFallback;
            }

            if (inicio > fin)
                inicio = CierreReferencia(fechaReporte.AddDays(-1));

            return (inicio, fin, arqueoDelDia, inicioEsArqueoAnterior);
        }

        public static bool EstaEnVentana(DateTime fechaPago, DateTime inicio, DateTime fin, bool inicioEsArqueoAnterior)
        {
            if (fechaPago > fin)
                return false;
            return inicioEsArqueoAnterior ? fechaPago > inicio : fechaPago >= inicio;
        }
    }
}
