using Dapper;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Infrastructure.Persistence.Repositories;

public sealed class CheckInRepository : ICheckInRepository
{
    private readonly IUnitOfWork _uow;
    private readonly DbConnectionFactory _factory;

    public CheckInRepository(IUnitOfWork uow, DbConnectionFactory factory)
    {
        _uow = uow;
        _factory = factory;
    }

    public async Task<bool> AdicionarSeNaoExistirAsync(CheckIn checkIn, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO checkins (id, usuario_id, id_local, data_hora, latitude, longitude, device_id)
            VALUES (@Id, @UsuarioId, @IdLocal, @DataHora, @Latitude, @Longitude, @DeviceId)
            ON CONFLICT (usuario_id, id_local) DO NOTHING
            """;

        var linhasAfetadas = await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                checkIn.Id,
                checkIn.UsuarioId,
                checkIn.IdLocal,
                checkIn.DataHora,
                checkIn.Latitude,
                checkIn.Longitude,
                checkIn.DeviceId
            }, transaction: _uow.Transaction, cancellationToken: ct));

        return linhasAfetadas > 0;
    }

    public async Task<IEnumerable<CheckIn>> ListarPorUsuarioAsync(
        Guid usuarioId, DateTimeOffset dataInicio, DateTimeOffset dataFim, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id          AS "Id",
                   usuario_id  AS "UsuarioId",
                   id_local    AS "IdLocal",
                   data_hora   AS "DataHora",
                   latitude    AS "Latitude",
                   longitude   AS "Longitude",
                   device_id   AS "DeviceId"
            FROM checkins
            WHERE usuario_id = @UsuarioId
              AND data_hora BETWEEN @DataInicio AND @DataFim
            ORDER BY data_hora DESC
            """;

        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<CheckInRow>(
            new CommandDefinition(sql,
                new { UsuarioId = usuarioId, DataInicio = dataInicio, DataFim = dataFim },
                cancellationToken: ct));

        return rows.Select(Mapear).ToList();
    }

    public async Task<IEnumerable<Guid>> ListarUsuariosInativosDesdeAsync(
        DateTimeOffset dataCorte, CancellationToken ct = default)
    {
        // Retorna usuario_id de todos usuários ativos cujo ÚLTIMO check-in foi antes de dataCorte
        // ou que nunca fizeram check-in
        const string sql = """
            SELECT u.id
            FROM usuarios u
            WHERE u.ativo = TRUE
              AND (
                    -- Nunca fez check-in
                    NOT EXISTS (SELECT 1 FROM checkins c WHERE c.usuario_id = u.id)
                    OR
                    -- Último check-in antes da data de corte
                    (SELECT MAX(c.data_hora) FROM checkins c WHERE c.usuario_id = u.id) < @DataCorte
              )
            """;

        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Guid>(
            new CommandDefinition(sql, new { DataCorte = dataCorte }, cancellationToken: ct));
    }

    public async Task<bool> ExisteCheckInRecenteAsync(Guid usuarioId, int horas, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM checkins
            WHERE usuario_id = @UsuarioId
              AND data_hora >= NOW() - INTERVAL '1 hour' * @Horas
            """;

        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId, Horas = horas }, cancellationToken: ct));
        return count > 0;
    }

    public async Task<IEnumerable<object>> ListarTodosAsync(int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                c.id,
                u.email AS usuario_email,
                u.nome  AS usuario_nome,
                c.data_hora,
                c.device_id
            FROM checkins c
            JOIN usuarios u ON u.id = c.usuario_id
            ORDER BY c.data_hora DESC
            LIMIT @Limite OFFSET @Offset
            """;

        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync(
            new CommandDefinition(sql,
                new { Limite = tamanhoPagina, Offset = (pagina - 1) * tamanhoPagina },
                cancellationToken: ct));
    }

#pragma warning disable CA1812
    private sealed class CheckInRow
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid IdLocal { get; set; }
        public DateTimeOffset DataHora { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string DeviceId { get; set; } = string.Empty;
    }
#pragma warning restore CA1812

    private static CheckIn Mapear(CheckInRow row)
    {
        var c = (CheckIn)System.Runtime.CompilerServices
            .RuntimeHelpers.GetUninitializedObject(typeof(CheckIn));
        Set(c, nameof(CheckIn.Id), row.Id);
        Set(c, nameof(CheckIn.UsuarioId), row.UsuarioId);
        Set(c, nameof(CheckIn.IdLocal), row.IdLocal);
        Set(c, nameof(CheckIn.DataHora), row.DataHora);
        Set(c, nameof(CheckIn.Latitude), row.Latitude);
        Set(c, nameof(CheckIn.Longitude), row.Longitude);
        Set(c, nameof(CheckIn.DeviceId), row.DeviceId);
        return c;
    }

    private static void Set(object obj, string prop, object? valor) =>
        typeof(CheckIn)
            .GetProperty(prop,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance)
            ?.SetValue(obj, valor);
}
