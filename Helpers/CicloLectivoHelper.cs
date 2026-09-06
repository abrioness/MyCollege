using WebColegio.Models;

namespace WebColegio.Helpers
{
    /// <summary>
    /// El ciclo lectivo vigente (CatPeriodo.Actual) cubre mensualidades ene–dic,
    /// morosos y adelantos del año en curso.
    /// La matrícula del siguiente ciclo se habilita sin sustituir el ciclo actual.
    /// </summary>
    public static class CicloLectivoHelper
    {
        public const int MesInicioMatriculaSiguienteCiclo = 10;

        public static bool EstaAbiertaMatriculaSiguienteCiclo(DateTime fecha)
            => fecha.Month >= MesInicioMatriculaSiguienteCiclo;

        public static int AnioSiguienteCiclo(int anioCicloActual)
            => anioCicloActual + 1;

        public static CatPeriodo? ResolverPeriodoActual(IEnumerable<CatPeriodo>? periodos)
        {
            var lista = periodos?.Where(p => p.Activo).ToList() ?? new List<CatPeriodo>();
            return lista.FirstOrDefault(p => p.Actual)
                   ?? lista.OrderByDescending(p => p.Periodo).ThenByDescending(p => p.IdPeriodo).FirstOrDefault();
        }

        public static CatPeriodo? ResolverPeriodoSiguiente(IEnumerable<CatPeriodo>? periodos)
        {
            var actual = ResolverPeriodoActual(periodos);
            if (actual == null)
                return null;

            int anio = AnioSiguienteCiclo(actual.Periodo);
            return periodos?.FirstOrDefault(p => p.Activo && p.Periodo == anio);
        }

        /// <summary>
        /// El siguiente ciclo existe y no está marcado como actual (p. ej. 2027 habilitado en 2026).
        /// </summary>
        public static bool EstaHabilitadoSiguienteCiclo(IEnumerable<CatPeriodo>? periodos)
        {
            var actual = ResolverPeriodoActual(periodos);
            var siguiente = ResolverPeriodoSiguiente(periodos);
            return actual != null
                   && siguiente != null
                   && siguiente.IdPeriodo != actual.IdPeriodo
                   && !siguiente.Actual;
        }

        /// <summary>
        /// Ya se puede matricular al siguiente ciclo: octubre o el administrador ya lo habilitó.
        /// </summary>
        public static bool CorrespondeMatriculaSiguienteCiclo(DateTime fecha, IEnumerable<CatPeriodo>? periodos)
            => EstaHabilitadoSiguienteCiclo(periodos) || EstaAbiertaMatriculaSiguienteCiclo(fecha);

        /// <summary>
        /// Mensualidades, morosos y adelantos del año en curso.
        /// Si el siguiente ciclo ya está habilitado (o quedó marcado como Actual),
        /// las mensualidades siguen en el año calendario vigente.
        /// </summary>
        public static CatPeriodo? ResolverPeriodoMensualidad(IEnumerable<CatPeriodo>? periodos)
        {
            var lista = periodos?.Where(p => p.Activo).ToList() ?? new List<CatPeriodo>();
            if (lista.Count == 0)
                return null;

            var marcado = lista.FirstOrDefault(p => p.Actual);
            var delCalendario = lista.FirstOrDefault(p => p.Periodo == DateTime.Now.Year);
            if (marcado != null && delCalendario != null && marcado.Periodo > DateTime.Now.Year)
                return delCalendario;

            return marcado
                   ?? delCalendario
                   ?? lista.OrderByDescending(p => p.Periodo).ThenByDescending(p => p.IdPeriodo).FirstOrDefault();
        }

        /// <summary>
        /// Año de matrícula/confirmación nueva del siguiente ciclo.
        /// No usar para mensualidad ni para liquidar deuda del ciclo vigente.
        /// </summary>
        public static int AnioDestinoMatricula(DateTime fecha, int anioCicloActual, bool siguienteHabilitado = false)
        {
            if (siguienteHabilitado || EstaAbiertaMatriculaSiguienteCiclo(fecha))
                return AnioSiguienteCiclo(anioCicloActual);
            return anioCicloActual;
        }

        /// <summary>
        /// Destino de matrícula/reserva del siguiente ciclo.
        /// <paramref name="forzarCicloActual"/>: continuidad, moroso o matrícula pendiente del año en curso.
        /// </summary>
        public static CatPeriodo? ResolverPeriodoMatricula(
            IEnumerable<CatPeriodo>? periodos,
            DateTime fecha,
            bool forzarCicloActual = false)
        {
            var actual = ResolverPeriodoActual(periodos);
            if (actual == null)
                return null;
            if (forzarCicloActual)
                return actual;

            if (!CorrespondeMatriculaSiguienteCiclo(fecha, periodos))
                return actual;

            return ResolverPeriodoSiguiente(periodos) ?? actual;
        }

        public static bool EsMismoPeriodo(CatPeriodo? a, int idPeriodo)
            => a != null && a.IdPeriodo == idPeriodo;
    }
}
