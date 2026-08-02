// João Gabriel Hensen dos Santos
namespace AcademiaDoZe.Domain.Entities;

public abstract class Entity
{
    public int Id { get; protected set; }

    protected Entity(int id = 0)
    {
        if (id < 0) throw new Exception("ID_NEGATIVO");
        Id = id;
    }
}