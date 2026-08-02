// João Gabriel Hensen dos Santos
namespace AcademiaDoZe.Domain.ValueObjects;

public record Endereco(
    Cep Cep,
    string Pais,
    string Estado,
    string Cidade,
    string Bairro,
    string Logradouro,
    string Numero,
    string Complemento
);