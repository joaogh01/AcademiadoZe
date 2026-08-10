// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity
{
    public Aluno Aluno { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoAluno(int id, Aluno aluno, DateTime dataHora) : base(id)
    {
        Aluno = aluno;
        DataHora = dataHora;
    }

    public static Result<AcessoAluno> Criar(int id, Aluno aluno, DateTime dataHora)
    {
        if (aluno == null)
            return Result<AcessoAluno>.Failure("Aluno", "ALUNO_OBRIGATORIO");

        if (dataHora == default)
            return Result<AcessoAluno>.Failure("DataHora", "DATA_HORA_OBRIGATORIA");

        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno, dataHora));
    }
}