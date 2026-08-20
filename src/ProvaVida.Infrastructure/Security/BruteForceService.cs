using Dapper;
using ProvaVida.Application.Interfaces;
using ProvaVida.Infrastructure.Persistence;

namespace ProvaVida.Infrastructure.Security;

/// <summary>
/// Protege endpoints públicos contra brute force.
/// Registra tentativas no banco e bloqueia IPs por 24h após exceder o limite.
/// </summary>
public sealed class BruteForceService : IBruteForceService
{
    private readonly DbConnectionFactory _factory;
    private const int HorasJanela   = 24;
    private const int HorasBloqueio = 24;

    public BruteForceService(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<DateTime?> ObterBloqueioAsync(string ip, CancellationToken ct = default)
    {
        const string sql = """
            SELECT expira_em
            FROM ips_bloqueados
            WHERE ip = @Ip
              AND liberado_em IS NULL
              AND expira_em > NOW()
            ORDER BY bloqueado_em DESC
            LIMIT 1
            """;

        using var conn = _factory.CreateConnection();
        var expiraEm = await conn.QueryFirstOrDefaultAsync<DateTime?>(
            new CommandDefinition(sql, new { Ip = ip }, cancellationToken: ct));

        return expiraEm;
    }

    public async Task RegistrarTentativaAsync(
        string ip, string endpoint, int limiteMax, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();

        // Registra a tentativa
        const string sqlInsert = """
            INSERT INTO tentativas_login (ip, endpoint)
            VALUES (@Ip, @Endpoint)
            """;
        await conn.ExecuteAsync(
            new CommandDefinition(sqlInsert, new { Ip = ip, Endpoint = endpoint }, cancellationToken: ct));

        // Conta tentativas na janela
        const string sqlCount = """
            SELECT COUNT(*)
            FROM tentativas_login
            WHERE ip = @Ip
              AND criado_em >= NOW() - INTERVAL '1 hour' * @Horas
            """;
        var total = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlCount, new { Ip = ip, Horas = HorasJanela }, cancellationToken: ct));

        // Bloqueia se atingiu o limite e ainda não está bloqueado
        if (total >= limiteMax)
        {
            var jaBloqueado = await ObterBloqueioAsync(ip, ct);
            if (jaBloqueado is null)
            {
                const string sqlBloquear = """
                    INSERT INTO ips_bloqueados (ip, motivo, expira_em)
                    VALUES (@Ip, @Motivo, NOW() + INTERVAL '1 hour' * @Horas)
                    """;
                await conn.ExecuteAsync(
                    new CommandDefinition(sqlBloquear, new
                    {
                        Ip = ip,
                        Motivo = $"{total} tentativas em {HorasJanela}h no endpoint {endpoint}",
                        Horas  = HorasBloqueio
                    }, cancellationToken: ct));
            }
        }
    }

    public async Task<IEnumerable<IpBloqueadoDto>> ListarBloqueadosAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                b.id            AS "Id",
                b.ip            AS "Ip",
                b.motivo        AS "Motivo",
                b.bloqueado_em  AS "BloqueadoEm",
                b.expira_em     AS "ExpiraEm",
                COUNT(t.id)     AS "TotalTentativas"
            FROM ips_bloqueados b
            LEFT JOIN tentativas_login t ON t.ip = b.ip
                AND t.criado_em >= NOW() - INTERVAL '24 hours'
            WHERE b.liberado_em IS NULL
              AND b.expira_em > NOW()
            GROUP BY b.id, b.ip, b.motivo, b.bloqueado_em, b.expira_em
            ORDER BY b.bloqueado_em DESC
            """;

        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<IpBloqueadoDto>(
            new CommandDefinition(sql, cancellationToken: ct));
    }

    public async Task LiberarAsync(string ip, string liberadoPor, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE ips_bloqueados
            SET liberado_em  = NOW(),
                liberado_por = @LiberadoPor
            WHERE ip = @Ip
              AND liberado_em IS NULL
              AND expira_em > NOW()
            """;

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { Ip = ip, LiberadoPor = liberadoPor }, cancellationToken: ct));
    }
}
