using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WebColegio.Models;
using WebColegio.Models.ViewModel;

namespace WebColegio.Helpers
{
    public static class EstadoCuentaCalculoHelper
    {
        public const int TipoMensualidad = 1;
        public const int TipoMatricula = 2;
        public const int TipoMatriculaAbono = 4;
        public const int TipoRifa = 10;

        public static HashSet<int> IdsPorConcepto(IEnumerable<CatTipoMovimiento>? tipos, params string[] claves)
        {
            var ids = new HashSet<int>();
            if (tipos == null || claves.Length == 0)
                return ids;
            foreach (var t in tipos)
            {
                var c = (t.Concepto ?? "").ToLowerInvariant();
                if (claves.Any(k => c.Contains(k, StringComparison.Ordinal)))
                    ids.Add(t.IdTipoMovimiento);
            }
            return ids;
        }

        public static List<EstadoCuentaFilaAlumno> ConstruirFilas(
            IReadOnlyList<TblAlumno> alumnos,
            IReadOnlyList<TblPago> pagos,
            IReadOnlyList<TblCostoMensualidad> costosMen,
            IReadOnlyList<TblCostoMatricula> costosMat,
            IReadOnlyList<TblMatricula> matriculas,
            IReadOnlyList<Grados> grados,
            IReadOnlyList<Recintos> recintos,
            IReadOnlyList<TblCatMeses> mesesCatalog,
            int idPeriodoRef,
            int anioPeriodo,
            IReadOnlyList<CatPeriodo>? periodos = null,
            IReadOnlyCollection<int>? idsMensualidad = null,
            IReadOnlyCollection<int>? idsRifa = null,
            IReadOnlyCollection<int>? idsPromocion = null,
            IReadOnlyList<TblPagoCaja>? pagosCaja = null,
            IReadOnlyList<CatTipoMovimiento>? tiposMovimiento = null)
        {
            var culturaEs = new CultureInfo("es-NI");
            var nombresMes = Enumerable.Range(1, 12).Select(m =>
                mesesCatalog.FirstOrDefault(x => x.IdMes == m)?.Mes?.Trim()
                ?? culturaEs.DateTimeFormat.GetMonthName(m)).ToArray();

            int mesMensualidadRequerido = EstadoCuentaSolvenciaHelper.ObtenerMesMensualidadRequerido();
            string nombreMesRequerido = nombresMes[mesMensualidadRequerido - 1];
            var tiposMes = new HashSet<int>(idsMensualidad ?? Array.Empty<int>()) { TipoMensualidad };
            var tiposRifa = new HashSet<int>(idsRifa ?? Array.Empty<int>()) { TipoRifa };
            var tiposPromo = new HashSet<int>(idsPromocion ?? Array.Empty<int>());
            var tiposMatriculaPago = new HashSet<int> { TipoMatricula, TipoMatriculaAbono };
            if (tiposMovimiento != null)
            {
                foreach (var id in IdsPorConcepto(tiposMovimiento, "rifa", "rifas"))
                    tiposRifa.Add(id);
                foreach (var id in IdsPorConcepto(tiposMovimiento, "promoc"))
                    tiposPromo.Add(id);
                foreach (var t in tiposMovimiento)
                {
                    var n = NormalizarNombrePersona(t.Concepto);
                    if (!n.Contains("matricula"))
                        continue;
                    if (n.Contains("confirmacion") || n.Contains("reserva"))
                        continue;
                    tiposMatriculaPago.Add(t.IdTipoMovimiento);
                }
            }
            var filas = new List<EstadoCuentaFilaAlumno>();
            var matsPorAlumno = (matriculas ?? Array.Empty<TblMatricula>())
                .Where(m => m.Activo)
                .GroupBy(m => m.IdAlumno)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<TblMatricula>)g.ToList());

            foreach (var alumno in alumnos.OrderBy(a => a.Apellido).ThenBy(a => a.Nombre))
            {
                matsPorAlumno.TryGetValue(alumno.IdAlumno, out var matsAlum);
                var ciclo = ResolverCicloAlumno(
                    alumno,
                    matsAlum ?? Array.Empty<TblMatricula>(),
                    periodos,
                    idPeriodoRef,
                    anioPeriodo);
                int idPeriodoFila = ciclo.IdPeriodo;
                int anioFila = ciclo.Anio;
                var matriculaAlum = ciclo.Matricula;

                int? idG = matriculaAlum is { IdGrado: > 0 } ? matriculaAlum.IdGrado : alumno.IdGrado;
                int? idR = matriculaAlum is { IdRecinto: > 0 } ? matriculaAlum.IdRecinto : alumno.IdRecinto;
                int? idMod = matriculaAlum is { IdModalidad: > 0 } ? matriculaAlum.IdModalidad : alumno.IdModalidad;

                if (!idG.HasValue || !idR.HasValue || !idMod.HasValue)
                {
                    filas.Add(new EstadoCuentaFilaAlumno
                    {
                        IdAlumno = alumno.IdAlumno,
                        NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}".Trim(),
                        AnioPeriodo = anioFila,
                        DatosIncompletos = true,
                        MotivoIncompleto = "Falta grado, recinto o modalidad en la matrícula del ciclo o en la ficha."
                    });
                    continue;
                }

                bool becaCompleta = (matriculaAlum?.BecaCompleta == true || alumno.BecaCompleta == true)
                    && (alumno.IdPeriodo == idPeriodoFila || matriculaAlum != null);
                bool mediaBeca = !becaCompleta
                    && (matriculaAlum?.MediaBeca == true || alumno.MediaBeca == true)
                    && (alumno.IdPeriodo == idPeriodoFila || matriculaAlum != null);

                var costoMen = ResolverFilaCostoMensualidad(costosMen, idR, idG.Value, idPeriodoFila, idMod);
                decimal montoMensual = costoMen != null ? (decimal)costoMen.CostoMensualidad : 0m;
                if (becaCompleta)
                    montoMensual = 0m;
                else if (mediaBeca && montoMensual > 0)
                    montoMensual *= 0.5m;

                var costoMat = ResolverFilaCostoMatricula(costosMat, idR, idPeriodoFila, idMod);
                decimal montoMat = costoMat != null ? (decimal)costoMat.CostoMatricula : 0m;

                var pagosAlum = pagos
                    .Where(p => p.IdAlumno == alumno.IdAlumno && p.IdPeriodo == idPeriodoFila && p.Activo)
                    .ToList();

                bool PagoEsMatriculaOAbono(TblPago p)
                {
                    if (!p.Activo || p.IdAlumno != alumno.IdAlumno)
                        return false;
                    if (!tiposMatriculaPago.Contains(p.IdTipoMovimiento))
                        return false;
                    if (p.IdPeriodo == idPeriodoFila)
                        return true;
                    var anioPago = periodos?.FirstOrDefault(x => x.IdPeriodo == p.IdPeriodo)?.Periodo ?? 0;
                    return anioPago > 0 && anioPago == anioFila;
                }

                decimal pagadoMatBruto = pagos.Where(PagoEsMatriculaOAbono).Sum(p => p.Monto);
                decimal pagadoEneroBruto = pagosAlum
                    .Where(p => tiposMes.Contains(p.IdTipoMovimiento) && p.IdMes == 1)
                    .Sum(p => p.Monto);
                decimal montoMatNeta = MatriculaNetaDesdePaquete(montoMat, montoMensual);
                var (pagadoMat, pagadoEneroConMatricula) = DistribuirPagoMatriculaYEnero(
                    montoMat, pagadoMatBruto, pagadoEneroBruto, montoMensual);

                bool esTraslado = TrasladoHelper.EsTraslado(matriculaAlum, alumno);
                int? mesIngreso = esTraslado
                    ? TrasladoHelper.LeerMesIngreso(matriculaAlum?.Observaciones)
                      ?? TrasladoHelper.LeerMesIngreso(alumno.Observaciones)
                    : null;

                var estadoMat = CalcularEstadoMatricula(montoMat, montoMensual, pagadoMat, pagadoEneroConMatricula);
                bool matCancelada = estadoMat.Cancelada;
                decimal saldoMat = matCancelada ? 0m : estadoMat.SaldoPendiente;
                bool tieneAbonoMat = !matCancelada && pagos.Any(PagoEsMatriculaOAbono);

                var meses = new EstadoCuentaMesCelda[12];
                decimal totalSaldoMeses = 0m;

                for (int m = 1; m <= 12; m++)
                {
                    var pagosMes = pagosAlum
                        .Where(p => tiposMes.Contains(p.IdTipoMovimiento) && p.IdMes == m)
                        .ToList();
                    decimal pagadoMes = m == 1 ? pagadoEneroConMatricula : pagosMes.Sum(p => p.Monto);
                    decimal esperado = montoMensual;
                    decimal tarifaHistorica = 0m;
                    var recintoPagoMes = pagosMes.FirstOrDefault(p => p.IdRecinto.HasValue && p.IdRecinto.Value > 0)?.IdRecinto;
                    int? recintoHistorico = recintoPagoMes;
                    if ((!recintoHistorico.HasValue || recintoHistorico.Value == idR) && esTraslado)
                        recintoHistorico = TrasladoHelper.LeerRecintoOrigen(matriculaAlum?.Observaciones)
                            ?? TrasladoHelper.LeerRecintoOrigen(alumno.Observaciones);
                    if (recintoHistorico.HasValue && recintoHistorico.Value != idR)
                    {
                        var costoHist = ResolverFilaCostoMensualidad(costosMen, recintoHistorico, idG.Value, idPeriodoFila, idMod);
                        tarifaHistorica = costoHist != null ? (decimal)costoHist.CostoMensualidad : 0m;
                        if (mediaBeca && tarifaHistorica > 0)
                            tarifaHistorica *= 0.5m;
                    }

                    bool noCorresponde = esTraslado && mesIngreso.HasValue && m < mesIngreso.Value
                        && !TrasladoHelper.MesPagadoSeRespeta(pagadoMes, tarifaHistorica, esperado);
                    bool respetado = TrasladoHelper.MesPagadoSeRespeta(pagadoMes, tarifaHistorica, esperado);
                    decimal saldo = noCorresponde || respetado ? 0m : Math.Max(0m, esperado - pagadoMes);
                    bool cancelado = noCorresponde || esperado <= 0m || respetado || saldo <= 0.01m;
                    if (!noCorresponde)
                        totalSaldoMeses += cancelado ? 0m : saldo;

                    meses[m - 1] = new EstadoCuentaMesCelda
                    {
                        Mes = m,
                        NombreMes = nombresMes[m - 1],
                        MontoEsperado = noCorresponde ? 0m : esperado,
                        MontoPagado = pagadoMes,
                        Saldo = cancelado ? 0m : saldo,
                        Cancelado = cancelado,
                        TieneAbonoParcial = !noCorresponde && !cancelado && pagadoMes > 0.01m,
                        NoCorresponde = noCorresponde
                    };
                }

                var ultimoPago = pagosAlum.OrderByDescending(p => p.IdPago).FirstOrDefault();
                var cajaAlum = RecibosCajaDelAlumno(pagosCaja, alumno, idPeriodoFila, anioFila).ToList();
                string nombreGradoFila = grados.FirstOrDefault(g => g.IdGrado == idG)?.NombreGrado ?? string.Empty;
                bool aplicaPromo = EsGradoConPromocion(nombreGradoFila);
                var rifasPagoAlum = pagos
                    .Where(p => p.IdAlumno == alumno.IdAlumno && p.Activo && tiposRifa.Contains(p.IdTipoMovimiento))
                    .OrderBy(p => p.FechaRegistro)
                    .ThenBy(p => p.IdPago)
                    .ToList();
                var rifasCajaAlum = cajaAlum
                    .Where(c => EsReciboRifa(c, tiposRifa, tiposMovimiento))
                    .OrderBy(c => c.FechaRegistro)
                    .ThenBy(c => c.IdPagoCaja)
                    .ToList();
                int cantidadRifas = rifasPagoAlum.Count + rifasCajaAlum.Count;
                var (rifaS1, rifaS2) = EstadoRifasPorOrden(cantidadRifas);
                var (montoRifa1, montoRifa2) = MontosRifaPorOrden(rifasPagoAlum, rifasCajaAlum);
                decimal montoPromo = 0m;
                if (aplicaPromo)
                {
                    if (tiposPromo.Count > 0)
                        montoPromo += pagosAlum.Where(p => tiposPromo.Contains(p.IdTipoMovimiento)).Sum(p => p.Monto);
                    montoPromo += cajaAlum.Where(c => EsReciboPromocion(c, tiposPromo, tiposMovimiento)).Sum(c => c.Monto);
                }
                bool promoPagada = aplicaPromo && montoPromo > 0.01m;

                string estadoPagoMensualidad = EstadoCuentaSolvenciaHelper.EvaluarEstadoPagoMensualidad(meses);
                var mesesPendientesSolvencia = EstadoCuentaSolvenciaHelper.ObtenerMesesPendientes(meses);
                string? textoMesesPendientes = mesesPendientesSolvencia.Count > 0
                    ? string.Join(", ", mesesPendientesSolvencia)
                    : null;
                int? ultimoMesPagado = EstadoCuentaSolvenciaHelper.ObtenerMesHastaPagadoParaMostrar(meses);
                string? nombreUltimoMesPagado = ultimoMesPagado.HasValue
                    ? nombresMes[ultimoMesPagado.Value - 1]
                    : null;

                filas.Add(new EstadoCuentaFilaAlumno
                {
                    IdAlumno = alumno.IdAlumno,
                    NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}".Trim(),
                    NombreGrado = nombreGradoFila,
                    Recinto = recintos.FirstOrDefault(r => r.IdRecinto == idR)?.Recinto,
                    AnioPeriodo = anioFila,
                    MensualidadReferencia = montoMensual,
                    MatriculaReferencia = montoMatNeta,
                    TotalPagadoMatricula = pagadoMat,
                    SaldoMatricula = saldoMat,
                    MatriculaCancelada = matCancelada,
                    TieneAbonoMatricula = tieneAbonoMat,
                    RifaPagada = rifaS1 && rifaS2,
                    RifaSemestre1Pagada = rifaS1,
                    RifaSemestre2Pagada = rifaS2,
                    MontoRifaSemestre1 = montoRifa1,
                    MontoRifaSemestre2 = montoRifa2,
                    AplicaPromocion = aplicaPromo,
                    PromocionPagada = promoPagada,
                    MontoPromocion = montoPromo,
                    Meses = meses,
                    TotalSaldoMensualidades = totalSaldoMeses,
                    GranTotalPendiente = saldoMat + totalSaldoMeses,
                    IdPagoParaEnlace = ultimoPago?.IdPago,
                    SinTarifaMensualidad = costoMen == null && !becaCompleta && !mediaBeca,
                    EstadoPagoMensualidad = estadoPagoMensualidad,
                    MesMensualidadRequerido = mesMensualidadRequerido,
                    NombreMesMensualidadRequerido = nombreMesRequerido,
                    MesesPendientesSolvencia = textoMesesPendientes,
                    UltimoMesPagado = ultimoMesPagado,
                    NombreUltimoMesPagado = nombreUltimoMesPagado
                });
            }

            return filas;
        }

        /// <summary>
        /// Ciclo de la fila: ficha del alumno, o matrícula inscrita/activa (no reserva del siguiente año).
        /// </summary>
        public static (int IdPeriodo, int Anio, TblMatricula? Matricula) ResolverCicloAlumno(
            TblAlumno alumno,
            IReadOnlyList<TblMatricula> matriculasAlumno,
            IReadOnlyList<CatPeriodo>? periodos,
            int idPeriodoFallback,
            int anioFallback)
        {
            var mats = (matriculasAlumno ?? Array.Empty<TblMatricula>())
                .Where(m => m.Activo)
                .OrderByDescending(m => m.IdMatricula)
                .ToList();

            static bool EsMatriculaDeCurso(TblMatricula m)
                => !string.Equals(m.Estado, TblMatricula.EstadoReserva, StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(m.Estado, TblMatricula.EstadoRetirado, StringComparison.OrdinalIgnoreCase);

            int AnioDe(int idPeriodo)
            {
                var p = periodos?.FirstOrDefault(x => x.IdPeriodo == idPeriodo);
                return p is { Periodo: > 0 } ? p.Periodo : anioFallback;
            }

            TblMatricula? MatriculaDe(int idPeriodo)
                => mats.FirstOrDefault(m => m.IdPeriodo == idPeriodo && EsMatriculaDeCurso(m))
                   ?? mats.FirstOrDefault(m => m.IdPeriodo == idPeriodo);

            if (alumno.IdPeriodo is > 0)
            {
                int id = alumno.IdPeriodo.Value;
                return (id, AnioDe(id), MatriculaDe(id) ?? mats.FirstOrDefault(EsMatriculaDeCurso) ?? mats.FirstOrDefault());
            }

            var matCurso = mats.FirstOrDefault(EsMatriculaDeCurso);
            if (matCurso != null)
                return (matCurso.IdPeriodo, AnioDe(matCurso.IdPeriodo), matCurso);

            var matCualquiera = mats.FirstOrDefault();
            if (matCualquiera != null)
                return (matCualquiera.IdPeriodo, AnioDe(matCualquiera.IdPeriodo), matCualquiera);

            return (idPeriodoFallback, anioFallback, null);
        }

        /// <summary>
        /// En catálogo, CostoMatricula es el paquete (matrícula + enero). La neta es paquete − enero
        /// (p. ej. C$ 1300 − C$ 370 = C$ 930).
        /// </summary>
        public static decimal MatriculaNetaDesdePaquete(decimal catalogoPaquete, decimal mensualidadEnero)
        {
            if (catalogoPaquete <= 0m)
                return 0m;
            if (mensualidadEnero > 0m && catalogoPaquete > mensualidadEnero + 0.01m)
                return decimal.Round(catalogoPaquete - mensualidadEnero, 2);
            return catalogoPaquete;
        }

        /// <summary>
        /// Si el dinero en tipo 2/4 + enero supera la matrícula neta, el sobrante se atribuye a enero
        /// (p. ej. pago C$ 1000 con neta C$ 930 → matrícula 930 y abono enero 70).
        /// </summary>
        public static (decimal PagadoMatricula, decimal PagadoEnero) DistribuirPagoMatriculaYEnero(
            decimal catalogoMatricula,
            decimal pagadoTipoMatricula,
            decimal pagadoMensualidadEnero,
            decimal mensualidadEnero = 0m)
        {
            decimal neta = MatriculaNetaDesdePaquete(catalogoMatricula, mensualidadEnero);
            decimal total = pagadoTipoMatricula + pagadoMensualidadEnero;
            if (neta > 0m && total > neta + 0.01m)
            {
                decimal aMat = Math.Min(neta, total);
                return (aMat, total - aMat);
            }

            return (pagadoTipoMatricula, pagadoMensualidadEnero);
        }

        /// <summary>
        /// Cubre primero la matrícula pendiente y el resto se abona a enero.
        /// </summary>
        public static (decimal AMatricula, decimal AEnero) AsignarPagoAMatriculaLuegoEnero(
            decimal faltaMatricula,
            decimal faltaEnero,
            decimal montoPago)
        {
            decimal disponible = Math.Max(0m, montoPago);
            decimal aMat = Math.Min(Math.Max(0m, faltaMatricula), disponible);
            disponible -= aMat;
            decimal aEne = Math.Min(Math.Max(0m, faltaEnero), disponible);
            return (decimal.Round(aMat, 2), decimal.Round(aEne, 2));
        }

        public static (bool Cancelada, decimal SaldoPendiente) CalcularEstadoMatricula(
            decimal montoCatalogo,
            decimal montoMensualidadEnero,
            decimal pagadoMatricula,
            decimal pagadoMensualidadEnero)
        {
            if (montoCatalogo <= 0m)
                return (true, 0m);

            decimal neta = MatriculaNetaDesdePaquete(montoCatalogo, montoMensualidadEnero);
            var (pagadoMat, pagadoEne) = DistribuirPagoMatriculaYEnero(
                montoCatalogo, pagadoMatricula, pagadoMensualidadEnero, montoMensualidadEnero);

            bool cancelada = pagadoMat >= neta - 0.01m
                || pagadoMat >= montoCatalogo - 0.01m
                || (montoMensualidadEnero > 0m
                    && pagadoMat + pagadoEne >= montoCatalogo - 0.01m
                    && pagadoEne >= montoMensualidadEnero - 0.01m);

            if (cancelada)
                return (true, 0m);

            return (false, Math.Max(0m, neta - pagadoMat));
        }

        public static TblCostoMensualidad? ResolverFilaCostoMensualidad(
            IEnumerable<TblCostoMensualidad>? list,
            int? idRecinto,
            int idGrado,
            int idPeriodo,
            int? idModalidad)
        {
            if (list == null || !list.Any())
                return null;

            IEnumerable<TblCostoMensualidad> Base() =>
                list.Where(x => x.IdRecinto == idRecinto &&
                                x.IdPeriodo == idPeriodo &&
                                x.Activo);

            var porGrado = Base().Where(x => x.IdGrado == idGrado).ToList();

            TblCostoMensualidad? EligePorModalidad(IReadOnlyList<TblCostoMensualidad> src)
            {
                if (src.Count == 0) return null;
                if (idModalidad.HasValue && idModalidad.Value > 0)
                {
                    var exacta = src.FirstOrDefault(x => x.IdModalidad == idModalidad.Value);
                    if (exacta != null) return exacta;
                    var wildcard = src.FirstOrDefault(x => x.IdModalidad == 0);
                    if (wildcard != null) return wildcard;
                }
                return src.FirstOrDefault();
            }

            var fila = EligePorModalidad(porGrado);
            if (fila != null)
                return fila;

            if (!idModalidad.HasValue || idModalidad.Value <= 0)
                return null;

            var soloModalidad = Base().Where(x => x.IdModalidad == idModalidad.Value).ToList();
            if (soloModalidad.Count == 1)
                return soloModalidad[0];
            var soloWildcard = Base().Where(x => x.IdModalidad == 0).ToList();
            if (soloWildcard.Count == 1)
                return soloWildcard[0];

            return null;
        }

        public static TblCostoMatricula? ResolverFilaCostoMatricula(
            IEnumerable<TblCostoMatricula>? list,
            int? idRecinto,
            int idPeriodo,
            int? idModalidad)
        {
            if (list == null || !list.Any())
                return null;

            var candidatas = list
                .Where(x => x.IdRecinto == idRecinto && x.IdPeriodo == idPeriodo && x.Activo)
                .ToList();
            if (candidatas.Count == 0)
                return null;

            if (idModalidad.HasValue && idModalidad.Value > 0)
            {
                var exacta = candidatas.FirstOrDefault(x => x.IdModalidad == idModalidad.Value);
                if (exacta != null)
                    return exacta;
                var wildcard = candidatas.FirstOrDefault(x => x.IdModalidad == 0);
                if (wildcard != null)
                    return wildcard;
            }

            return candidatas.FirstOrDefault();
        }

        public const string MarcaAlumnoPagoCaja = "[ALUMNO:";
        public const string MarcaRifaSemestre = "[RIFA:";

        public static bool EsGradoConPromocion(string? nombreGrado)
        {
            var n = NormalizarNombrePersona(nombreGrado);
            if (string.IsNullOrEmpty(n))
                return false;

            if (n.Contains("undecimo") || n.Contains("11mo") || n.Contains("11°")
                || Regex.IsMatch(n, @"\b11\b"))
                return true;

            if (n.Contains("sexto") || n.Contains("6to") || n.Contains("6°")
                || Regex.IsMatch(n, @"\b6\s*(to|grado)\b"))
                return true;

            bool esTercer = n.Contains("tercer") || n.Contains("3er") || n.Contains("iii");
            bool esNivel = n.Contains("nivel");
            return esTercer && esNivel && !n.Contains("grado");
        }

        public static int SemestreRifa(int? mes, DateTime? fechaEmision, DateTime? fechaRegistro, string? descripcion)
        {
            if (!string.IsNullOrWhiteSpace(descripcion))
            {
                var marca = Regex.Match(descripcion, @"\[RIFA:([12])\]", RegexOptions.IgnoreCase);
                if (marca.Success && int.TryParse(marca.Groups[1].Value, out var semMarca))
                    return semMarca;
                var d = NormalizarNombrePersona(descripcion);
                if (d.Contains("segundo semestre") || d.Contains("2do semestre") || d.Contains("2o semestre"))
                    return 2;
                if (d.Contains("primer semestre") || d.Contains("1er semestre") || d.Contains("1er sem"))
                    return 1;
            }

            if (mes is >= 1 and <= 6)
                return 1;
            if (mes is >= 7 and <= 12)
                return 2;

            var fecha = fechaEmision ?? fechaRegistro;
            if (fecha.HasValue && fecha.Value != default)
                return fecha.Value.Month <= 6 ? 1 : 2;

            return DateTime.Now.Month <= 6 ? 1 : 2;
        }

        public static int SemestreRifaDePago(TblPago pago)
            => SemestreRifa(pago.IdMes, pago.FechaEmision, pago.FechaRegistro, pago.Descripcion);

        public static int SemestreRifaDeCaja(TblPagoCaja caja)
            => SemestreRifa(null, caja.FechaEmision, caja.FechaRegistro, caja.Descripcion);

        /// <summary>
        /// Las rifas se cubren en orden: el primer pago cancela el 1.er semestre;
        /// el segundo cancela el 2.º. No importa la fecha del recibo.
        /// </summary>
        public static (decimal Rifa1, decimal Rifa2) MontosRifaPorOrden(
            IReadOnlyList<TblPago> pagosRifa,
            IReadOnlyList<TblPagoCaja> cajaRifa)
        {
            var lineas = new List<(DateTime Fecha, int Id, decimal Monto)>();
            foreach (var p in pagosRifa ?? Array.Empty<TblPago>())
            {
                var fecha = p.FechaRegistro != default ? p.FechaRegistro : (p.FechaEmision ?? DateTime.MinValue);
                lineas.Add((fecha, p.IdPago, p.Monto));
            }
            foreach (var c in cajaRifa ?? Array.Empty<TblPagoCaja>())
            {
                var fecha = c.FechaRegistro != default ? c.FechaRegistro : (c.FechaEmision ?? DateTime.MinValue);
                lineas.Add((fecha, c.IdPagoCaja, c.Monto));
            }

            var montos = lineas.OrderBy(x => x.Fecha).ThenBy(x => x.Id).Select(x => x.Monto).ToList();
            return (
                montos.Count > 0 ? montos[0] : 0m,
                montos.Count > 1 ? montos[1] : 0m);
        }

        public static (bool Semestre1, bool Semestre2) EstadoRifasPorOrden(int cantidadPagosRifa)
        {
            if (cantidadPagosRifa <= 0)
                return (false, false);
            if (cantidadPagosRifa == 1)
                return (true, false);
            return (true, true);
        }

        public static int SiguienteSemestreRifa(int cantidadPagosRifaExistentes)
        {
            if (cantidadPagosRifaExistentes <= 0)
                return 1;
            if (cantidadPagosRifaExistentes == 1)
                return 2;
            return 0;
        }

        public static bool EsReciboRifa(
            TblPagoCaja caja,
            IReadOnlyCollection<int> tiposRifa,
            IEnumerable<CatTipoMovimiento>? tipos = null)
        {
            if (tiposRifa.Contains(caja.Concepto))
                return true;
            var nombreTipo = tipos?.FirstOrDefault(t => t.IdTipoMovimiento == caja.Concepto)?.Concepto;
            if (!string.IsNullOrWhiteSpace(nombreTipo)
                && nombreTipo.Contains("rifa", StringComparison.OrdinalIgnoreCase))
                return true;
            var texto = $"{caja.Descripcion}";
            return texto.Contains("rifa", StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsReciboPromocion(
            TblPagoCaja caja,
            IReadOnlyCollection<int> tiposPromo,
            IEnumerable<CatTipoMovimiento>? tipos = null)
        {
            if (tiposPromo.Count > 0 && tiposPromo.Contains(caja.Concepto))
                return true;
            var nombreTipo = tipos?.FirstOrDefault(t => t.IdTipoMovimiento == caja.Concepto)?.Concepto ?? "";
            if (NormalizarNombrePersona(nombreTipo).Contains("promoc"))
                return true;
            return NormalizarNombrePersona(caja.Descripcion).Contains("promoc");
        }

        public static string AdjuntarMarcaRifa(string? descripcion, int semestre)
        {
            if (semestre != 1 && semestre != 2)
                return descripcion ?? string.Empty;
            var marca = $"{MarcaRifaSemestre}{semestre}]";
            if (string.IsNullOrWhiteSpace(descripcion))
                return marca;
            if (descripcion.Contains(MarcaRifaSemestre, StringComparison.OrdinalIgnoreCase))
                return descripcion;
            return marca + " " + descripcion.Trim();
        }

        public static string QuitarMarcasInternasDescripcion(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return string.Empty;
            var limpio = Regex.Replace(descripcion, @"\[ALUMNO:\d+\]", "", RegexOptions.IgnoreCase);
            limpio = Regex.Replace(limpio, @"\[RIFA:[12]\]", "", RegexOptions.IgnoreCase);
            limpio = Regex.Replace(limpio, @"\s+", " ").Trim();
            return limpio.Trim(' ', ',', ';', '-', '·');
        }

        public static string TextoLineasProducto(IEnumerable<(int Cantidad, string Nombre)> lineas)
        {
            if (lineas == null)
                return string.Empty;
            return string.Join(", ", lineas
                .Where(x => x.Cantidad > 0 && !string.IsNullOrWhiteSpace(x.Nombre))
                .Select(x => $"{x.Cantidad} {x.Nombre.Trim()}"));
        }

        public static string CombinarDescripcionVisible(string? observaciones, string? detalleProductos)
        {
            var obs = QuitarMarcasInternasDescripcion(observaciones);
            var prod = (detalleProductos ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(prod))
                return obs;
            if (string.IsNullOrEmpty(obs))
                return prod;
            return $"{prod}. {obs}";
        }

        public static string AdjuntarMarcaAlumno(string? descripcion, int idAlumno)
        {
            if (idAlumno <= 0)
                return descripcion ?? string.Empty;
            var marca = $"{MarcaAlumnoPagoCaja}{idAlumno}]";
            if (string.IsNullOrWhiteSpace(descripcion))
                return marca;
            if (descripcion.Contains(MarcaAlumnoPagoCaja, StringComparison.OrdinalIgnoreCase))
                return descripcion;
            return marca + " " + descripcion.Trim();
        }

        public static int? LeerIdAlumnoPagoCaja(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return null;
            var match = Regex.Match(descripcion, @"\[ALUMNO:(\d+)\]", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            return int.TryParse(match.Groups[1].Value, out var id) && id > 0 ? id : null;
        }

        public static IEnumerable<TblPagoCaja> RecibosCajaDelAlumno(
            IReadOnlyList<TblPagoCaja>? pagosCaja,
            TblAlumno alumno,
            int idPeriodo,
            int anioPeriodo = 0)
        {
            if (pagosCaja == null || pagosCaja.Count == 0)
                return Array.Empty<TblPagoCaja>();

            var nombreAlumno = NormalizarNombrePersona($"{alumno.Nombre} {alumno.Apellido}");
            var nombreInvertido = NormalizarNombrePersona($"{alumno.Apellido} {alumno.Nombre}");

            return pagosCaja.Where(c =>
            {
                if (!c.Activo && c.UsuarioActualizo.HasValue)
                    return false;
                if (!ReciboCajaDelMismoCiclo(c, idPeriodo, anioPeriodo))
                    return false;

                var idMarca = LeerIdAlumnoPagoCaja(c.Descripcion);
                if (idMarca == alumno.IdAlumno)
                    return true;

                var nombreCaja = NormalizarNombrePersona(c.Nombre);
                if (string.IsNullOrEmpty(nombreCaja) || string.IsNullOrEmpty(nombreAlumno))
                    return false;

                if (nombreCaja == nombreAlumno || nombreCaja == nombreInvertido)
                    return true;

                var tokens = nombreAlumno.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return tokens.Length >= 2 && tokens.All(t => t.Length > 1 && nombreCaja.Contains(t));
            });
        }

        private static bool ReciboCajaDelMismoCiclo(TblPagoCaja caja, int idPeriodo, int anioPeriodo)
        {
            if (caja.IdPeriodo <= 0)
                return true;
            if (idPeriodo > 0 && caja.IdPeriodo == idPeriodo)
                return true;

            int anioRecibo = caja.Anyo ?? 0;
            var fecha = caja.FechaEmision ?? caja.FechaRegistro;
            if (anioRecibo <= 0 && fecha != default)
                anioRecibo = fecha.Year;
            if (anioRecibo <= 0)
                return true;

            int anioMin = anioPeriodo > 0 ? anioPeriodo - 1 : DateTime.Now.Year - 1;
            int anioMax = Math.Max(anioPeriodo > 0 ? anioPeriodo : DateTime.Now.Year, DateTime.Now.Year);
            return anioRecibo >= anioMin && anioRecibo <= anioMax;
        }

        private static string NormalizarNombrePersona(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;
            var n = texto.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(n.Length);
            foreach (var ch in n)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.ToLowerInvariant(ch));
            }
            return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }
    }
}
