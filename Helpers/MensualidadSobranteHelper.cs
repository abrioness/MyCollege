namespace WebColegio.Helpers
{
    public class AplicacionMensualidad
    {
        public int IdMes { get; set; }
        public decimal Monto { get; set; }
        public bool EsAbono { get; set; }
        public bool EsSobranteSiguienteMes { get; set; }
        /// <summary>0 = ciclo actual; 1 = enero (u otros meses) del ciclo siguiente.</summary>
        public int PeriodoOffset { get; set; }
    }

    /// <summary>
    /// Reparte un pago de mensualidad: cubre los meses seleccionados y aplica el
    /// sobrante como abono a los meses siguientes (mismo recibo).
    /// </summary>
    public static class MensualidadSobranteHelper
    {
        public const decimal Tolerancia = 0.05m;
        public const decimal Centavo = 0.01m;

        public static decimal RestanteMes(decimal tarifa, decimal pagado)
            => Math.Max(0m, tarifa - pagado);

        public static bool MesCancelado(decimal tarifa, decimal pagado)
            => tarifa <= 0m || pagado >= tarifa - Centavo;

        public static List<AplicacionMensualidad> Distribuir(
            decimal tarifa,
            IReadOnlyDictionary<int, decimal> pagadoPorMes,
            IReadOnlyList<int> mesesSeleccionados,
            decimal montoIngresado)
        {
            var result = new List<AplicacionMensualidad>();
            if (tarifa <= 0m || montoIngresado <= Centavo || mesesSeleccionados == null || mesesSeleccionados.Count == 0)
                return result;

            decimal disponible = montoIngresado;
            var pagado = new Dictionary<int, decimal>();
            foreach (var kv in pagadoPorMes)
                pagado[kv.Key] = kv.Value;

            decimal PagadoActual(int mes) => pagado.TryGetValue(mes, out var v) ? v : 0m;

            foreach (var mes in mesesSeleccionados.Where(m => m >= 1 && m <= 12).OrderBy(m => m))
            {
                decimal restante = RestanteMes(tarifa, PagadoActual(mes));
                if (restante <= Centavo)
                    continue;

                decimal aplicar = Math.Min(restante, disponible);
                if (aplicar <= Centavo)
                    break;

                result.Add(new AplicacionMensualidad
                {
                    IdMes = mes,
                    Monto = decimal.Round(aplicar, 2),
                    EsAbono = aplicar < restante - Centavo,
                    EsSobranteSiguienteMes = false,
                    PeriodoOffset = 0
                });
                pagado[mes] = PagadoActual(mes) + aplicar;
                disponible -= aplicar;
            }

            int cursor = mesesSeleccionados.Max();
            int offset = 0;
            while (disponible > Centavo)
            {
                cursor++;
                if (cursor > 12)
                {
                    cursor = 1;
                    offset++;
                    if (offset > 1)
                        break;
                }

                decimal pagadoMes = offset == 0 ? PagadoActual(cursor) : 0m;
                decimal restante = RestanteMes(tarifa, pagadoMes);
                if (restante <= Centavo)
                    continue;

                decimal aplicar = Math.Min(restante, disponible);
                if (aplicar <= Centavo)
                    break;

                result.Add(new AplicacionMensualidad
                {
                    IdMes = cursor,
                    Monto = decimal.Round(aplicar, 2),
                    EsAbono = aplicar < restante - Centavo,
                    EsSobranteSiguienteMes = true,
                    PeriodoOffset = offset
                });
                if (offset == 0)
                    pagado[cursor] = pagadoMes + aplicar;
                disponible -= aplicar;
            }

            return result;
        }

        public static decimal SobranteNoAplicado(decimal montoIngresado, IEnumerable<AplicacionMensualidad> aplicaciones)
            => Math.Max(0m, montoIngresado - aplicaciones.Sum(a => a.Monto));
    }
}
