// João Gabriel Hensen dos Santos
namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateTime DataHoraEntrada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    public AcessoColaborador(int id, Colaborador colaborador, DateTime dataHoraEntrada, DateTime? dataHoraSaida = null)
        : base(id)
    {
        Colaborador = colaborador;
        DataHoraEntrada = dataHoraEntrada;
        DataHoraSaida = dataHoraSaida;
    }
}