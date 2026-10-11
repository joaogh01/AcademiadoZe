using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class AlunoMappingExtensions
{
    public static AlunoDto ToDto(this Aluno aluno, Logradouro? logradouro = null)
    {
        ArgumentNullException.ThrowIfNull(aluno);

        return new AlunoDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Cpf = aluno.Cpf.Valor,
            DataNascimento = aluno.DataNascimento,
            Telefone = aluno.Telefone.Valor,
            Email = aluno.Email.Valor,
            Endereco = logradouro != null ? logradouro.ToDto() : (aluno.Endereco != null ? new LogradouroDto
            {
                Id = aluno.Endereco.LogradouroId,
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = string.Empty
            } : null),
            Numero = aluno.Endereco?.Numero ?? string.Empty,
            Complemento = aluno.Endereco?.Complemento,
            Senha = null,
            Foto = aluno.Foto != null ? new ArquivoDto { Conteudo = aluno.Foto.Conteudo } : null
        };
    }

    public static Aluno ToEntity(this AlunoDto alunoDto, Logradouro? logradouro = null)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);

        var logradouroEntidade = logradouro ?? (alunoDto.Endereco != null ? alunoDto.Endereco.ToEntity() : throw new InvalidOperationException("Logradouro é obrigatório para cadastrar Aluno."));

        Arquivo? foto = null;
        if (alunoDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(alunoDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }

        var result = Aluno.Criar(
            alunoDto.Id,
            alunoDto.Nome,
            alunoDto.Cpf,
            alunoDto.DataNascimento,
            alunoDto.Telefone,
            alunoDto.Email ?? string.Empty,
            logradouroEntidade,
            alunoDto.Numero,
            alunoDto.Complemento ?? string.Empty,
            !string.IsNullOrWhiteSpace(alunoDto.Senha) ? alunoDto.Senha : "Academia@123",
            foto!
        );

        if (result.IsFailure)
            throw new InvalidOperationException($"Erro de validação ao converter Aluno: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");

        return result.Value!;
    }

    public static Aluno UpdateFromDto(this Aluno aluno, AlunoDto alunoDto, Logradouro? logradouro = null)
    {
        ArgumentNullException.ThrowIfNull(aluno);
        ArgumentNullException.ThrowIfNull(alunoDto);

        var logradouroEntidade = logradouro ?? throw new InvalidOperationException("Logradouro é obrigatório para atualizar Aluno.");

        Arquivo? foto = aluno.Foto;
        if (alunoDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(alunoDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }

        string senha = !string.IsNullOrWhiteSpace(alunoDto.Senha) ? alunoDto.Senha : (aluno.Senha?.Valor ?? "Academia@123");

        var result = Aluno.Criar(
            aluno.Id,
            alunoDto.Nome ?? aluno.Nome,
            alunoDto.Cpf ?? aluno.Cpf.Valor,
            alunoDto.DataNascimento != default ? alunoDto.DataNascimento : aluno.DataNascimento,
            alunoDto.Telefone ?? aluno.Telefone.Valor,
            alunoDto.Email ?? aluno.Email.Valor,
            logradouroEntidade,
            alunoDto.Numero ?? aluno.Endereco.Numero,
            alunoDto.Complemento ?? aluno.Endereco.Complemento,
            senha,
            foto!
        );

        if (result.IsFailure)
            throw new InvalidOperationException($"Erro de validação ao atualizar Aluno: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");

        return result.Value!;
    }
}