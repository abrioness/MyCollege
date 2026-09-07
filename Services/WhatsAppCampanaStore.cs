using System.Text.Json;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public class WhatsAppCampanaStore
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
        private readonly string _ruta;
        private readonly object _lock = new();

        public WhatsAppCampanaStore(IWebHostEnvironment env)
        {
            var dir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dir);
            _ruta = Path.Combine(dir, "whatsapp-campana.json");
        }

        public WhatsAppCampanaEstado Leer()
        {
            lock (_lock)
            {
                if (!File.Exists(_ruta))
                    return new WhatsAppCampanaEstado();
                try
                {
                    var json = File.ReadAllText(_ruta);
                    return JsonSerializer.Deserialize<WhatsAppCampanaEstado>(json) ?? new WhatsAppCampanaEstado();
                }
                catch
                {
                    return new WhatsAppCampanaEstado();
                }
            }
        }

        public void Guardar(WhatsAppCampanaEstado estado)
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

        public void Registrar(IEnumerable<WhatsAppEnvioLog> logs, bool marcarCampana)
        {
            var estado = Leer();
            estado.Envios.InsertRange(0, logs);
            if (marcarCampana)
            {
                var hoy = DateTime.Today;
                estado.UltimaCampanaYm = hoy.ToString("yyyy-MM");
                estado.UltimaCampanaFecha = DateTime.Now;
            }
            Guardar(estado);
        }

        public bool CampanaDelMesYaEnviada(DateTime? fecha = null)
        {
            var f = fecha ?? DateTime.Today;
            var estado = Leer();
            return string.Equals(estado.UltimaCampanaYm, f.ToString("yyyy-MM"), StringComparison.Ordinal);
        }
    }
}
