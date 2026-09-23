using System.Text.Json;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public class EmailCampanaStore
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
        private readonly string _ruta;
        private readonly object _lock = new();

        public EmailCampanaStore(IWebHostEnvironment env)
        {
            var dir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dir);
            _ruta = Path.Combine(dir, "email-campana.json");
        }

        public EmailCampanaEstado Leer()
        {
            lock (_lock)
            {
                if (!File.Exists(_ruta))
                    return new EmailCampanaEstado();
                try
                {
                    var json = File.ReadAllText(_ruta);
                    return JsonSerializer.Deserialize<EmailCampanaEstado>(json) ?? new EmailCampanaEstado();
                }
                catch
                {
                    return new EmailCampanaEstado();
                }
            }
        }

        public void Guardar(EmailCampanaEstado estado)
        {
            lock (_lock)
            {
                estado.Envios = estado.Envios
                    .OrderByDescending(e => e.Fecha)
                    .Take(300)
                    .ToList();
                File.WriteAllText(_ruta, JsonSerializer.Serialize(estado, JsonOpts));
            }
        }

        public void Programar(DateTime fecha)
        {
            var estado = Leer();
            estado.FechaProgramada = fecha.Date;
            Guardar(estado);
        }

        public void CancelarProgramacion()
        {
            var estado = Leer();
            estado.FechaProgramada = null;
            Guardar(estado);
        }

        public void Registrar(IEnumerable<EmailEnvioLog> logs, bool marcarCampana, DateTime? fechaProgramadaConsumida = null)
        {
            var estado = Leer();
            estado.Envios.InsertRange(0, logs);
            if (marcarCampana)
            {
                var hoy = DateTime.Today;
                estado.UltimaCampanaFecha = DateTime.Now;
                if (fechaProgramadaConsumida.HasValue)
                {
                    estado.UltimaProgramadaYmd = fechaProgramadaConsumida.Value.ToString("yyyy-MM-dd");
                    if (estado.FechaProgramada?.Date == fechaProgramadaConsumida.Value.Date)
                        estado.FechaProgramada = null;
                }
                else
                {
                    estado.UltimaProgramadaYmd = hoy.ToString("yyyy-MM-dd");
                }
            }
            Guardar(estado);
        }

        public bool ProgramadaPendienteParaHoy(DateTime ahora)
        {
            var estado = Leer();
            if (!estado.FechaProgramada.HasValue)
                return false;
            if (estado.FechaProgramada.Value.Date > ahora.Date)
                return false;
            return !string.Equals(estado.UltimaProgramadaYmd, ahora.ToString("yyyy-MM-dd"), StringComparison.Ordinal);
        }
    }
}
