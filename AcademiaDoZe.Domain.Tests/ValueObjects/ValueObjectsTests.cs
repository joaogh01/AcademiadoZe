// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Theory(DisplayName = "Cep: dígitos inválidos -> CEP_DIGITOS")]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123")]
    [InlineData("12-345")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    public void Deve_Falhar_Criacao_Quando_CepDigitosInvalidos(string input)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_DIGITOS");
    }

    [Theory(DisplayName = "Cep: formatos válidos (com e sem hífen)")]
    [InlineData("12345-678")]
    [InlineData("12345678")]
    [InlineData("88520-000")]
    [InlineData("88520000")]
    public void Deve_Criar_Cep_Quando_Valido(string input)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(8, result.Value!.Valor.Length);
    }

    [Theory(DisplayName = "Cep: obrigatório -> CEP_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CepNuloOuVazio(string? input)
    {
        var result = Cep.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    [Theory(DisplayName = "Endereco: criação válida")]
    [InlineData("10", "Bloco A")]
    [InlineData("1", "")]
    [InlineData("100", "Apto 201")]
    [InlineData("S/N", "Fundos")]
    public void Deve_Criar_Endereco_Quando_Valido(string numero, string complemento)
    {
        var logradouro = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
        var result = Endereco.Criar(logradouro, numero, complemento);
        Assert.True(result.IsSuccess);
        Assert.Equal(logradouro.Id, result.Value!.LogradouroId);
    }

    [Theory(DisplayName = "Endereco: valida obrigatoriedade")]
    [InlineData(null, "1", "LOGRADOURO_OBRIGATORIO")]
    [InlineData("valid", "", "NUMERO_OBRIGATORIO")]
    [InlineData("valid", " ", "NUMERO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_EnderecoInvalido(string? logradouroCase, string numero, string expected)
    {
        Logradouro? logradouro = null;
        if (logradouroCase == "valid")
            logradouro = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

        var result = Endereco.Criar(logradouro!, numero, "");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == expected);
    }

    [Theory(DisplayName = "Cpf: obrigatório")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CpfNuloOuVazio(string? input)
    {
        var result = Cpf.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cpf: formatos válidos")]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("123.456.789-01")]
    public void Deve_Criar_Cpf_Quando_ValorValido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsSuccess);
    }

    [Theory(DisplayName = "Cpf: inválido - repetidos")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    public void Deve_Falhar_Criacao_Quando_CpfInvalido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Theory(DisplayName = "Cpf: tamanho inválido")]
    [InlineData("123")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Criacao_Quando_CpfSemDigitos(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }

    [Theory(DisplayName = "Telefone: dígitos inválidos")]
    [InlineData("1")]
    [InlineData("1234")]
    [InlineData("(1)2345")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Criacao_Quando_TelefoneDigitosInvalidos(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_DIGITOS");
    }

    [Theory(DisplayName = "Telefone: válidos")]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]
    [InlineData("49999998888")]
    public void Deve_Criar_Telefone_Quando_Valido(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsSuccess);
    }

    [Theory(DisplayName = "Telefone: obrigatório")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_TelefoneNuloOuVazio(string? input)
    {
        var result = Telefone.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }

    [Theory(DisplayName = "Senha: valida maiúscula e tamanho")]
    [InlineData("abcdef", false)]
    [InlineData("12345", false)]
    [InlineData("Abcdef", true)]
    [InlineData("Senha123", true)]
    public void Deve_Validar_RequisitoUppercase_Senha(string senha, bool isSuccess)
    {
        var result = Senha.Criar(senha);
        Assert.Equal(isSuccess, result.IsSuccess);
    }

    [Theory(DisplayName = "Senha: obrigatório")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_SenhaNulaOuVazia(string? input)
    {
        var result = Senha.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: obrigatório")]
    public void Deve_Falhar_Criacao_Arquivo_Nulo()
    {
        var result = Arquivo.Criar(null!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: criação com sucesso")]
    public void Deve_Criar_Arquivo_Sucesso()
    {
        var result = Arquivo.Criar([1, 2, 3]);
        Assert.True(result.IsSuccess);
    }

    [Theory(DisplayName = "Email: validação")]
    [InlineData("email@valido.com", true)]
    [InlineData("aluno@uniplac.net", true)]
    [InlineData("emailinvalido", false)]
    [InlineData("@dominio.com", false)]
    [InlineData("nome@", false)]
    public void Deve_Validar_Email(string email, bool esperado)
    {
        var result = Email.Criar(email);
        Assert.Equal(esperado, result.IsSuccess);
    }
}