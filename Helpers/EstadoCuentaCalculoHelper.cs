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
        /// <summary>Catálogo: 4 = Confirmación / reserva. Si quedó en el mismo ciclo, abona esa matrícula.</summary>
        public const int TipoConfirmacionMatricula = 4;
        /// <summary>Catálogo: 18 = Abono de Matrícula. El 4 es Confirmación Matrícula.</summary>
        public const int TipoMatriculaAbono = 18;
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
            var tiposMatriculaPago = TiposPagoMatricula(tiposMovimiento);
            if (tiposMovimiento != null)
            {
                foreach (var id in IdsPorConcepto(tiposMovimiento, "rifa", "rifas"))
                    tiposRifa.Add(id);
                foreach (var id in IdsPorConcepto(tiposMovimiento, "promoc"))
                    tiposPromo.Add(id);
            }
            var cortesRifa = ConstruirCortesGeneracionRifa(
                pagos, pagosCaja, tiposRifa, tiposMovimiento);
            var tarifasRifa = ConstruirTarifasConcepto(RecopilarMontosConcepto(
                pagos, pagosCaja, tiposRifa, tiposMovimiento, esRifa: true));
            var tarifasPromo = ConstruirTarifasConcepto(RecopilarMontosConcepto(
                pagos, pagosCaja, tiposPromo, tiposMovimiento, esRifa: false));
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
                bool esRetiradoOInactivo = EsAlumnoRetiradoOInactivo(alumno, matriculaAlum);
                string? etiquetaEstadoAlumno = EtiquetaEstadoAlumno(alumno, matriculaAlum);

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
                        EsRetiradoOInactivo = esRetiradoOInactivo,
                        EtiquetaEstadoAlumno = etiquetaEstadoAlumno,
                        NoAplicaMatricula = esRetiradoOInactivo,
                        MotivoIncompleto = "Falta grado, recinto o modalidad en la matrícula del ciclo o en la ficha."
                    });
                    continue;
                }

                bool becaCompleta = (matriculaAlum?.BecaCompleta == true || alumno.BecaCompleta == true)
                    && (alumno.IdPeriodo == idPeriodoFila || matriculaAlum != null);
                bool mediaBeca = !becaCompleta
                    && (matriculaAlum?.MediaBeca == true || alumno.MediaBeca == true)
                    && (alumno.IdPeriodo == idPeriodoFila || matriculaAlum != null);

                var costoMen = ResolverFilaCostoMensualidad(costosMen, idR, idG.Value, idPeriodoFila, idMod, grados);
                decimal montoMensual = costoMen != null ? (decimal)costoMen.CostoMensualidad : 0m;
                if (becaCompleta)
                    montoMensual = 0m;
                else if (mediaBeca && montoMensual > 0)
                    montoMensual *= 0.5m;

                var costoMat = esRetiradoOInactivo
                    ? null
                    : ResolverFilaCostoMatricula(costosMat, idR, idPeriodoFila, idMod);
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
                pagadoMatBruto += AcreditarReservaSiguienteSiMatriculaPendiente(
                    pagos, alumno.IdAlumno, idPeriodoFila, anioFila, periodos,
                    TiposPagoConfirmacion(tiposMovimiento), pagadoMatBruto, montoMatNeta, idR);
                var (pagadoMat, pagadoEneroConMatricula) = DistribuirPagoMatriculaYEnero(
                    montoMat, pagadoMatBruto, pagadoEneroBruto, montoMensual);

                bool esTraslado = TrasladoHelper.EsTraslado(matriculaAlum, alumno);
                int? mesIngreso = ResolverMesInicioMensualidad(
                    matriculaAlum, alumno, pagos, tiposMatriculaPago, idPeriodoFila, idR);
                int? idRecintoOrigen = esTraslado
                    ? TrasladoHelper.LeerRecintoOrigen(matriculaAlum?.Observaciones)
                      ?? TrasladoHelper.LeerRecintoOrigen(alumno.Observaciones)
                    : null;
                DateTime? fechaTraslado = esTraslado
                    ? TrasladoHelper.LeerFechaTraslado(matriculaAlum?.Observaciones)
                      ?? TrasladoHelper.LeerFechaTraslado(alumno.Observaciones)
                    : null;
                if (idRecintoOrigen is > 0 && idRecintoOrigen.Value == idR)
                    idRecintoOrigen = null;

                var pagosDestino = FiltrarPagosRecinto(pagosAlum, idR.Value, idRecintoOrigen, mesIngreso, fechaTraslado, esOrigen: false);
                var pagosOrigen = idRecintoOrigen is > 0
                    ? FiltrarPagosRecinto(pagosAlum, idRecintoOrigen.Value, idRecintoOrigen, mesIngreso, fechaTraslado, esOrigen: true)
                    : new List<TblPago>();

                decimal pagadoMatDestino = pagos.Where(p =>
                    PagoEsMatriculaOAbono(p) && PerteneceRecintoPago(p, idR.Value, idRecintoOrigen, mesIngreso, fechaTraslado, false)).Sum(p => p.Monto);
                decimal reservaSiguiente = AcreditarReservaSiguienteSiMatriculaPendiente(
                    pagos, alumno.IdAlumno, idPeriodoFila, anioFila, periodos,
                    TiposPagoConfirmacion(tiposMovimiento), pagadoMatDestino, montoMatNeta, idR);
                pagadoMatDestino += reservaSiguiente;
                decimal pagadoEneroDestino = pagosDestino
                    .Where(p => tiposMes.Contains(p.IdTipoMovimiento) && p.IdMes == 1)
                    .Sum(p => p.Monto);
                var (pagadoMatDest, pagadoEneroDest) = DistribuirPagoMatriculaYEnero(
                    montoMat, pagadoMatDestino, pagadoEneroDestino, montoMensual);
                pagadoMat = pagadoMatDest;
                pagadoEneroConMatricula = pagadoEneroDest;

                var estadoMat = CalcularEstadoMatricula(montoMat, montoMensual, pagadoMat, pagadoEneroConMatricula);
                bool matCancelada = estadoMat.Cancelada;
                decimal saldoMat = matCancelada ? 0m : estadoMat.SaldoPendiente;
                bool tieneAbonoMat = !matCancelada
                    && (pagosDestino.Any(p => tiposMatriculaPago.Contains(p.IdTipoMovimiento))
                        || reservaSiguiente > 0.01m);
                if (esRetiradoOInactivo)
                {
                    montoMatNeta = 0m;
                    matCancelada = false;
                    saldoMat = 0m;
                    tieneAbonoMat = false;
                }

                    var meses = ConstruirMesesRecinto(
                    pagosDestino, tiposMes, pagadoEneroConMatricula, montoMensual, nombresMes,
                    mesIngreso, esHistorial: false);
                decimal totalSaldoMeses = meses.Where(c => !c.NoCorresponde && !c.Cancelado).Sum(c => c.Saldo);

                var ultimoPago = pagosDestino.OrderByDescending(p => p.IdPago).FirstOrDefault()
                    ?? pagosAlum.OrderByDescending(p => p.IdPago).FirstOrDefault();
                var cajaAlum = RecibosCajaDelAlumno(pagosCaja, alumno, idPeriodoFila, anioFila).ToList();
                string nombreGradoFila = grados.FirstOrDefault(g => g.IdGrado == idG)?.NombreGrado ?? string.Empty;
                bool aplicaPromo = EsGradoConPromocion(nombreGradoFila);
                var evalRifa = EvaluarRifaAlumno(
                    alumno,
                    matriculaAlum,
                    pagos,
                    pagosCaja,
                    tiposRifa,
                    tiposMatriculaPago,
                    tiposMovimiento,
                    idPeriodoFila,
                    anioFila,
                    idR,
                    cortesRifa,
                    fechaTraslado);
                decimal montoRifa1 = evalRifa.MontoRifa1;
                decimal montoRifa2 = evalRifa.MontoRifa2;
                decimal tarifaRifa = ResolverTarifaConcepto(tarifasRifa, idPeriodoFila, idR);
                var saldoRifa1 = EvaluarSaldoConcepto(evalRifa.AplicaRifa1, tarifaRifa, montoRifa1);
                var saldoRifa2 = EvaluarSaldoConcepto(evalRifa.AplicaRifa2, tarifaRifa, montoRifa2);
                decimal montoPromo = 0m;
                if (aplicaPromo)
                {
                    if (tiposPromo.Count > 0)
                        montoPromo += pagosAlum.Where(p => tiposPromo.Contains(p.IdTipoMovimiento)).Sum(p => p.Monto);
                    montoPromo += cajaAlum.Where(c => EsReciboPromocion(c, tiposPromo, tiposMovimiento)).Sum(c => c.Monto);
                }
                decimal tarifaPromo = TarifaPromocionPorGrado(nombreGradoFila);
                if (tarifaPromo <= 0.01m)
                    tarifaPromo = ResolverTarifaConcepto(tarifasPromo, idPeriodoFila, idR);
                var saldoPromo = EvaluarSaldoConcepto(aplicaPromo, tarifaPromo, montoPromo);

                string estadoPagoMensualidad = EstadoCuentaSolvenciaHelper.EvaluarEstadoPagoMensualidad(meses);
                var mesesPendientesSolvencia = EstadoCuentaSolvenciaHelper.ObtenerMesesPendientes(meses);
                string? textoMesesPendientes = mesesPendientesSolvencia.Count > 0
                    ? string.Join(", ", mesesPendientesSolvencia)
                    : null;
                int? ultimoMesPagado = EstadoCuentaSolvenciaHelper.ObtenerMesHastaPagadoParaMostrar(meses);
                string? nombreUltimoMesPagado = ultimoMesPagado.HasValue
                    ? nombresMes[ultimoMesPagado.Value - 1]
                    : null;

                bool aplicaRifa1 = evalRifa.AplicaRifa1;
                bool aplicaRifa2 = evalRifa.AplicaRifa2;
                if (esRetiradoOInactivo)
                {
                    montoMensual = 0m;
                    montoMatNeta = 0m;
                    pagadoMat = 0m;
                    saldoMat = 0m;
                    matCancelada = false;
                    tieneAbonoMat = false;
                    meses = MesesSinCobro(nombresMes);
                    totalSaldoMeses = 0m;
                    aplicaPromo = false;
                    aplicaRifa1 = false;
                    aplicaRifa2 = false;
                    tarifaRifa = 0m;
                    tarifaPromo = 0m;
                    montoRifa1 = 0m;
                    montoRifa2 = 0m;
                    montoPromo = 0m;
                    saldoRifa1 = default;
                    saldoRifa2 = default;
                    saldoPromo = default;
                    estadoPagoMensualidad = null;
                    textoMesesPendientes = null;
                    ultimoMesPagado = null;
                    nombreUltimoMesPagado = null;
                }

                filas.Add(new EstadoCuentaFilaAlumno
                {
                    IdAlumno = alumno.IdAlumno,
                    NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}".Trim(),
                    NombreGrado = nombreGradoFila,
                    Recinto = recintos.FirstOrDefault(r => r.IdRecinto == idR)?.Recinto,
                    IdRecinto = idR,
                    EsHistorialTraslado = false,
                    EtiquetaRecinto = idRecintoOrigen is > 0 ? "Actual" : null,
                    AnioPeriodo = anioFila,
                    EsRetiradoOInactivo = esRetiradoOInactivo,
                    EtiquetaEstadoAlumno = etiquetaEstadoAlumno,
                    NoAplicaMatricula = esRetiradoOInactivo,
                    MensualidadReferencia = montoMensual,
                    MatriculaReferencia = montoMatNeta,
                    TotalPagadoMatricula = pagadoMat,
                    SaldoMatricula = saldoMat,
                    MatriculaCancelada = matCancelada,
                    TieneAbonoMatricula = tieneAbonoMat,
                    RifaPagada = (!aplicaRifa1 || saldoRifa1.Cancelado)
                        && (!aplicaRifa2 || saldoRifa2.Cancelado),
                    RifaSemestre1Pagada = saldoRifa1.Cancelado,
                    RifaSemestre2Pagada = saldoRifa2.Cancelado,
                    MontoRifaSemestre1 = montoRifa1,
                    MontoRifaSemestre2 = montoRifa2,
                    RifaReferencia = tarifaRifa,
                    SaldoRifaSemestre1 = saldoRifa1.Saldo,
                    SaldoRifaSemestre2 = saldoRifa2.Saldo,
                    TieneAbonoRifaSemestre1 = saldoRifa1.AbonoParcial,
                    TieneAbonoRifaSemestre2 = saldoRifa2.AbonoParcial,
                    AplicaRifaSemestre1 = aplicaRifa1,
                    AplicaRifaSemestre2 = aplicaRifa2,
                    MotivoRifaSemestre1 = aplicaRifa1 ? null : (esRetiradoOInactivo ? "Retirado o bloqueado: no se generan cobros" : evalRifa.MotivoNoAplica(1)),
                    MotivoRifaSemestre2 = aplicaRifa2 ? null : (esRetiradoOInactivo ? "Retirado o bloqueado: no se generan cobros" : evalRifa.MotivoNoAplica(2)),
                    AplicaPromocion = aplicaPromo,
                    PromocionPagada = saldoPromo.Cancelado,
                    PromocionReferencia = saldoPromo.Esperado,
                    MontoPromocion = montoPromo,
                    SaldoPromocion = saldoPromo.Saldo,
                    TieneAbonoPromocion = saldoPromo.AbonoParcial,
                    Meses = meses,
                    TotalSaldoMensualidades = totalSaldoMeses,
                    GranTotalPendiente = esRetiradoOInactivo ? 0m : saldoMat + totalSaldoMeses + saldoRifa1.Saldo + saldoRifa2.Saldo + saldoPromo.Saldo,
                    IdPagoParaEnlace = ultimoPago?.IdPago,
                    SinTarifaMensualidad = !esRetiradoOInactivo && costoMen == null && !becaCompleta && !mediaBeca,
                    EstadoPagoMensualidad = estadoPagoMensualidad,
                    MesMensualidadRequerido = mesMensualidadRequerido,
                    NombreMesMensualidadRequerido = nombreMesRequerido,
                    MesesPendientesSolvencia = textoMesesPendientes,
                    UltimoMesPagado = ultimoMesPagado,
                    NombreUltimoMesPagado = nombreUltimoMesPagado
                });

                if (idRecintoOrigen is > 0)
                {
                    var costoMenOrig = ResolverFilaCostoMensualidad(costosMen, idRecintoOrigen, idG.Value, idPeriodoFila, idMod, grados);
                    decimal montoMensualOrig = costoMenOrig != null ? (decimal)costoMenOrig.CostoMensualidad : 0m;
                    if (becaCompleta)
                        montoMensualOrig = 0m;
                    else if (mediaBeca && montoMensualOrig > 0)
                        montoMensualOrig *= 0.5m;
                    var costoMatOrig = esRetiradoOInactivo
                        ? null
                        : ResolverFilaCostoMatricula(costosMat, idRecintoOrigen, idPeriodoFila, idMod);
                    decimal montoMatOrig = costoMatOrig != null ? (decimal)costoMatOrig.CostoMatricula : 0m;
                    decimal pagadoMatOrigBruto = pagos.Where(p =>
                        PagoEsMatriculaOAbono(p)
                        && PerteneceRecintoPago(p, idRecintoOrigen.Value, idRecintoOrigen, mesIngreso, fechaTraslado, true)).Sum(p => p.Monto);
                    decimal pagadoEneroOrigBruto = pagosOrigen
                        .Where(p => tiposMes.Contains(p.IdTipoMovimiento) && p.IdMes == 1)
                        .Sum(p => p.Monto);
                    decimal montoMatNetaOrig = MatriculaNetaDesdePaquete(montoMatOrig, montoMensualOrig);
                    pagadoMatOrigBruto += AcreditarReservaSiguienteSiMatriculaPendiente(
                        pagos, alumno.IdAlumno, idPeriodoFila, anioFila, periodos,
                        TiposPagoConfirmacion(tiposMovimiento), pagadoMatOrigBruto, montoMatNetaOrig, idRecintoOrigen);
                    var (pagadoMatOrig, pagadoEneroOrig) = DistribuirPagoMatriculaYEnero(
                        montoMatOrig, pagadoMatOrigBruto, pagadoEneroOrigBruto, montoMensualOrig);
                    var estadoMatOrig = CalcularEstadoMatricula(montoMatOrig, montoMensualOrig, pagadoMatOrig, pagadoEneroOrig);
                    var mesesOrig = ConstruirMesesRecinto(
                        pagosOrigen, tiposMes, pagadoEneroOrig, montoMensualOrig, nombresMes,
                        mesIngreso, esHistorial: true);
                    decimal totalSaldoOrig = mesesOrig.Where(c => !c.NoCorresponde && !c.Cancelado).Sum(c => c.Saldo);
                    var estadoOrig = EstadoCuentaSolvenciaHelper.EvaluarEstadoPagoMensualidad(mesesOrig);
                    var pendOrig = EstadoCuentaSolvenciaHelper.ObtenerMesesPendientes(mesesOrig);
                    int? ultimoOrig = EstadoCuentaSolvenciaHelper.ObtenerMesHastaPagadoParaMostrar(mesesOrig);
                    var evalRifaOrig = EvaluarRifaAlumno(
                        alumno,
                        matriculaAlum,
                        pagos,
                        pagosCaja,
                        tiposRifa,
                        tiposMatriculaPago,
                        tiposMovimiento,
                        idPeriodoFila,
                        anioFila,
                        idRecintoOrigen,
                        cortesRifa,
                        fechaIngresoOverride: null);
                    decimal montoRifa1o = evalRifaOrig.MontoRifa1;
                    decimal montoRifa2o = evalRifaOrig.MontoRifa2;
                    decimal tarifaRifaOrig = ResolverTarifaConcepto(tarifasRifa, idPeriodoFila, idRecintoOrigen);
                    var saldoRifa1o = EvaluarSaldoConcepto(evalRifaOrig.AplicaRifa1, tarifaRifaOrig, montoRifa1o);
                    var saldoRifa2o = EvaluarSaldoConcepto(evalRifaOrig.AplicaRifa2, tarifaRifaOrig, montoRifa2o);
                    decimal montoPromoOrig = 0m;
                    if (aplicaPromo && tiposPromo.Count > 0)
                        montoPromoOrig = pagosOrigen.Where(p => tiposPromo.Contains(p.IdTipoMovimiento)).Sum(p => p.Monto);
                    decimal tarifaPromoOrig = TarifaPromocionPorGrado(nombreGradoFila);
                    if (tarifaPromoOrig <= 0.01m)
                        tarifaPromoOrig = ResolverTarifaConcepto(tarifasPromo, idPeriodoFila, idRecintoOrigen);
                    var saldoPromoOrig = EvaluarSaldoConcepto(aplicaPromo, tarifaPromoOrig, montoPromoOrig);
                    decimal saldoMatOrig = esRetiradoOInactivo || estadoMatOrig.Cancelada ? 0m : estadoMatOrig.SaldoPendiente;
                    decimal montoMatNetaOrigMostrar = esRetiradoOInactivo ? 0m : montoMatNetaOrig;
                    if (esRetiradoOInactivo)
                    {
                        montoMensualOrig = 0m;
                        mesesOrig = MesesSinCobro(nombresMes);
                        totalSaldoOrig = 0m;
                        saldoMatOrig = 0m;
                        montoMatNetaOrigMostrar = 0m;
                        saldoRifa1o = default;
                        saldoRifa2o = default;
                        saldoPromoOrig = default;
                        tarifaRifaOrig = 0m;
                        tarifaPromoOrig = 0m;
                        montoRifa1o = 0m;
                        montoRifa2o = 0m;
                        montoPromoOrig = 0m;
                        estadoOrig = string.Empty;
                        pendOrig = new List<string>();
                        ultimoOrig = null;
                    }

                    filas.Add(new EstadoCuentaFilaAlumno
                    {
                        IdAlumno = alumno.IdAlumno,
                        NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}".Trim(),
                        NombreGrado = nombreGradoFila,
                        Recinto = recintos.FirstOrDefault(r => r.IdRecinto == idRecintoOrigen)?.Recinto,
                        IdRecinto = idRecintoOrigen,
                        EsHistorialTraslado = true,
                        EtiquetaRecinto = "Historial",
                        AnioPeriodo = anioFila,
                        EsRetiradoOInactivo = esRetiradoOInactivo,
                        EtiquetaEstadoAlumno = etiquetaEstadoAlumno,
                        NoAplicaMatricula = esRetiradoOInactivo,
                        MensualidadReferencia = montoMensualOrig,
                        MatriculaReferencia = montoMatNetaOrigMostrar,
                        TotalPagadoMatricula = esRetiradoOInactivo ? 0m : pagadoMatOrig,
                        SaldoMatricula = saldoMatOrig,
                        MatriculaCancelada = !esRetiradoOInactivo && estadoMatOrig.Cancelada,
                        TieneAbonoMatricula = !esRetiradoOInactivo && !estadoMatOrig.Cancelada && pagadoMatOrig > 0.01m,
                        RifaPagada = !esRetiradoOInactivo
                            && (!evalRifaOrig.AplicaRifa1 || saldoRifa1o.Cancelado)
                            && (!evalRifaOrig.AplicaRifa2 || saldoRifa2o.Cancelado),
                        RifaSemestre1Pagada = !esRetiradoOInactivo && saldoRifa1o.Cancelado,
                        RifaSemestre2Pagada = !esRetiradoOInactivo && saldoRifa2o.Cancelado,
                        MontoRifaSemestre1 = montoRifa1o,
                        MontoRifaSemestre2 = montoRifa2o,
                        RifaReferencia = tarifaRifaOrig,
                        SaldoRifaSemestre1 = saldoRifa1o.Saldo,
                        SaldoRifaSemestre2 = saldoRifa2o.Saldo,
                        TieneAbonoRifaSemestre1 = saldoRifa1o.AbonoParcial,
                        TieneAbonoRifaSemestre2 = saldoRifa2o.AbonoParcial,
                        AplicaRifaSemestre1 = !esRetiradoOInactivo && evalRifaOrig.AplicaRifa1,
                        AplicaRifaSemestre2 = !esRetiradoOInactivo && evalRifaOrig.AplicaRifa2,
                        MotivoRifaSemestre1 = esRetiradoOInactivo ? "Retirado o bloqueado: no se generan cobros" : (evalRifaOrig.AplicaRifa1 ? null : evalRifaOrig.MotivoNoAplica(1)),
                        MotivoRifaSemestre2 = esRetiradoOInactivo ? "Retirado o bloqueado: no se generan cobros" : (evalRifaOrig.AplicaRifa2 ? null : evalRifaOrig.MotivoNoAplica(2)),
                        AplicaPromocion = !esRetiradoOInactivo && aplicaPromo,
                        PromocionPagada = !esRetiradoOInactivo && saldoPromoOrig.Cancelado,
                        PromocionReferencia = saldoPromoOrig.Esperado,
                        MontoPromocion = montoPromoOrig,
                        SaldoPromocion = saldoPromoOrig.Saldo,
                        TieneAbonoPromocion = saldoPromoOrig.AbonoParcial,
                        Meses = mesesOrig,
                        TotalSaldoMensualidades = totalSaldoOrig,
                        GranTotalPendiente = esRetiradoOInactivo ? 0m : saldoMatOrig + totalSaldoOrig + saldoRifa1o.Saldo + saldoRifa2o.Saldo + saldoPromoOrig.Saldo,
                        IdPagoParaEnlace = pagosOrigen.OrderByDescending(p => p.IdPago).FirstOrDefault()?.IdPago,
                        SinTarifaMensualidad = !esRetiradoOInactivo && costoMenOrig == null && !becaCompleta && !mediaBeca,
                        EstadoPagoMensualidad = estadoOrig,
                        MesMensualidadRequerido = mesMensualidadRequerido,
                        NombreMesMensualidadRequerido = nombreMesRequerido,
                        MesesPendientesSolvencia = pendOrig.Count > 0 ? string.Join(", ", pendOrig) : null,
                        UltimoMesPagado = ultimoOrig,
                        NombreUltimoMesPagado = ultimoOrig.HasValue ? nombresMes[ultimoOrig.Value - 1] : null
                    });
                }
            }

            return filas;
        }

        /// <summary>
        /// Primer mes que se cobra de mensualidad. Traslado: mes de ingreso al destino.
        /// Matrícula tardía (feb–sep): el mes de la ficha o del primer pago de matrícula.
        /// Oct–dic o continuidad: el ciclo completo desde enero.
        /// </summary>
        public static int? ResolverMesInicioMensualidad(
            TblMatricula? matricula,
            TblAlumno? alumno,
            IEnumerable<TblPago>? pagos = null,
            IReadOnlyCollection<int>? tiposMatricula = null,
            int idPeriodo = 0,
            int? idRecinto = null)
        {
            if (TrasladoHelper.EsTraslado(matricula, alumno))
            {
                var mesTraslado = TrasladoHelper.ResolverMesIngreso(matricula, alumno);
                if (mesTraslado is >= 1 and <= 12)
                    return mesTraslado;
            }

            if (matricula?.Continuidad == true || EsTipoContinuidad(matricula?.TipoEstudiante) || EsTipoContinuidad(alumno?.TipoEstudiante))
                return null;

            if (matricula?.FechaMatricula is { Year: > 2000 } fechaAcad)
            {
                if (fechaAcad.Month >= 10)
                    return null;
                if (fechaAcad.Month >= 2)
                    return fechaAcad.Month;
            }

            if (pagos != null && tiposMatricula is { Count: > 0 } && alumno != null)
            {
                var pagoMat = pagos
                    .Where(p => p.Activo
                        && p.IdAlumno == alumno.IdAlumno
                        && tiposMatricula.Contains(p.IdTipoMovimiento)
                        && (idPeriodo <= 0 || p.IdPeriodo == idPeriodo)
                        && EsPagoDelRecinto(p, idRecinto))
                    .OrderBy(p => p.FechaEmision ?? p.FechaRegistro)
                    .ThenBy(p => p.IdPago)
                    .FirstOrDefault();
                if (pagoMat != null)
                {
                    if (pagoMat.Continuidad)
                        return null;
                    var mesPago = (pagoMat.FechaEmision ?? pagoMat.FechaRegistro).Month;
                    if (mesPago >= 10)
                        return null;
                    if (mesPago >= 2)
                        return mesPago;
                }
            }

            if (matricula?.FechaRegistro is { Year: > 2000 } fechaReg
                && fechaReg.Month is >= 2 and <= 9)
                return fechaReg.Month;

            return null;
        }

        private static bool EsTipoContinuidad(string? tipoEstudiante)
        {
            if (string.IsNullOrWhiteSpace(tipoEstudiante))
                return false;
            var t = tipoEstudiante.Trim().ToLowerInvariant();
            return t.Contains("continuidad") || t.Contains("reintegro") || t.Contains("reingreso");
        }

        /// <summary>
        /// Un pago con recinto informado solo cuenta en ese colegio.
        /// Sin recinto (recibos viejos) se atribuye al colegio actual, salvo traslado
        /// con mes de ingreso, que reparte por mes.
        /// </summary>
        public static bool EsPagoDelRecinto(TblPago p, int? idRecinto)
        {
            if (idRecinto is null or <= 0)
                return true;
            if (p.IdRecinto is null or <= 0)
                return true;
            return p.IdRecinto.Value == idRecinto.Value;
        }

        private static List<TblPago> FiltrarPagosRecinto(
            IEnumerable<TblPago> pagos,
            int idRecinto,
            int? idRecintoOrigen,
            int? mesIngreso,
            DateTime? fechaTraslado,
            bool esOrigen)
            => pagos.Where(p => PerteneceRecintoPago(p, idRecinto, idRecintoOrigen, mesIngreso, fechaTraslado, esOrigen)).ToList();

        private static bool PerteneceRecintoPago(
            TblPago p,
            int idRecinto,
            int? idRecintoOrigen,
            int? mesIngreso,
            DateTime? fechaTraslado,
            bool esOrigen)
        {
            if (p.IdRecinto is > 0)
            {
                if (p.IdRecinto.Value != idRecinto)
                    return false;
                // El destino no hereda recibos anteriores al traslado.
                if (!esOrigen && fechaTraslado.HasValue)
                {
                    var fechaPago = (p.FechaEmision ?? p.FechaRegistro).Date;
                    if (fechaPago < fechaTraslado.Value.Date)
                        return false;
                }
                return true;
            }

            if (idRecintoOrigen is null or <= 0)
                return !esOrigen;

            if (p.IdMes is >= 1 and <= 12 && mesIngreso is >= 1 and <= 12)
                return esOrigen ? p.IdMes.Value < mesIngreso.Value : p.IdMes.Value >= mesIngreso.Value;

            return esOrigen;
        }

        private static EstadoCuentaMesCelda[] ConstruirMesesRecinto(
            IReadOnlyList<TblPago> pagosRecinto,
            HashSet<int> tiposMes,
            decimal pagadoEneroDistribuido,
            decimal montoMensual,
            string[] nombresMes,
            int? mesIngreso,
            bool esHistorial)
        {
            var meses = new EstadoCuentaMesCelda[12];
            for (int m = 1; m <= 12; m++)
            {
                var pagosMes = pagosRecinto
                    .Where(p => tiposMes.Contains(p.IdTipoMovimiento) && p.IdMes == m)
                    .ToList();
                decimal pagadoMes = m == 1 ? pagadoEneroDistribuido : pagosMes.Sum(p => p.Monto);
                // Mes anterior al ingreso (traslado o matrícula tardía) o posterior (historial)
                // no se cobra, salvo que ese colegio ya lo haya registrado.
                bool noCorresponde = mesIngreso is >= 1 and <= 12
                    && (esHistorial ? m >= mesIngreso.Value : m < mesIngreso.Value)
                    && pagadoMes <= 0.01m;
                decimal esperado = noCorresponde ? 0m : montoMensual;
                decimal saldo = noCorresponde ? 0m : Math.Max(0m, esperado - pagadoMes);
                bool cancelado = noCorresponde || esperado <= 0m || saldo <= 0.01m;
                meses[m - 1] = new EstadoCuentaMesCelda
                {
                    Mes = m,
                    NombreMes = nombresMes[m - 1],
                    MontoEsperado = esperado,
                    MontoPagado = pagadoMes,
                    Saldo = cancelado ? 0m : saldo,
                    Cancelado = cancelado,
                    TieneAbonoParcial = !noCorresponde && !cancelado && pagadoMes > 0.01m,
                    NoCorresponde = noCorresponde
                };
            }
            return meses;
        }

        private static EstadoCuentaMesCelda[] MesesSinCobro(string[] nombresMes)
        {
            var meses = new EstadoCuentaMesCelda[12];
            for (int m = 1; m <= 12; m++)
            {
                meses[m - 1] = new EstadoCuentaMesCelda
                {
                    Mes = m,
                    NombreMes = nombresMes[m - 1],
                    NoCorresponde = true
                };
            }
            return meses;
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

        public static List<TblAlumno> AlumnosParaEstadoCuenta(
            IEnumerable<TblAlumno>? alumnos,
            IEnumerable<TblMatricula>? matriculas,
            int idPeriodoRef)
        {
            var lista = alumnos?.ToList() ?? new List<TblAlumno>();
            var idsCiclo = new HashSet<int>(
                (matriculas ?? Array.Empty<TblMatricula>())
                    .Where(m => m.Activo && (idPeriodoRef <= 0 || m.IdPeriodo == idPeriodoRef))
                    .Select(m => m.IdAlumno));

            return lista.Where(a =>
            {
                bool delCiclo = a.IdPeriodo == idPeriodoRef || idsCiclo.Contains(a.IdAlumno);
                if (a.Activo != false)
                    return true;
                return delCiclo;
            }).ToList();
        }

        public static bool EsAlumnoRetiradoOInactivo(TblAlumno? alumno, TblMatricula? matricula)
        {
            if (alumno?.Activo == false)
                return true;
            if (EsMarcaRetiradoOInactivo(alumno?.TipoEstudiante)
                || EsMarcaRetiradoOInactivo(matricula?.TipoEstudiante))
                return true;
            return string.Equals(matricula?.Estado, TblMatricula.EstadoRetirado, StringComparison.OrdinalIgnoreCase);
        }

        public static string? EtiquetaEstadoAlumno(TblAlumno? alumno, TblMatricula? matricula)
        {
            bool retirado = EsMarcaRetirado(alumno?.TipoEstudiante)
                || EsMarcaRetirado(matricula?.TipoEstudiante)
                || string.Equals(matricula?.Estado, TblMatricula.EstadoRetirado, StringComparison.OrdinalIgnoreCase);
            if (retirado)
                return "Retirado";
            if (alumno?.Activo == false)
                return "Bloqueado";
            return null;
        }

        private static bool EsMarcaRetirado(string? texto)
        {
            var n = NormalizarNombrePersona(texto);
            return n.Contains("retirado");
        }

        private static bool EsMarcaRetiradoOInactivo(string? texto)
        {
            var n = NormalizarNombrePersona(texto);
            return n.Contains("retirado") || n.Contains("inactivo") || n.Contains("anulado");
        }

        private readonly struct SaldoConcepto
        {
            public decimal Esperado { get; init; }
            public decimal Saldo { get; init; }
            public bool Cancelado { get; init; }
            public bool AbonoParcial { get; init; }
        }

        private static SaldoConcepto EvaluarSaldoConcepto(bool aplica, decimal tarifa, decimal pagado)
        {
            if (!aplica)
                return default;
            decimal esperado = tarifa > 0.01m ? tarifa : (pagado > 0.01m ? pagado : 0m);
            if (esperado <= 0.01m)
                return default;
            decimal saldo = Math.Max(0m, esperado - pagado);
            bool cancelado = saldo <= 0.01m;
            return new SaldoConcepto
            {
                Esperado = esperado,
                Saldo = cancelado ? 0m : saldo,
                Cancelado = cancelado,
                AbonoParcial = !cancelado && pagado > 0.01m
            };
        }

        private static IEnumerable<(int Periodo, int Recinto, decimal Monto)> RecopilarMontosConcepto(
            IReadOnlyList<TblPago> pagos,
            IReadOnlyList<TblPagoCaja>? pagosCaja,
            HashSet<int> tipos,
            IReadOnlyList<CatTipoMovimiento>? tiposMovimiento,
            bool esRifa)
        {
            foreach (var p in pagos ?? Array.Empty<TblPago>())
            {
                if (!p.Activo || tipos.Count == 0 || !tipos.Contains(p.IdTipoMovimiento))
                    continue;
                yield return (p.IdPeriodo, p.IdRecinto ?? 0, p.Monto);
            }

            foreach (var c in pagosCaja ?? Array.Empty<TblPagoCaja>())
            {
                if (!c.Activo)
                    continue;
                bool coincide = esRifa
                    ? EsReciboRifa(c, tipos, tiposMovimiento)
                    : EsReciboPromocion(c, tipos, tiposMovimiento);
                if (!coincide)
                    continue;
                yield return (c.IdPeriodo, c.IdRecinto ?? 0, c.Monto);
            }
        }

        private static Dictionary<(int Periodo, int Recinto), decimal> ConstruirTarifasConcepto(
            IEnumerable<(int Periodo, int Recinto, decimal Monto)> lineas)
        {
            var dict = new Dictionary<(int Periodo, int Recinto), decimal>();
            var lista = lineas?.Where(x => x.Monto > 0.01m && x.Periodo > 0).ToList()
                ?? new List<(int Periodo, int Recinto, decimal Monto)>();
            foreach (var g in lista.GroupBy(x => (x.Periodo, Recinto: x.Recinto > 0 ? x.Recinto : 0)))
                dict[g.Key] = MontoMasFrecuente(g.Select(x => x.Monto));
            foreach (var g in lista.GroupBy(x => x.Periodo))
            {
                var key = (g.Key, 0);
                if (!dict.ContainsKey(key))
                    dict[key] = MontoMasFrecuente(g.Select(x => x.Monto));
            }
            return dict;
        }

        private static decimal ResolverTarifaConcepto(
            IReadOnlyDictionary<(int Periodo, int Recinto), decimal> tarifas,
            int idPeriodo,
            int? idRecinto)
        {
            if (tarifas == null || tarifas.Count == 0)
                return 0m;
            if (idRecinto is > 0
                && tarifas.TryGetValue((idPeriodo, idRecinto.Value), out var porRecinto)
                && porRecinto > 0.01m)
                return porRecinto;
            if (tarifas.TryGetValue((idPeriodo, 0), out var porPeriodo) && porPeriodo > 0.01m)
                return porPeriodo;
            var delPeriodo = tarifas.FirstOrDefault(kv => kv.Key.Periodo == idPeriodo && kv.Value > 0.01m).Value;
            if (delPeriodo > 0.01m)
                return delPeriodo;
            return tarifas.Values.FirstOrDefault(v => v > 0.01m);
        }

        private static decimal MontoMasFrecuente(IEnumerable<decimal> montos)
        {
            var positivos = montos
                .Where(m => m > 0.01m)
                .Select(m => decimal.Round(m, 2))
                .ToList();
            if (positivos.Count == 0)
                return 0m;
            return positivos
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Key)
                .First().Key;
        }

        public static bool EsConceptoConfirmacionMatricula(string? concepto)
        {
            var n = NormalizarNombrePersona(concepto);
            return n.Contains("confirmacion") || n.Contains("reserva");
        }

        public static HashSet<int> TiposPagoConfirmacion(IEnumerable<CatTipoMovimiento>? tipos)
        {
            var ids = new HashSet<int> { TipoConfirmacionMatricula };
            if (tipos == null)
                return ids;
            foreach (var t in tipos)
            {
                if (EsConceptoConfirmacionMatricula(t.Concepto))
                    ids.Add(t.IdTipoMovimiento);
            }
            return ids;
        }

        public static HashSet<int> TiposPagoMatricula(IEnumerable<CatTipoMovimiento>? tipos)
        {
            var ids = new HashSet<int> { TipoMatricula, TipoMatriculaAbono, TipoConfirmacionMatricula };
            if (tipos == null)
                return ids;
            foreach (var t in tipos)
            {
                var n = NormalizarNombrePersona(t.Concepto);
                if (n.Contains("matricula") || EsConceptoConfirmacionMatricula(t.Concepto))
                    ids.Add(t.IdTipoMovimiento);
            }
            return ids;
        }

        /// <summary>
        /// Si la matrícula del ciclo aún no está cubierta y la confirmación se guardó
        /// en el año siguiente (reserva automática), esa reserva abona este ciclo.
        /// No se usa si el ciclo ya está pagado: la reserva queda para el siguiente.
        /// </summary>
        public static decimal AcreditarReservaSiguienteSiMatriculaPendiente(
            IEnumerable<TblPago> pagos,
            int idAlumno,
            int idPeriodo,
            int anioPeriodo,
            IReadOnlyList<CatPeriodo>? periodos,
            IReadOnlyCollection<int>? tiposConfirmacion,
            decimal yaPagadoEnCiclo,
            decimal costoMatriculaNeta,
            int? idRecinto = null)
        {
            if (idAlumno <= 0 || idPeriodo <= 0)
                return 0m;
            if (costoMatriculaNeta > 0.01m && yaPagadoEnCiclo >= costoMatriculaNeta - 0.01m)
                return 0m;

            var tiposConf = tiposConfirmacion is { Count: > 0 }
                ? tiposConfirmacion
                : new HashSet<int> { TipoConfirmacionMatricula };

            bool yaHayConfirmacionEnCiclo = pagos.Any(p =>
                p.Activo
                && p.IdAlumno == idAlumno
                && p.IdPeriodo == idPeriodo
                && EsPagoDelRecinto(p, idRecinto)
                && tiposConf.Contains(p.IdTipoMovimiento));
            if (yaHayConfirmacionEnCiclo)
                return 0m;

            var siguiente = periodos?.FirstOrDefault(p => p.Activo && p.Periodo == anioPeriodo + 1);
            if (siguiente == null)
                return 0m;

            return pagos
                .Where(p => p.Activo
                    && p.IdAlumno == idAlumno
                    && p.IdPeriodo == siguiente.IdPeriodo
                    && EsPagoDelRecinto(p, idRecinto)
                    && tiposConf.Contains(p.IdTipoMovimiento))
                .Sum(p => p.Monto);
        }

        public static decimal SumarPagadoMatricula(
            IEnumerable<TblPago> pagos,
            int idAlumno,
            int idPeriodo,
            int anioPeriodo,
            IReadOnlyList<CatPeriodo>? periodos,
            IReadOnlyCollection<int>? tiposMatriculaPago,
            int? idRecinto = null)
        {
            var tipos = tiposMatriculaPago is { Count: > 0 }
                ? tiposMatriculaPago
                : new HashSet<int> { TipoMatricula, TipoMatriculaAbono };

            return pagos
                .Where(p =>
                {
                    if (!p.Activo || p.IdAlumno != idAlumno)
                        return false;
                    if (!EsPagoDelRecinto(p, idRecinto))
                        return false;
                    if (!tipos.Contains(p.IdTipoMovimiento))
                        return false;
                    if (p.IdPeriodo == idPeriodo)
                        return true;
                    var anioPago = periodos?.FirstOrDefault(x => x.IdPeriodo == p.IdPeriodo)?.Periodo ?? 0;
                    return anioPago > 0 && anioPago == anioPeriodo;
                })
                .Sum(p => p.Monto);
        }

        public static decimal SumarPagadoEnero(
            IEnumerable<TblPago> pagos,
            int idAlumno,
            int idPeriodo,
            IReadOnlyCollection<int>? idsMensualidad,
            int? idRecinto = null)
        {
            var tiposMes = new HashSet<int>(idsMensualidad ?? Array.Empty<int>()) { TipoMensualidad };
            return pagos
                .Where(p => p.Activo
                    && p.IdAlumno == idAlumno
                    && p.IdPeriodo == idPeriodo
                    && EsPagoDelRecinto(p, idRecinto)
                    && tiposMes.Contains(p.IdTipoMovimiento)
                    && p.IdMes == 1)
                .Sum(p => p.Monto);
        }

        public static HashSet<int> TiposPagoMensualidad(IEnumerable<CatTipoMovimiento>? tipos)
        {
            var ids = IdsPorConcepto(tipos, "mensualidad");
            ids.Add(TipoMensualidad);
            return ids;
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
            int? idModalidad,
            IEnumerable<Grados>? grados = null)
        {
            if (list == null || !list.Any())
                return null;

            IEnumerable<TblCostoMensualidad> Base()
            {
                var q = list.Where(x => x.IdRecinto == idRecinto &&
                                        x.IdPeriodo == idPeriodo &&
                                        x.Activo);
                if (grados != null)
                    q = q.Where(x => CostoNivelHelper.EsValido(x.IdGrado, grados));
                return q;
            }

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
        public const string MarcaRifaAplica = "[RIFA_APLICA:";

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

        public const decimal TarifaPromocionTercerNivel = 1100m;
        public const decimal TarifaPromocionSexto = 1400m;
        public const decimal TarifaPromocionUndecimo = 1900m;

        /// <summary>
        /// Promoción: 3.er nivel C$ 110, 6.° C$ 1 400, 11.° C$ 1 900.
        /// </summary>
        public static decimal TarifaPromocionPorGrado(string? nombreGrado)
        {
            if (!EsGradoConPromocion(nombreGrado))
                return 0m;

            var n = NormalizarNombrePersona(nombreGrado);
            if (n.Contains("undecimo") || n.Contains("11mo") || n.Contains("11°")
                || Regex.IsMatch(n, @"\b11\b"))
                return TarifaPromocionUndecimo;

            if (n.Contains("sexto") || n.Contains("6to") || n.Contains("6°")
                || Regex.IsMatch(n, @"\b6\s*(to|grado)\b"))
                return TarifaPromocionSexto;

            return TarifaPromocionTercerNivel;
        }

        public static int SemestreRifa(int? mes, DateTime? fechaEmision, DateTime? fechaRegistro, string? descripcion)
        {
            if (!string.IsNullOrWhiteSpace(descripcion))
            {
                var marca = Regex.Match(descripcion, @"\[RIFA:([12])\]", RegexOptions.IgnoreCase);
                if (marca.Success && int.TryParse(marca.Groups[1].Value, out var semMarca))
                    return semMarca;
                var d = NormalizarNombrePersona(descripcion);
                if (d.Contains("rifa 2") || d.Contains("julio a diciembre")
                    || d.Contains("segundo semestre") || d.Contains("2do semestre") || d.Contains("2o semestre"))
                    return 2;
                if (d.Contains("rifa 1") || d.Contains("enero a junio")
                    || d.Contains("primer semestre") || d.Contains("1er semestre") || d.Contains("1er sem"))
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

        public static string TextoPeriodoRifa(int semestre)
            => semestre == 2
                ? "Rifa 2 de julio a diciembre"
                : "Rifa 1 de enero a junio";

        public static bool EsConceptoRifa(string? conceptoCatalogo, int? idTipoMovimiento = null)
        {
            if (idTipoMovimiento == TipoRifa)
                return true;
            return !string.IsNullOrWhiteSpace(conceptoCatalogo)
                && conceptoCatalogo.Contains("rifa", StringComparison.OrdinalIgnoreCase);
        }

        public static string ConceptoVisibleRifa(string? conceptoCatalogo, int semestre, string? descripcion = null)
        {
            var periodo = TextoPeriodoRifa(semestre);
            var extra = QuitarMarcasInternasDescripcion(descripcion);
            if (string.IsNullOrWhiteSpace(extra))
                return periodo;
            if (extra.Contains("rifa 1", StringComparison.OrdinalIgnoreCase)
                || extra.Contains("rifa 2", StringComparison.OrdinalIgnoreCase)
                || extra.Contains("enero a junio", StringComparison.OrdinalIgnoreCase)
                || extra.Contains("julio a diciembre", StringComparison.OrdinalIgnoreCase))
                return extra;
            return $"{periodo}. {extra}";
        }

        public sealed class CortesGeneracionRifa
        {
            public Dictionary<(int Periodo, int Recinto, int Semestre), DateTime> PorRecinto { get; } = new();
            public Dictionary<(int Periodo, int Semestre), DateTime> PorPeriodo { get; } = new();

            public DateTime? FechaGeneracion(int idPeriodo, int? idRecinto, int semestre)
            {
                if (idRecinto is > 0
                    && PorRecinto.TryGetValue((idPeriodo, idRecinto.Value, semestre), out var porR))
                    return porR;
                return PorPeriodo.TryGetValue((idPeriodo, semestre), out var porP) ? porP : null;
            }
        }

        public sealed class EvaluacionRifaAlumno
        {
            public bool AplicaRifa1 { get; init; } = true;
            public bool AplicaRifa2 { get; init; } = true;
            public bool AplicaDefinidoEnMatricula { get; init; }
            public bool Rifa1Pagada { get; init; }
            public bool Rifa2Pagada { get; init; }
            public decimal MontoRifa1 { get; init; }
            public decimal MontoRifa2 { get; init; }
            public DateTime? FechaIngreso { get; init; }
            public DateTime? FechaGeneracionRifa1 { get; init; }
            public DateTime? FechaGeneracionRifa2 { get; init; }

            public bool RifasCubiertas =>
                (!AplicaRifa1 || Rifa1Pagada) && (!AplicaRifa2 || Rifa2Pagada);

            public int SiguienteSemestreACobrar()
            {
                if (AplicaRifa1 && !Rifa1Pagada)
                    return 1;
                if (AplicaRifa2 && !Rifa2Pagada)
                    return 2;
                return 0;
            }

            public string MotivoNoAplica(int semestre)
            {
                bool aplica = semestre == 2 ? AplicaRifa2 : AplicaRifa1;
                if (aplica)
                    return "";
                if (AplicaDefinidoEnMatricula)
                    return $"No aplica: en la matrícula se indicó que no debe pagar rifa {semestre}";
                return $"No aplica: matrícula posterior a la generación de la rifa {semestre}";
            }

            public string MensajeSinCobro()
            {
                if (!AplicaRifa1 && !AplicaRifa2)
                    return AplicaDefinidoEnMatricula
                        ? "En la matrícula se indicó que no debe pagar rifa 1 ni rifa 2."
                        : "No le corresponde rifa 1 ni rifa 2: se matriculó después de que se generaron.";
                if (AplicaRifa1 && Rifa1Pagada && AplicaRifa2 && Rifa2Pagada)
                    return "Este alumno ya tiene pagadas las dos rifas del año (1.er y 2.º semestre).";
                if (!AplicaRifa1 && AplicaRifa2 && Rifa2Pagada)
                    return AplicaDefinidoEnMatricula
                        ? "La rifa 1 no aplica (indicado en la matrícula). La rifa 2 ya está pagada."
                        : "La rifa 1 no aplica (matrícula posterior a su generación). La rifa 2 ya está pagada.";
                if (AplicaRifa1 && Rifa1Pagada && !AplicaRifa2)
                    return AplicaDefinidoEnMatricula
                        ? "La rifa 1 ya está pagada. La rifa 2 no aplica (indicado en la matrícula)."
                        : "La rifa 1 ya está pagada. La rifa 2 no aplica (matrícula posterior a su generación).";
                return "Este alumno ya tiene pagadas las rifas que le corresponden.";
            }
        }

        public static CortesGeneracionRifa ConstruirCortesGeneracionRifa(
            IEnumerable<TblPago>? pagos,
            IEnumerable<TblPagoCaja>? pagosCaja,
            IReadOnlyCollection<int> tiposRifa,
            IEnumerable<CatTipoMovimiento>? tiposMovimiento)
        {
            var cortes = new CortesGeneracionRifa();
            void Registrar(int idPeriodo, int? idRecinto, int semestre, DateTime fecha)
            {
                if (idPeriodo <= 0 || semestre is not (1 or 2) || fecha == default)
                    return;
                fecha = fecha.Date;
                if (idRecinto is > 0)
                {
                    var keyR = (idPeriodo, idRecinto.Value, semestre);
                    if (!cortes.PorRecinto.TryGetValue(keyR, out var actualR) || fecha < actualR)
                        cortes.PorRecinto[keyR] = fecha;
                }
                var keyP = (idPeriodo, semestre);
                if (!cortes.PorPeriodo.TryGetValue(keyP, out var actualP) || fecha < actualP)
                    cortes.PorPeriodo[keyP] = fecha;
            }

            foreach (var p in pagos ?? Array.Empty<TblPago>())
            {
                if (!p.Activo || !tiposRifa.Contains(p.IdTipoMovimiento))
                    continue;
                Registrar(p.IdPeriodo, p.IdRecinto, SemestreRifaDePago(p), FechaMovimientoRifa(p.FechaEmision, p.FechaRegistro));
            }

            foreach (var c in pagosCaja ?? Array.Empty<TblPagoCaja>())
            {
                if (!c.Activo || !EsReciboRifa(c, tiposRifa, tiposMovimiento))
                    continue;
                Registrar(c.IdPeriodo, c.IdRecinto, SemestreRifaDeCaja(c), FechaMovimientoRifa(c.FechaEmision, c.FechaRegistro));
            }

            return cortes;
        }

        public static EvaluacionRifaAlumno EvaluarRifaAlumno(
            TblAlumno? alumno,
            TblMatricula? matricula,
            IEnumerable<TblPago>? pagos,
            IEnumerable<TblPagoCaja>? pagosCaja,
            IReadOnlyCollection<int> tiposRifa,
            IReadOnlyCollection<int>? tiposMatricula,
            IEnumerable<CatTipoMovimiento>? tiposMovimiento,
            int idPeriodo,
            int anioPeriodo,
            int? idRecinto,
            CortesGeneracionRifa? cortes = null,
            DateTime? fechaIngresoOverride = null)
        {
            cortes ??= ConstruirCortesGeneracionRifa(pagos, pagosCaja, tiposRifa, tiposMovimiento);
            var tiposMat = tiposMatricula is { Count: > 0 }
                ? tiposMatricula
                : new HashSet<int> { TipoMatricula, TipoMatriculaAbono };

            DateTime? ingreso = fechaIngresoOverride?.Date
                ?? FechaIngresoCicloParaRifa(alumno, matricula, pagos, tiposMat, idPeriodo, anioPeriodo, idRecinto);
            var gen1 = cortes.FechaGeneracion(idPeriodo, idRecinto, 1);
            var gen2 = cortes.FechaGeneracion(idPeriodo, idRecinto, 2);
            var marcaAplica = LeerAplicaRifa(matricula?.Observaciones, alumno?.Observaciones);
            bool aplica1 = marcaAplica.TieneMarca
                ? marcaAplica.Aplica1
                : AplicaRifaSegunIngreso(ingreso, gen1);
            bool aplica2 = marcaAplica.TieneMarca
                ? marcaAplica.Aplica2
                : AplicaRifaSegunIngreso(ingreso, gen2);

            var pagosAlum = (pagos ?? Array.Empty<TblPago>())
                .Where(p => alumno != null
                    && p.Activo
                    && p.IdAlumno == alumno.IdAlumno
                    && tiposRifa.Contains(p.IdTipoMovimiento)
                    && (idPeriodo <= 0 || p.IdPeriodo == idPeriodo)
                    && EsPagoDelRecinto(p, idRecinto))
                .ToList();
            var cajaAlum = alumno != null
                ? RecibosCajaDelAlumno(pagosCaja?.ToList(), alumno, idPeriodo, anioPeriodo)
                    .Where(c => EsReciboRifa(c, tiposRifa, tiposMovimiento)
                        && (idRecinto is null or <= 0 || c.IdRecinto is null or <= 0 || c.IdRecinto == idRecinto))
                    .ToList()
                : new List<TblPagoCaja>();

            decimal monto1 = pagosAlum.Where(p => SemestreRifaDePago(p) == 1).Sum(p => p.Monto)
                + cajaAlum.Where(c => SemestreRifaDeCaja(c) == 1).Sum(c => c.Monto);
            decimal monto2 = pagosAlum.Where(p => SemestreRifaDePago(p) == 2).Sum(p => p.Monto)
                + cajaAlum.Where(c => SemestreRifaDeCaja(c) == 2).Sum(c => c.Monto);

            return new EvaluacionRifaAlumno
            {
                AplicaRifa1 = aplica1,
                AplicaRifa2 = aplica2,
                AplicaDefinidoEnMatricula = marcaAplica.TieneMarca,
                Rifa1Pagada = monto1 > 0.01m,
                Rifa2Pagada = monto2 > 0.01m,
                MontoRifa1 = monto1,
                MontoRifa2 = monto2,
                FechaIngreso = ingreso,
                FechaGeneracionRifa1 = gen1,
                FechaGeneracionRifa2 = gen2
            };
        }

        public static (bool TieneMarca, bool Aplica1, bool Aplica2) LeerAplicaRifa(params string?[] textos)
        {
            foreach (var texto in textos)
            {
                var leido = LeerAplicaRifaDeTexto(texto);
                if (leido.TieneMarca)
                    return leido;
            }
            return (false, true, true);
        }

        public static string QuitarMarcaAplicaRifa(string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                return string.Empty;
            return Regex.Replace(observaciones, @"\[RIFA_APLICA:[^\]]*\]\s*", "", RegexOptions.IgnoreCase).Trim();
        }

        public static string AnotarAplicaRifa(string? observaciones, bool aplica1, bool aplica2)
        {
            string valor = aplica1 && aplica2 ? "1,2"
                : aplica1 ? "1"
                : aplica2 ? "2"
                : "no";
            string marca = $"{MarcaRifaAplica}{valor}]";
            var resto = QuitarMarcaAplicaRifa(observaciones);
            var combined = string.IsNullOrWhiteSpace(resto) ? marca : marca + " " + resto;
            const int max = 300;
            if (combined.Length <= max)
                return combined;
            int maxResto = max - marca.Length - 1;
            if (maxResto <= 0)
                return marca.Length <= max ? marca : marca[..max];
            return marca + " " + resto[..Math.Min(resto.Length, maxResto)];
        }

        private static (bool TieneMarca, bool Aplica1, bool Aplica2) LeerAplicaRifaDeTexto(string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                return (false, true, true);
            var match = Regex.Match(observaciones, @"\[RIFA_APLICA:([^\]]*)\]", RegexOptions.IgnoreCase);
            if (!match.Success)
                return (false, true, true);
            var raw = match.Groups[1].Value.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(raw) || raw is "no" or "ninguna" or "ninguno" or "0")
                return (true, false, false);
            return (true, raw.Contains('1'), raw.Contains('2'));
        }

        public static DateTime? FechaIngresoCicloParaRifa(
            TblAlumno? alumno,
            TblMatricula? matricula,
            IEnumerable<TblPago>? pagos,
            IReadOnlyCollection<int> tiposMatricula,
            int idPeriodo,
            int anioPeriodo,
            int? idRecinto)
        {
            var pagoMat = (pagos ?? Array.Empty<TblPago>())
                .Where(p => alumno != null
                    && p.Activo
                    && p.IdAlumno == alumno.IdAlumno
                    && tiposMatricula.Contains(p.IdTipoMovimiento)
                    && (idPeriodo <= 0 || p.IdPeriodo == idPeriodo)
                    && EsPagoDelRecinto(p, idRecinto))
                .OrderBy(p => p.FechaEmision ?? p.FechaRegistro)
                .ThenBy(p => p.IdPago)
                .FirstOrDefault();
            if (pagoMat != null)
                return NormalizarFechaIngresoRifa(pagoMat.FechaEmision ?? pagoMat.FechaRegistro, anioPeriodo);

            if (matricula?.FechaMatricula is { Year: > 2000 } fm)
                return NormalizarFechaIngresoRifa(fm, anioPeriodo);

            return null;
        }

        private static DateTime NormalizarFechaIngresoRifa(DateTime fecha, int anioPeriodo)
        {
            fecha = fecha.Date;
            if (fecha.Month >= 10)
            {
                int anio = anioPeriodo > 2000 ? anioPeriodo : fecha.Year + 1;
                return new DateTime(anio, 1, 1);
            }
            return fecha;
        }

        private static bool AplicaRifaSegunIngreso(DateTime? fechaIngreso, DateTime? fechaGeneracion)
        {
            if (!fechaGeneracion.HasValue)
                return true;
            if (!fechaIngreso.HasValue)
                return true;
            return fechaIngreso.Value.Date <= fechaGeneracion.Value.Date;
        }

        private static DateTime FechaMovimientoRifa(DateTime? fechaEmision, DateTime fechaRegistro)
        {
            var fecha = fechaEmision ?? fechaRegistro;
            return fecha == default ? fechaRegistro : fecha.Date;
        }

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
