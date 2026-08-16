// João Gabriel Hensen dos Santos
using System.Linq;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.Services;

public static class NormalizacaoService
{
    private static readonly Regex EspacosRegex = new(@"\s+", RegexOptions.Compiled);

    public static bool TextoVazioOuNulo(string? texto) => string.IsNullOrWhiteSpace(texto);

    public static string LimparEspacos(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? string.Empty : EspacosRegex.Replace(texto, " ").Trim();

    public static string LimparTodosEspacos(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? string.Empty : texto.Replace(" ", string.Empty);

    public static string ParaMaiusculo(string? texto) =>
        string.IsNullOrEmpty(texto) ? string.Empty : texto.ToUpperInvariant();

    public static string LimparEDigitos(string? texto) =>
        string.IsNullOrEmpty(texto) ? string.Empty : new string(texto.Where(char.IsDigit).ToArray());
}