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

public record IpBloqueadoDto(
    Guid Id,
    string Ip,
    string Motivo,
    DateTime BloqueadoEm,
    DateTime ExpiraEm,
    int TotalTentativas);
