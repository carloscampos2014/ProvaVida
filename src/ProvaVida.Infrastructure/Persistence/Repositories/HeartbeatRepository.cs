using Dapper;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Infrastructure.Persistence.Repositories;

public sealed class HeartbeatRepository : IHeartbeatRepository
{
    private readonly IUnitOfWork _uow;
    private readonly DbConnectionFactory _factory;

    public HeartbeatRepository(IUnitOfWork uow, DbConnectionFactory factory)
    {
        _uow = uow;
        _factory = factory;
    }

    public async Task AdicionarAsync(Heartbeat heartbeat, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO heartbeats (id, usuario_id, data_hora)
            VALUES (@Id, @UsuarioId, @DataHora)
            """;

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                heartbeat.Id,
                heartbeat.UsuarioId,
                heartbeat.DataHora
            }, transaction: _uow.Transaction, cancellationToken: ct));
    }

    public async Task<bool> ExisteHeartbeatRecenteAsync(
        Guid usuarioId, int horas, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(1) FROM heartbeats
            WHERE usuario_id = @UsuarioId
              AND data_hora >= NOW() - (@Horas || ' hours')::interval
            """;

        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId, Horas = horas },
                cancellationToken: ct));

        return count > 0;
    }
}
