using System.Globalization;
using Microsoft.Extensions.Options;
using WebColegio.Configuration;
using WebColegio.Helpers;
using WebColegio.Models;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public class WhatsAppAvisoService
    {
        private readonly IServicesApi _api;
        private readonly IWhatsAppSender _sender;
        private readonly WhatsAppCampanaStore _store;
        private readonly WhatsAppSettings _settings;

        public WhatsAppAvisoService(
            IServicesApi api,
            IWhatsAppSender sender,
            WhatsAppCampanaStore store,
            IOptions<WhatsAppSettings> settings)
        {
            _api = api;
            _sender = sender;
            _store = store;
            _settings = settings.Value;
        }

        public bool ApiConfigurada => _sender.EstaConfigurado;
        public bool EnvioAutomatico => _settings.EnvioAutomatico;

        public async Task<List<WhatsAppAvisoDestino>> ListarPendientesAsync()
        {
            var filas = await CargarFilasAsync();
            var alumnos = (await _api.GetAlumnosAsync() ?? new List<TblAlumno>())
                .ToDictionary(a => a.IdAlumno);

            var destinos = new List<WhatsAppAvisoDestino>();
            foreach (var fila in filas)
            {
                if (fila.DatosIncompletos)
                    continue;
                bool debe = fila.GranTotalPendiente > 0.01m
                    || string.Equals(fila.EstadoPagoMensualidad, "Insolvente", StringComparison.OrdinalIgnoreCase);
                if (!debe)
                    continue;

                alumnos.TryGetValue(fila.IdAlumno, out var alumno);
                var (tel, origen, nombre) = WhatsAppTelefonoHelper.ResolverContacto(
                    alumno ?? new TblAlumno(),
                    _settings.CodigoPais);

                var destino = new WhatsAppAvisoDestino
                {
                    IdAlumno = fila.IdAlumno,
                    NombreAlumno = fila.NombreCompleto,
                    Grado = fila.NombreGrado,
                    Recinto = fila.Recinto,
                    NombreContacto = nombre,
                    OrigenTelefono = origen,
                    TelefonoE164 = tel,
                    MesesPendientes = fila.MesesPendientesSolvencia,
                    SaldoMatricula = fila.SaldoMatricula,
                    SaldoMensualidades = fila.TotalSaldoMensualidades,
                    TotalPendiente = fila.GranTotalPendiente,
                    EstadoPago = fila.EstadoPagoMensualidad
                };
                destino.Mensaje = WhatsAppMensajeBuilder.Construir(
                    _settings.NombreColegio,
                    destino.NombreContacto,
                    destino);
                if (!string.IsNullOrWhiteSpace(tel))
                    destino.EnlaceWaMe = "https://wa.me/" + tel + "?text=" + Uri.EscapeDataString(destino.Mensaje);
                destinos.Add(destino);
            }

            return destinos
                .OrderByDescending(d => d.TotalPendiente)
                .ThenBy(d => d.NombreAlumno)
                .ToList();
        }

        public async Task<List<WhatsAppEnvioLog>> EnviarAsync(IEnumerable<int> idAlumnos, bool marcarCampana, CancellationToken ct = default)
        {
            var ids = idAlumnos.ToHashSet();
            var destinos = (await ListarPendientesAsync())
                .Where(d => ids.Contains(d.IdAlumno) && !d.SinTelefono)
                .ToList();

            var logs = new List<WhatsAppEnvioLog>();
            foreach (var d in destinos)
            {
                ct.ThrowIfCancellationRequested();
                var (ok, error) = await EnviarDestinoAsync(d, ct);
                logs.Add(new WhatsAppEnvioLog
                {
                    Fecha = DateTime.Now,
                    IdAlumno = d.IdAlumno,
                    NombreAlumno = d.NombreAlumno,
                    Telefono = d.TelefonoE164,
                    Exito = ok,
                    Detalle = error,
                    Canal = "api"
                });
                await Task.Delay(700, ct);
            }

            if (logs.Count > 0)
                _store.Registrar(logs, marcarCampana);
            return logs;
        }

        public Task<List<WhatsAppEnvioLog>> EnviarCampanaAsync(CancellationToken ct = default)
            => EnviarTodosPendientesAsync(marcarCampana: true, ct);

        public async Task<List<WhatsAppEnvioLog>> EnviarTodosPendientesAsync(bool marcarCampana, CancellationToken ct = default)
        {
            var destinos = (await ListarPendientesAsync()).Where(d => !d.SinTelefono).Select(d => d.IdAlumno);
            return await EnviarAsync(destinos, marcarCampana, ct);
        }

        public WhatsAppCampanaEstado EstadoCampana() => _store.Leer();

        public bool CampanaDelMesYaEnviada() => _store.CampanaDelMesYaEnviada();

        private async Task<(bool Ok, string? Error)> EnviarDestinoAsync(WhatsAppAvisoDestino d, CancellationToken ct)
        {
            var cultura = new CultureInfo("es-NI");
            var detalle = string.IsNullOrWhiteSpace(d.MesesPendientes)
                ? $"Saldo C$ {d.TotalPendiente.ToString("N2", cultura)}"
                : $"Meses: {d.MesesPendientes}";
            var total = "C$ " + d.TotalPendiente.ToString("N2", cultura);

            if (!string.IsNullOrWhiteSpace(_settings.TemplateName))
            {
                return await _sender.EnviarPlantillaMoraAsync(
                    d.TelefonoE164!,
                    d.NombreContacto ?? "familiar",
                    d.NombreAlumno,
                    detalle,
                    total,
                    ct);
            }

            return await _sender.EnviarTextoAsync(d.TelefonoE164!, d.Mensaje, ct);
        }

        private async Task<List<EstadoCuentaFilaAlumno>> CargarFilasAsync()
        {
            var pagos = await _api.GetPagosAsync() ?? new List<TblPago>();
            var alumnos = (await _api.GetAlumnosAsync())?.Where(a => a.Activo != false).ToList()
                ?? new List<TblAlumno>();
            var periodos = await _api.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoRef = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos);
            if (periodoRef == null)
                return new List<EstadoCuentaFilaAlumno>();

            var costosMen = await _api.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>();
            var costosMat = await _api.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>();
            var matriculas = (await _api.GetMatriculasAsync() ?? new List<TblMatricula>()).Where(m => m.Activo).ToList();
            var grados = await _api.GetGradosAsync() ?? new List<Grados>();
            var recintos = await _api.GetRecintosAsync() ?? new List<Recintos>();
            var mesesCatalog = await _api.GetMesesAsync() ?? new List<TblCatMeses>();
            var tiposMov = await _api.GetTipoMovimientoAsync() ?? new List<CatTipoMovimiento>();
            var pagosCaja = await _api.GetPagoCajaAsync() ?? new List<TblPagoCaja>();

            return EstadoCuentaCalculoHelper.ConstruirFilas(
                alumnos,
                pagos,
                costosMen,
                costosMat,
                matriculas,
                grados,
                recintos,
                mesesCatalog,
                periodoRef.IdPeriodo,
                periodoRef.Periodo,
                periodos,
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "mensualidad"),
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "rifa", "rifas"),
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "promoc"),
                pagosCaja,
                tiposMov);
        }
    }
}
