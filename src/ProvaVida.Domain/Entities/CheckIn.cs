namespace ProvaVida.Domain.Entities;

public class CheckIn
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid IdLocal { get; private set; }   // UUID gerado no app — garante idempotência
    public DateTime DataHora { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;

    // EF / Dapper
    protected CheckIn() { }

    public static CheckIn Criar(
        Guid usuarioId,
        Guid idLocal,
        DateTime dataHora,
        double? latitude,
        double? longitude,
        string deviceId)
    {
        return new CheckIn
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            IdLocal = idLocal,
            DataHora = dataHora,
            Latitude = latitude,
            Longitude = longitude,
            DeviceId = deviceId.Trim()
        };
    }
}
