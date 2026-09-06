using WebColegio.Models.ViewModel;

namespace WebColegio.Helpers
{
    /// <summary>
    /// Solvencia: Insolvente si debe cualquier mensualidad de meses anteriores al mes calendario actual;
    /// Solvente si todos esos meses (hasta el mes anterior) están cancelados en el período lectivo.
    /// </summary>
    public static class EstadoCuentaSolvenciaHelper
    {
        /// <summary>Último mes (1-12) que debe estar pagado: mes anterior al actual.</summary>
        public static int ObtenerUltimoMesExigible(DateTime? referencia = null)
        {
            var fecha = (referencia ?? DateTime.Today).Date;
            return fecha.Month == 1 ? 12 : fecha.Month - 1;
        }

        /// <summary>Compatibilidad con código que usa un solo mes de referencia.</summary>
        public static int ObtenerMesMensualidadRequerido(DateTime? referencia = null)
            => ObtenerUltimoMesExigible(referencia);

        /// <summary>Meses del 1 al último exigible que deben revisarse.</summary>
        public static IEnumerable<int> ObtenerMesesExigibles(DateTime? referencia = null)
        {
            int ultimo = ObtenerUltimoMesExigible(referencia);
            return Enumerable.Range(1, ultimo);
        }

        /// <summary>
        /// Solvente solo si todos los meses anteriores al actual (1 … mes anterior) están cancelados.
        /// </summary>
        public static string EvaluarEstadoPagoMensualidad(
            IEnumerable<EstadoCuentaMesCelda>? meses,
            DateTime? referencia = null)
        {
            if (meses == null)
                return "N/D";

            var lista = meses as IList<EstadoCuentaMesCelda> ?? meses.ToList();
            if (lista.Count < 12)
                return "N/D";

            foreach (int mes in ObtenerMesesExigibles(referencia))
            {
                var celda = lista.FirstOrDefault(m => m.Mes == mes) ?? lista[mes - 1];
                if (celda.NoCorresponde)
                    continue;
                if (!celda.Cancelado)
                    return "Insolvente";
            }

            return "Solvente";
        }

        /// <summary>Nombres de meses con saldo pendiente (para detalle opcional).</summary>
        public static IReadOnlyList<string> ObtenerMesesPendientes(
            IEnumerable<EstadoCuentaMesCelda>? meses,
            DateTime? referencia = null)
        {
            if (meses == null)
                return Array.Empty<string>();

            var lista = meses as IList<EstadoCuentaMesCelda> ?? meses.ToList();
            if (lista.Count < 12)
                return Array.Empty<string>();

            var pendientes = new List<string>();
            foreach (int mes in ObtenerMesesExigibles(referencia))
            {
                var celda = lista.FirstOrDefault(m => m.Mes == mes) ?? lista[mes - 1];
                if (celda.NoCorresponde)
                    continue;
                if (!celda.Cancelado)
                    pendientes.Add(celda.NombreMes);
            }

            return pendientes;
        }

        /// <summary>
        /// Último mes (1-12) pagado en secuencia desde enero; refleja adelantos (ej. año completo → diciembre).
        /// </summary>
        public static int? ObtenerUltimoMesPagadoConsecutivo(IEnumerable<EstadoCuentaMesCelda>? meses)
        {
            if (meses == null)
                return null;

            var lista = meses as IList<EstadoCuentaMesCelda> ?? meses.ToList();
            if (lista.Count < 12)
                return null;

            int ultimo = 0;
            for (int mes = 1; mes <= 12; mes++)
            {
                var celda = lista.FirstOrDefault(m => m.Mes == mes) ?? lista[mes - 1];
                if (celda.NoCorresponde)
                    continue;
                if (!celda.Cancelado)
                    break;
                ultimo = mes;
            }

            return ultimo > 0 ? ultimo : null;
        }

        /// <summary>Mes más alto del año con mensualidad cancelada (incluye pagos adelantados con huecos).</summary>
        public static int? ObtenerMesMasAltoPagado(IEnumerable<EstadoCuentaMesCelda>? meses)
        {
            if (meses == null)
                return null;

            var lista = meses as IList<EstadoCuentaMesCelda> ?? meses.ToList();
            if (lista.Count < 12)
                return null;

            int max = 0;
            for (int mes = 1; mes <= 12; mes++)
            {
                var celda = lista.FirstOrDefault(m => m.Mes == mes) ?? lista[mes - 1];
                if (celda.NoCorresponde)
                    continue;
                if (celda.Cancelado)
                    max = mes;
            }

            return max > 0 ? max : null;
        }

        /// <summary>
        /// Texto "hasta [mes]": el mayor entre secuencia desde enero y último mes cancelado (cubre pago anual adelantado).
        /// </summary>
        public static int? ObtenerMesHastaPagadoParaMostrar(IEnumerable<EstadoCuentaMesCelda>? meses)
        {
            int? consecutivo = ObtenerUltimoMesPagadoConsecutivo(meses);
            int? masAlto = ObtenerMesMasAltoPagado(meses);

            if (consecutivo == null && masAlto == null)
                return null;
            if (consecutivo == null)
                return masAlto;
            if (masAlto == null)
                return consecutivo;
            return Math.Max(consecutivo.Value, masAlto.Value);
        }
    }
}
