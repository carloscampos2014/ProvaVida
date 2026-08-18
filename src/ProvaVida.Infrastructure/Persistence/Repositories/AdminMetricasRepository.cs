using Dapper;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Infrastructure.Persistence.Repositories;

public class AdminMetricasRepository : IAdminMetricasRepository
{
    private readonly DbConnectionFactory _db;

    public AdminMetricasRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<int> ContarUsuariosAtivosAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM usuarios WHERE ativo = true");
    }

    public async Task<int> ContarNovosUsuariosAsync(int ultimosDias, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM usuarios WHERE ativo = true AND criado_em >= NOW() - INTERVAL '1 day' * @dias",
            new { dias = ultimosDias });
    }

    public async Task<int> ContarUsuariosComCheckInHojeAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(DISTINCT usuario_id)
            FROM check_ins
            WHERE data_hora >= DATE_TRUNC('day', NOW() AT TIME ZONE 'UTC')
            """);
    }

    public async Task<int> ContarUsuariosComCheckInAtrasadoAsync(int diasSemCheckIn, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        // Usuários ativos cujo último check-in foi há mais de N dias (ou nunca fizeram)
        return await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM usuarios u
            WHERE u.ativo = true
              AND NOT EXISTS (
                  SELECT 1 FROM check_ins c
                  WHERE c.usuario_id = u.id
                    AND c.data_hora >= NOW() - INTERVAL '1 day' * @dias
              )
            """,
            new { dias = diasSemCheckIn });
    }

    public async Task<int> ContarUsuariosPossivelmnteSemInternetAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        // Teve heartbeat nas últimas 48h mas não fez check-in nas últimas 24h
        // Indica app aberto mas sem conseguir sincronizar o check-in
        return await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(DISTINCT h.usuario_id)
            FROM heartbeats h
            WHERE h.data_hora >= NOW() - INTERVAL '48 hours'
              AND NOT EXISTS (
                  SELECT 1 FROM check_ins c
                  WHERE c.usuario_id = h.usuario_id
                    AND c.data_hora >= NOW() - INTERVAL '24 hours'
              )
            """);
    }

    public async Task<int> ContarNotificacoesPorStatusHojeAsync(string status, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM notificacoes_emergencia
            WHERE status = @status
              AND data_disparo >= DATE_TRUNC('day', NOW() AT TIME ZONE 'UTC')
            """,
            new { status });
    }

    public async Task<int> ContarTotalNotificacoesPorStatusAsync(string status, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM notificacoes_emergencia WHERE status = @status",
            new { status });
    }
}
