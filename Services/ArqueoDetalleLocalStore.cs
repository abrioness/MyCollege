using System.Text.Json;

namespace WebColegio.Services
{
    public class ArqueoDetalleLocalStore
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
        private readonly string _ruta;
        private readonly object _lock = new();

        public ArqueoDetalleLocalStore(IWebHostEnvironment env)
        {
            var dir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dir);
            _ruta = Path.Combine(dir, "arqueo-detalle.json");
        }

        public void Guardar(int? idRecinto, DateTime fecha, int? idArqueo, string? detalle)
        {
            if (string.IsNullOrWhiteSpace(detalle))
                return;

            lock (_lock)
            {
                var estado = LeerArchivo();
                var clave = Clave(idRecinto, fecha);
                estado.Items.RemoveAll(x =>
                    string.Equals(x.Clave, clave, StringComparison.OrdinalIgnoreCase)
                    || (idArqueo is > 0 && x.IdArqueo == idArqueo));
                estado.Items.Add(new Item
                {
                    Clave = clave,
                    IdArqueo = idArqueo is > 0 ? idArqueo : null,
                    Detalle = detalle.Trim(),
                    Fecha = DateTime.Now
                });
                estado.Items = estado.Items
                    .OrderByDescending(x => x.Fecha)
                    .Take(400)
                    .ToList();
                File.WriteAllText(_ruta, JsonSerializer.Serialize(estado, JsonOpts));
            }
        }

        public string? Leer(int? idRecinto, DateTime fecha, int? idArqueo = null)
        {
            lock (_lock)
            {
                var estado = LeerArchivo();
                if (idArqueo is > 0)
                {
                    var porId = estado.Items.FirstOrDefault(x => x.IdArqueo == idArqueo);
                    if (!string.IsNullOrWhiteSpace(porId?.Detalle))
                        return porId.Detalle;
                }

                var clave = Clave(idRecinto, fecha);
                return estado.Items
                    .FirstOrDefault(x => string.Equals(x.Clave, clave, StringComparison.OrdinalIgnoreCase))
                    ?.Detalle;
            }
        }

        private static string Clave(int? idRecinto, DateTime fecha)
            => $"{idRecinto.GetValueOrDefault()}:{fecha:yyyy-MM-dd}";

        private Estado LeerArchivo()
        {
            if (!File.Exists(_ruta))
                return new Estado();
            try
            {
                return JsonSerializer.Deserialize<Estado>(File.ReadAllText(_ruta), JsonOpts) ?? new Estado();
            }
            catch
            {
                return new Estado();
            }
        }

        private sealed class Estado
        {
            public List<Item> Items { get; set; } = new();
        }

        private sealed class Item
        {
            public string Clave { get; set; } = "";
            public int? IdArqueo { get; set; }
            public string Detalle { get; set; } = "";
            public DateTime Fecha { get; set; }
        }
    }
}
