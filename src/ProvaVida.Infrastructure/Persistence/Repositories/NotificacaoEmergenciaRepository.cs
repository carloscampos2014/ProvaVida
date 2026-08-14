using Dapper;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Infrastructure.Persistence.Repositories;

public sealed class NotificacaoEmergenciaRepository : INotificacaoEmergenciaRepository
{
    private readonly IUnitOfWork _uow;
    private readonly DbConnectionFactory _factory;

    public NotificacaoEmergenciaRepository(IUnitOfWork uow, DbConnectionFactory factory)
    {
        _uow = uow;
        _factory = factory;
    }

    public async Task AdicionarAsync(NotificacaoEmergencia notificacao, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO notificacoes_emergencia (id, usuario_id, status, canal, data_disparo, janela_expira_em)
            VALUES (@Id, @UsuarioId, @Status, @Canal, @DataDisparo, @JanelaExpiraEm)
            ON CONFLICT (id) DO UPDATE
              SET status           = EXCLUDED.status,
                  canal            = EXCLUDED.canal,
                  data_disparo     = EXCLUDED.data_disparo,
                  janela_expira_em = EXCLUDED.janela_expira_em
            """;

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                notificacao.Id,
                notificacao.UsuarioId,
                notificacao.Status,
                notificacao.Canal,
                DataDisparo    = notificacao.DataDisparo,
                JanelaExpiraEm = notificacao.JanelaExpiraEm
            }, transaction: _uow.Transaction, cancellationToken: ct));
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => Task.CompletedTask; // commit via IUnitOfWork

    public async Task<bool> ExisteNotificacaoAtivaNasUltimasHorasAsync(
        Guid usuarioId, int horas, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(1) FROM notificacoes_emergencia
            WHERE usuario_id = @UsuarioId
              AND status IN ('aguardando_resposta', 'disparado', 'heartbeat_ativo')
              AND data_disparo >= NOW() - (@Horas || ' hours')::interval
            """;

        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId, Horas = horas },
                cancellationToken: ct));
        return count > 0;
    }

    public async Task<IEnumerable<NotificacaoEmergencia>> ListarJanelasExpiradasAsync(
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT id               AS "Id",
                   usuario_id       AS "UsuarioId",
                   status           AS "Status",
                   canal            AS "Canal",
                   data_disparo     AS "DataDisparo",
                   janela_expira_em AS "JanelaExpiraEm"
            FROM notificacoes_emergencia
            WHERE status = 'aguardando_resposta'
              AND janela_expira_em <= NOW()
            """;

        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<NotifRow>(
            new CommandDefinition(sql, cancellationToken: ct));

        return rows.Select(Mapear).ToList();
    }

#pragma warning disable CA1812
    private sealed class NotifRow
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;
        public DateTime DataDisparo { get; set; }
        public DateTime? JanelaExpiraEm { get; set; }
    }
#pragma warning restore CA1812

    private static NotificacaoEmergencia Mapear(NotifRow row)
    {
        var n = (NotificacaoEmergencia)System.Runtime.CompilerServices
            .RuntimeHelpers.GetUninitializedObject(typeof(NotificacaoEmergencia));
        Set(n, nameof(NotificacaoEmergencia.Id), row.Id);
        Set(n, nameof(NotificacaoEmergencia.UsuarioId), row.UsuarioId);
        Set(n, nameof(NotificacaoEmergencia.Status), row.Status);
        Set(n, nameof(NotificacaoEmergencia.Canal), row.Canal);
        Set(n, nameof(NotificacaoEmergencia.DataDisparo), row.DataDisparo);
        Set(n, nameof(NotificacaoEmergencia.JanelaExpiraEm), row.JanelaExpiraEm);
        return n;
    }

    private static void Set(object obj, string prop, object? valor) =>
        typeof(NotificacaoEmergencia)
            .GetProperty(prop, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            ?.SetValue(obj, valor);
}
