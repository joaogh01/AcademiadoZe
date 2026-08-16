// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.Tests.Services;

public class NormalizacaoServiceTests
{
    [Theory(DisplayName = "NormalizacaoService: TextoVazioOuNulo")]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("   ", true)]
    [InlineData("\t", true)]
    [InlineData("\n", true)]
    [InlineData("a", false)]
    [InlineData("texto", false)]
    public void Deve_TextoVazioOuNulo_RetornarEsperado(string? input, bool expected)
    {
        var result = NormalizacaoService.TextoVazioOuNulo(input);
        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizacaoService: LimparEspacos")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(" a ", "a")]
    [InlineData(" a b c ", "a b c")]
    [InlineData("a   b   c", "a b c")]
    [InlineData("a\tb\nc", "a b c")]
    [InlineData("texto sem espacos extras", "texto sem espacos extras")]
    public void Deve_Normalizar_Espacos_Quando_LimparEspacosChamado(string? input, string expected)
    {
        var result = NormalizacaoService.LimparEspacos(input);
        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizacaoService: LimparTodosEspacos")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("a b c", "abc")]
    [InlineData(" a b ", "ab")]
    [InlineData("  x  y  z  ", "xyz")]
    public void Deve_Remover_Todos_Espacos_Quando_LimparTodosEspacosChamado(string? input, string expected)
    {
        var result = NormalizacaoService.LimparTodosEspacos(input);
        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizacaoService: ParaMaiusculo")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("abc", "ABC")]
    [InlineData("sc", "SC")]
    [InlineData("áéíõç", "ÁÉÍÕÇ")]
    public void Deve_Converter_Para_Maiusculo_Quando_ParaMaiusculoChamado(string? input, string expected)
    {
        var result = NormalizacaoService.ParaMaiusculo(input);
        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizacaoService: LimparEDigitos")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("a1b2c3", "123")]
    [InlineData("(11) 91234-5678", "11912345678")]
    [InlineData("88.520-000", "88520000")]
    [InlineData("no-digits", "")]
    public void Deve_Manter_Apenas_Digitos_Quando_LimparEDigitosChamado(string? input, string expected)
    {
        var result = NormalizacaoService.LimparEDigitos(input);
        Assert.Equal(expected, result);
    }
}