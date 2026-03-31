using System.Text.RegularExpressions;
using WebColegio.Models.ViewModel;

namespace WebColegio;

/// <summary>
/// Formato tipo libreta: semestres con CUAL/CUANT; preescolar y 1.°–2.° solo muestran cualitativo (cuantitativo vacío).
/// </summary>
public static class NotasDisplayHelper
{
    public static int? ObtenerNumeroGrado(string? nombreGrado)
    {
        if (string.IsNullOrWhiteSpace(nombreGrado)) return null;
        var normalizado = nombreGrado.Trim().ToLowerInvariant();
        var m = Regex.Match(normalizado, @"\d+");
        if (m.Success && int.TryParse(m.Value, out var numero))
            return numero;
        if (normalizado.Contains("primero") || normalizado.Contains("primer")) return 1;
        if (normalizado.Contains("segundo")) return 2;
        if (normalizado.Contains("tercero") || normalizado.Contains("tercer")) return 3;
        if (normalizado.Contains("cuarto")) return 4;
        if (normalizado.Contains("quinto")) return 5;
        if (normalizado.Contains("sexto")) return 6;
        if (normalizado.Contains("setimo") || normalizado.Contains("séptimo") || normalizado.Contains("septimo")) return 7;
        if (normalizado.Contains("octavo")) return 8;
        if (normalizado.Contains("noveno")) return 9;
        if (normalizado.Contains("decimo") || normalizado.Contains("décimo")) return 10;
        if (normalizado.Contains("undecimo") || normalizado.Contains("undécimo")) return 11;
        return null;
    }

    /// <summary>Preescolar (modalidad) o 1.°–2.°: solo cualitativo en pantalla.</summary>
    public static bool EsSoloCualitativo(string? nombreModalidad, string? nombreGrado)
    {
        var mod = nombreModalidad?.Trim().ToLowerInvariant() ?? string.Empty;
        if (mod.Contains("preescolar") || mod.Contains("pre escolar")) return true;
        var g = ObtenerNumeroGrado(nombreGrado);
        return g == 1 || g == 2;
    }

    public static decimal? PromedioDos(decimal? a, decimal? b)
    {
        if (a.HasValue && b.HasValue) return (a.Value + b.Value) / 2m;
        if (a.HasValue) return a;
        if (b.HasValue) return b;
        return null;
    }

    /// <summary>Promedio de cortes cualitativos vía equivalencia numérica.</summary>
    public static string? PromedioCualitativoSemestre(string? c1, decimal? n1, string? c2, decimal? n2)
    {
        var promNum = PromedioDos(n1, n2);
        if (promNum.HasValue)
            return EscalaCualitativa.NumeroACualitativo(promNum);
        var e1 = EscalaCualitativa.CualitativoANumero(c1);
        var e2 = EscalaCualitativa.CualitativoANumero(c2);
        var promEq = PromedioDos(e1, e2);
        if (promEq.HasValue)
            return EscalaCualitativa.NumeroACualitativo(promEq);
        if (!string.IsNullOrEmpty(c1)) return c1;
        if (!string.IsNullOrEmpty(c2)) return c2;
        return null;
    }

    public static string TextoCual(string? codigo) => string.IsNullOrEmpty(codigo) ? "—" : codigo;

    public static string TextoCuant(decimal? valor, bool mostrarCuantitativo)
    {
        if (!mostrarCuantitativo) return "";
        return valor.HasValue ? valor.Value.ToString("N1") : "—";
    }
}
