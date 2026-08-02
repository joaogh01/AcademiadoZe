// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Colaborador : Pessoa
{
    public DateOnly DataAdmissao { get; private set; }
    public ColaboradorTipo Tipo { get; private set; }
    public ColaboradorVinculo Vinculo { get; private set; }

    public Colaborador(
        int id,
        string nome,
        Cpf cpf,
        DateOnly dataNascimento,
        Telefone telefone,
        Email email,
        Endereco endereco,
        Senha senha,
        Arquivo foto,
        DateOnly dataAdmissao,
        ColaboradorTipo tipo,
        ColaboradorVinculo vinculo)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
        DataAdmissao = dataAdmissao;
        Tipo = tipo;
        Vinculo = vinculo;
    }
}