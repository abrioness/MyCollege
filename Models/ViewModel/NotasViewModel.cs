using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class NotasViewModel
    {
        public TblNotas notas { get; set; }

        public TblAlumno alumnoNotas { get; set; } =new();

        public List<TblNotas> listNotas { get; set; } = new();

        public List<SelectListItem> alumnosSelectListItem { get; set; } = new();

        public List<SelectListItem> tipoEvaluacionesSelectListItem { get; set; } = new();

        public List<SelectListItem> asignaturaSelectListItem { get; set; } = new();

        public List<SelectListItem> modalidadSelectListItem { get; set; } = new();

        public List<SelectListItem> gradosSelectListItem { get; set; } = new();

        /// <summary>Recintos (colegio/sede); mapea a <c>TblNotas.IdColegio</c> = <c>Recintos.IdRecinto</c>.</summary>
        public List<SelectListItem> recintosSelectListItem { get; set; } = new();

        public List<SelectListItem> periodoEvaluacionsSelectListItem { get; set; } = new();

        public List<SelectListItem> sexoSelectListItem { get; set; } = new();

        public List<SelectListItem> periodoSelectListItem { get; set; } = new();

        /// <summary>Año lectivo actual (CatPeriodo con Actual=true) para cabecera de detalle; evita mostrar período viejo de la ficha.</summary>
        public string? PeriodoLectivoActualTexto { get; set; }

        /// <summary>Opciones para calificación cualitativa: AA, AS, AF, AI según rangos 0-100.</summary>
        public List<SelectListItem> NotasCualitativasSelectListItem { get; set; } = new();
    }

    /// <summary>
    /// Escala cualitativa (nota final / acumulado+examen): AA 90-100, AS 76-89, AF 60-75, AI 59-0.
    /// En 3.° a 11.° el nivel AI (Aprendizaje inicial) corresponde a 59-0.
    /// </summary>
    public static class EscalaCualitativa
    {
        public const string AA = "AA";
        public const string AS = "AS";
        public const string AF = "AF";
        public const string AI = "AI";

        public static readonly List<(string Codigo, string Descripcion, string Rango)> Opciones = new()
        {
            (AA, "Aprendizaje alcanzado", "100-90"),
            (AS, "Aprendizaje satisfactorio", "89-76"),
            (AF, "Aprendizaje fundamental", "75-60"),
            (AI, "Aprendizaje inicial", "59-0")
        };

        /// <summary>Convierte nota numérica (0-100) a código cualitativo.</summary>
        public static string NumeroACualitativo(decimal? valor)
        {
            if (valor == null) return string.Empty;
            var v = (decimal)valor;
            if (v >= 90) return AA;
            if (v >= 76) return AS;
            if (v >= 60) return AF;
            return AI;
        }

        /// <summary>Valor representativo medio de cada código (reportes / equivalencias).</summary>
        public static decimal? CualitativoANumero(string codigo)
        {
            if (string.IsNullOrEmpty(codigo)) return null;
            return codigo.ToUpperInvariant() switch
            {
                AA => 95m,
                AS => 82.5m,
                AF => 67.5m,
                AI => 29.5m,
                _ => null
            };
        }
    }
}
