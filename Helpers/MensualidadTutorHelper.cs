using WebColegio.Models;

namespace WebColegio.Helpers
{
    /// <summary>Reglas de acceso del tutor a notas según mensualidad.</summary>
    public static class MensualidadTutorHelper
    {
        public const int TipoMovimientoMensualidad = 1;

        /// <summary>Indica si existe un pago de mensualidad activo del alumno para el mes calendario de la fecha de referencia (y período lectivo actual si se indica).</summary>
        public static bool TieneMensualidadMesActual(
            IEnumerable<TblPago> pagos,
            int idAlumno,
            int? idPeriodoLectivoActual,
            DateTime? referencia = null)
        {
            if (idAlumno <= 0)
                return false;

            var fecha = referencia ?? DateTime.Now;
            int mes = fecha.Month;

            IEnumerable<TblPago> q = pagos.Where(p =>
                p.Activo &&
                p.IdAlumno == idAlumno &&
                p.IdTipoMovimiento == TipoMovimientoMensualidad &&
                p.IdMes == mes);

            if (idPeriodoLectivoActual is int pid && pid > 0)
                q = q.Where(p => p.IdPeriodo == pid);

            return q.Any();
        }
    }
}
