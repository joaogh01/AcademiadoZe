// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string valor)
    {
        if (NormalizacaoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");

        var textoLimpo = NormalizacaoService.LimparEDigitos(valor);
        if (textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");

        if (!Validar(textoLimpo))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");

        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }

    private static bool Validar(string cpf)
    {
        if (cpf.Length != 11) return false;
        string[] invalidos = ["00000000000", "11111111111", "22222222222", "33333333333", "44444444444", "55555555555", "66666666666", "77777777777", "88888888888", "99999999999"];
        if (invalidos.Contains(cpf)) return false;
        return true;
    }

    public override string ToString() => Valor;
}