using System;

namespace WebColegio.Helpers
{
    /// <summary>
    /// La fecha de pago suele llegar a medianoche porque el formulario es solo día
    /// o la columna SQL es date. En el recibo se completa con la hora de emisión.
    /// </summary>
    public static class FechaReciboHelper
    {
        public static DateTime? CompletarHoraPago(DateTime? fechaPago, DateTime fechaRegistro)
        {
            if (!fechaPago.HasValue || fechaPago.Value == default)
                return fechaRegistro != default ? fechaRegistro : DateTime.Now;

            var pago = fechaPago.Value;
            if (pago.TimeOfDay == TimeSpan.Zero && fechaRegistro != default && fechaRegistro.TimeOfDay > TimeSpan.Zero)
                return pago.Date.Add(fechaRegistro.TimeOfDay);

            return pago;
        }
    }
}
