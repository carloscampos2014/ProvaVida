namespace ProvaVida.Application.Interfaces;

public interface IBruteForceService
{
    /// <summary>Verifica se o IP está bloqueado. Retorna null se livre, ou a data de expiração.</summary>
    Task<DateTime?> ObterBloqueioAsync(string ip, CancellationToken ct = default);

    /// <summary>Registra uma tentativa e bloqueia o IP se ultrapassar o limite.</summary>
    Task RegistrarTentativaAsync(string ip, string endpoint, int limiteMax, CancellationToken ct = default);

    /// <summary>Lista os IPs atualmente bloqueados.</summary>
    Task<IEnumerable<IpBloqueadoDto>> ListarBloqueadosAsync(CancellationToken ct = default);

    /// <summary>Libera manualmente um IP bloqueado.</summary>
    Task LiberarAsync(string ip, string liberadoPor, CancellationToken ct = default);
}

public class IpBloqueadoDto
{
    public Guid Id { get; init; }
    public string Ip { get; init; } = string.Empty;
    public string Motivo { get; init; } = string.Empty;
    public DateTime BloqueadoEm { get; init; }
    public DateTime ExpiraEm { get; init; }
    public long TotalTentativas { get; init; }
}
