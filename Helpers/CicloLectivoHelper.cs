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

        /// <summary>
        /// Año escolar en curso para cobros (ene–dic). Si marcaron el siguiente año como Actual
        /// (p. ej. 2027 en octubre 2026), sigue siendo el año calendario.
        /// </summary>
        public static CatPeriodo? ResolverPeriodoVigente(IEnumerable<CatPeriodo>? periodos)
            => ResolverPeriodoMensualidad(periodos) ?? ResolverPeriodoActual(periodos);

        public static CatPeriodo? ResolverPeriodoSiguiente(IEnumerable<CatPeriodo>? periodos)
        {
            var vigente = ResolverPeriodoVigente(periodos);
            if (vigente == null)
                return null;

            int anio = AnioSiguienteCiclo(vigente.Periodo);
            return periodos?.FirstOrDefault(p => p.Activo && p.Periodo == anio);
        }

        /// <summary>
        /// El siguiente ciclo existe respecto al año de mensualidades (p. ej. 2027 habilitado en 2026),
        /// aunque alguien lo haya marcado como Actual.
        /// </summary>
        public static bool EstaHabilitadoSiguienteCiclo(IEnumerable<CatPeriodo>? periodos)
        {
            var vigente = ResolverPeriodoVigente(periodos);
            var siguiente = ResolverPeriodoSiguiente(periodos);
            return vigente != null
                   && siguiente != null
                   && siguiente.IdPeriodo != vigente.IdPeriodo;
        }

        public static bool EsPeriodoSiguienteAlVigente(IEnumerable<CatPeriodo>? periodos, int idPeriodo)
        {
            if (idPeriodo <= 0)
                return false;
            var vigente = ResolverPeriodoVigente(periodos);
            var elegido = periodos?.FirstOrDefault(p => p.IdPeriodo == idPeriodo);
            return vigente != null && elegido != null && elegido.Periodo > vigente.Periodo;
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
            var vigente = ResolverPeriodoVigente(periodos);
            if (vigente == null)
                return null;
            if (forzarCicloActual)
                return vigente;

            if (!CorrespondeMatriculaSiguienteCiclo(fecha, periodos))
                return vigente;

            return ResolverPeriodoSiguiente(periodos) ?? vigente;
        }

        public static bool EsMismoPeriodo(CatPeriodo? a, int idPeriodo)
            => a != null && a.IdPeriodo == idPeriodo;
    }
}
