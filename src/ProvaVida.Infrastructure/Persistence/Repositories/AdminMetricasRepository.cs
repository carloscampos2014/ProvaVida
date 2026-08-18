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
            FROM checkins
            WHERE data_hora >= DATE_TRUNC('day', NOW() AT TIME ZONE 'UTC')
            """);
    }

    public async Task<int> ContarUsuariosComCheckInAtrasadoAsync(int diasSemCheckIn, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM usuarios u
            WHERE u.ativo = true
              AND NOT EXISTS (
                  SELECT 1 FROM checkins c
                  WHERE c.usuario_id = u.id
                    AND c.data_hora >= NOW() - INTERVAL '1 day' * @dias
              )
            """,
            new { dias = diasSemCheckIn });
    }

    public async Task<int> ContarUsuariosPossivelmnteSemInternetAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(DISTINCT h.usuario_id)
            FROM heartbeats h
            WHERE h.data_hora >= NOW() - INTERVAL '48 hours'
              AND NOT EXISTS (
                  SELECT 1 FROM checkins c
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

    public async Task<int> ContarTotalEventosAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM notificacoes_emergencia");
    }

    public async Task<IEnumerable<EventosNotificacaoRow>> ListarEventosAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        var offset = (pagina - 1) * tamanhoPagina;

        var rows = await conn.QueryAsync<EventosNotificacaoRow>(
            """
            SELECT
                ne.id                AS Id,
                u.nome               AS NomeUsuario,
                ne.status            AS Status,
                ne.canal             AS Canal,
                ne.data_disparo      AS DataDisparo,
                ne.janela_expira_em  AS JanelaExpiraEm
            FROM notificacoes_emergencia ne
            JOIN usuarios u ON u.id = ne.usuario_id
            ORDER BY ne.data_disparo DESC
            LIMIT @limite OFFSET @offset
            """,
            new { limite = tamanhoPagina, offset });

        return rows;
    }
}
