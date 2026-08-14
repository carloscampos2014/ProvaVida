namespace ProvaVida.Domain.Entities;

public class Heartbeat
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTime DataHora { get; private set; }

    protected Heartbeat() { }

    public static Heartbeat Criar(Guid usuarioId, DateTime dataHora)
    {
        return new Heartbeat
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            DataHora = dataHora
        };
    }
}
