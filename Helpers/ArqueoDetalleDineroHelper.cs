using System.Globalization;
using System.Text.Json;
using WebColegio.Models.ViewModel;

namespace WebColegio.Helpers
{
    public class ArqueoDenominacionDto
    {
        public decimal D { get; set; }
        public int C { get; set; }
    }

    public class ArqueoDetalleDineroDto
    {
        public List<ArqueoDenominacionDto> Cordobas { get; set; } = new();
        public List<ArqueoDenominacionDto> Dolares { get; set; } = new();
        public decimal Tasa { get; set; }
        public decimal TotalCordobas { get; set; }
        public decimal TotalDolares { get; set; }
        public decimal Equivalente { get; set; }
        public decimal TotalDetalleDinero { get; set; }
    }

    public static class ArqueoDetalleDineroHelper
    {
        public static readonly decimal[] DenomsCordoba = { 1000, 500, 200, 100, 50, 20, 10, 5, 1, 0.50m };
        public static readonly decimal[] DenomsDolar = { 100, 50, 20, 10, 5, 1 };

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static ArqueoDetalleDineroDto? Leer(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;
            var t = texto.Trim();
            if (t.StartsWith("v1|", StringComparison.OrdinalIgnoreCase))
                return LeerCompacto(t);
            if (t.Length > 0 && t[0] == '{')
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<ArqueoDetalleDineroDto>(t, JsonOpts);
                    if (dto != null)
                        CompletarTotales(dto);
                    return dto;
                }
                catch
                {
                    return null;
                }
            }

            return LeerCompacto(t);
        }

        public static bool TieneConteo(string? texto)
        {
            var det = Leer(texto);
            if (det == null)
                return false;
            return (det.Cordobas?.Any(x => x.C > 0) == true)
                || (det.Dolares?.Any(x => x.C > 0) == true);
        }

        public static string Serializar(ArqueoDetalleDineroDto det)
        {
            CompletarTotales(det);
            var c = DenomsCordoba.Select(d => CantidadDe(det.Cordobas, d)).ToList();
            var d = DenomsDolar.Select(x => CantidadDe(det.Dolares, x)).ToList();
            QuitarCerosFinales(c);
            QuitarCerosFinales(d);
            var tasa = det.Tasa > 0.0001m
                ? det.Tasa.ToString("0.##", CultureInfo.InvariantCulture)
                : "";
            return $"v1|{string.Join(",", c)}|{string.Join(",", d)}|{tasa}";
        }

        public static string? NormalizarParaGuardar(string? json)
        {
            var det = Leer(json);
            if (det == null)
                return string.IsNullOrWhiteSpace(json) ? null : json.Trim();
            return Serializar(det);
        }

        public static void Aplicar(ArqueoDiarioViewModel arqueo, ArqueoDetalleDineroDto det)
        {
            CompletarTotales(det);
            arqueo.DetalleCordobas = (det.Cordobas ?? new List<ArqueoDenominacionDto>())
                .Select(x => new DetalleCordoba
                {
                    Denominacion = x.D,
                    Cantidad = x.C,
                    Monto = Math.Round(x.D * x.C, 2)
                })
                .ToList();
            arqueo.DetalleDolares = (det.Dolares ?? new List<ArqueoDenominacionDto>())
                .Select(x => new DetalleDolar
                {
                    Denominacion = x.D,
                    Cantidad = x.C,
                    Monto = Math.Round(x.D * x.C, 2)
                })
                .ToList();
            arqueo.TotalCordobas = det.TotalCordobas;
            arqueo.TotalDolares = det.TotalDolares;
            arqueo.EquivalenteCordobas = det.Equivalente;
            arqueo.TasaCambio = det.Tasa;
        }

        public static int Cantidad(IEnumerable<DetalleCordoba>? lista, decimal denominacion)
            => lista?.FirstOrDefault(x => CasiIgual(x.Denominacion, denominacion))?.Cantidad ?? 0;

        public static int Cantidad(IEnumerable<DetalleDolar>? lista, decimal denominacion)
            => lista?.FirstOrDefault(x => CasiIgual(x.Denominacion, denominacion))?.Cantidad ?? 0;

        public static string TasaTexto(decimal tasa)
            => tasa > 0.0001m ? tasa.ToString("0.00", CultureInfo.InvariantCulture) : "";

        private static ArqueoDetalleDineroDto? LeerCompacto(string texto)
        {
            var cuerpo = texto.StartsWith("v1|", StringComparison.OrdinalIgnoreCase)
                ? texto[3..]
                : texto;
            var partes = cuerpo.Split('|');
            if (partes.Length < 2)
                return null;

            var dto = new ArqueoDetalleDineroDto
            {
                Cordobas = ParseCantidades(partes[0], DenomsCordoba),
                Dolares = partes.Length > 1 ? ParseCantidades(partes[1], DenomsDolar) : new List<ArqueoDenominacionDto>()
            };
            if (partes.Length > 2
                && decimal.TryParse(partes[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var tasa))
                dto.Tasa = tasa;
            CompletarTotales(dto);
            if (!dto.Cordobas.Any(x => x.C > 0) && !dto.Dolares.Any(x => x.C > 0) && dto.Tasa <= 0)
                return null;
            return dto;
        }

        private static List<ArqueoDenominacionDto> ParseCantidades(string csv, decimal[] denoms)
        {
            var lista = new List<ArqueoDenominacionDto>();
            if (string.IsNullOrWhiteSpace(csv))
                return lista;
            var nums = csv.Split(',');
            for (int i = 0; i < denoms.Length && i < nums.Length; i++)
            {
                if (!int.TryParse(nums[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var c) || c <= 0)
                    continue;
                lista.Add(new ArqueoDenominacionDto { D = denoms[i], C = c });
            }
            return lista;
        }

        private static int CantidadDe(IEnumerable<ArqueoDenominacionDto>? lista, decimal denom)
            => lista?.FirstOrDefault(x => CasiIgual(x.D, denom))?.C ?? 0;

        private static void QuitarCerosFinales(List<int> cantidades)
        {
            for (int i = cantidades.Count - 1; i >= 0; i--)
            {
                if (cantidades[i] != 0)
                    break;
                cantidades.RemoveAt(i);
            }
        }

        private static void CompletarTotales(ArqueoDetalleDineroDto det)
        {
            det.TotalCordobas = (det.Cordobas ?? new List<ArqueoDenominacionDto>())
                .Sum(x => Math.Round(x.D * x.C, 2));
            det.TotalDolares = (det.Dolares ?? new List<ArqueoDenominacionDto>())
                .Sum(x => Math.Round(x.D * x.C, 2));
            det.Equivalente = det.Tasa > 0 ? Math.Round(det.TotalDolares * det.Tasa, 2) : 0m;
            det.TotalDetalleDinero = det.TotalCordobas + det.Equivalente;
        }

        private static bool CasiIgual(decimal a, decimal b)
            => Math.Abs(a - b) < 0.001m;
    }
}
