// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoColaborador(int id, Colaborador colaborador, DateTime dataHora) : base(id)
    {
        Colaborador = colaborador;
        DataHora = dataHora;
    }

    public static Result<AcessoColaborador> Criar(int id, Colaborador colaborador, DateTime dataHora)
    {
        if (colaborador == null)
            return Result<AcessoColaborador>.Failure("Colaborador", "COLABORADOR_OBRIGATORIO");

        if (dataHora == default)
            return Result<AcessoColaborador>.Failure("DataHora", "DATA_HORA_OBRIGATORIA");

        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaborador, dataHora));
    }
}