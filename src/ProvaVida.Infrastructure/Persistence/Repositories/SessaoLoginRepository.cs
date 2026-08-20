using Dapper;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;
using ProvaVida.Infrastructure.Persistence;
using ProvaVida.Infrastructure.Security;

namespace ProvaVida.Infrastructure.Persistence.Repositories;

public sealed class SessaoLoginRepository : ISessaoLoginRepository
{
    private readonly IUnitOfWork _uow;
    private readonly DbConnectionFactory _factory;
    private readonly IRefreshTokenHasher _hasher;

    public SessaoLoginRepository(IUnitOfWork uow, DbConnectionFactory factory, IRefreshTokenHasher hasher)
    {
        _uow = uow;
        _factory = factory;
        _hasher = hasher;
    }

    public async Task<SessaoLogin?> ObterPorTokenAsync(string token, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id                          AS "Id",
                   usuario_id                  AS "UsuarioId",
                   token                       AS "Token",
                   criado_em                   AS "CriadoEm",
                   expira_em                   AS "ExpiraEm",
                   ativo                       AS "Ativo",
                   refresh_token_hash          AS "RefreshTokenHash",
                   refresh_token_expira_em     AS "RefreshTokenExpiraEm"
            FROM sessoes_login
            WHERE token = @Token
            LIMIT 1
            """;

        using var conn = _factory.CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<SessaoRow>(
            new CommandDefinition(sql, new { Token = token }, cancellationToken: ct));

        return row is null ? null : Mapear(row);
    }

    public async Task<SessaoLogin?> ObterPorRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // Busca pelo hash SHA-256 do token, não pelo valor original
        var hash = _hasher.Hash(refreshToken);

        // SELECT FOR UPDATE garante exclusividade — evita corrida em rotações simultâneas
        const string sql = """
            SELECT id                          AS "Id",
                   usuario_id                  AS "UsuarioId",
                   token                       AS "Token",
                   criado_em                   AS "CriadoEm",
                   expira_em                   AS "ExpiraEm",
                   ativo                       AS "Ativo",
                   refresh_token_hash          AS "RefreshTokenHash",
                   refresh_token_expira_em     AS "RefreshTokenExpiraEm"
            FROM sessoes_login
            WHERE refresh_token_hash = @Hash
              AND ativo = TRUE
            LIMIT 1
            FOR UPDATE
            """;

        var row = await _uow.Connection.QueryFirstOrDefaultAsync<SessaoRow>(
            new CommandDefinition(sql, new { Hash = hash },
                transaction: _uow.Transaction, cancellationToken: ct));

        return row is null ? null : Mapear(row);
    }

    public async Task<IEnumerable<SessaoLogin>> ListarAtivasPorUsuarioAsync(
        Guid usuarioId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id                          AS "Id",
                   usuario_id                  AS "UsuarioId",
                   token                       AS "Token",
                   criado_em                   AS "CriadoEm",
                   expira_em                   AS "ExpiraEm",
                   ativo                       AS "Ativo",
                   refresh_token_hash          AS "RefreshTokenHash",
                   refresh_token_expira_em     AS "RefreshTokenExpiraEm"
            FROM sessoes_login
            WHERE usuario_id = @UsuarioId AND ativo = TRUE
            """;

        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<SessaoRow>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId }, cancellationToken: ct));

        return rows.Select(Mapear).ToList();
    }

    public async Task AdicionarAsync(SessaoLogin sessao, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO sessoes_login
                (id, usuario_id, token, criado_em, expira_em, ativo, refresh_token_hash, refresh_token_expira_em)
            VALUES
                (@Id, @UsuarioId, @Token, @CriadoEm, @ExpiraEm, @Ativo, @RefreshTokenHash, @RefreshTokenExpiraEm)
            """;

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                sessao.Id,
                sessao.UsuarioId,
                sessao.Token,
                sessao.CriadoEm,
                sessao.ExpiraEm,
                sessao.Ativo,
                sessao.RefreshTokenHash,
                sessao.RefreshTokenExpiraEm
            }, transaction: _uow.Transaction, cancellationToken: ct));
    }

    public async Task SalvarAlteracoesAsync(CancellationToken ct = default)
    {
        // Invalidações de sessão são persistidas via UPDATE direto no UoW.
        // Chamado pelo LogoffUseCase e ExcluirContaUseCase após Invalidar().
        var sessoesInvalidadas = _sessoesModificadas.Where(s => !s.Ativo).ToList();

        if (sessoesInvalidadas.Count == 0) return;

        const string sql = """
            UPDATE sessoes_login SET ativo = FALSE WHERE id = @Id
            """;

        foreach (var sessao in sessoesInvalidadas)
        {
            await _uow.Connection.ExecuteAsync(
                new CommandDefinition(sql, new { sessao.Id },
                    transaction: _uow.Transaction, cancellationToken: ct));
        }

        _sessoesModificadas.Clear();
    }

    // Rastreia sessões carregadas para poder persistir mudanças
    private readonly List<SessaoLogin> _sessoesModificadas = [];

    // ----- Mapeamento -----

#pragma warning disable CA1812
    private sealed class SessaoRow
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public DateTime ExpiraEm { get; set; }
        public bool Ativo { get; set; }
        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiraEm { get; set; }
    }
#pragma warning restore CA1812

    private SessaoLogin Mapear(SessaoRow row)
    {
        var s = (SessaoLogin)System.Runtime.CompilerServices
            .RuntimeHelpers.GetUninitializedObject(typeof(SessaoLogin));

        Set(s, nameof(SessaoLogin.Id), row.Id);
        Set(s, nameof(SessaoLogin.UsuarioId), row.UsuarioId);
        Set(s, nameof(SessaoLogin.Token), row.Token);
        Set(s, nameof(SessaoLogin.CriadoEm), row.CriadoEm);
        Set(s, nameof(SessaoLogin.ExpiraEm), row.ExpiraEm);
        Set(s, nameof(SessaoLogin.Ativo), row.Ativo);
        Set(s, nameof(SessaoLogin.RefreshTokenHash), row.RefreshTokenHash);
        Set(s, nameof(SessaoLogin.RefreshTokenExpiraEm), row.RefreshTokenExpiraEm);

        _sessoesModificadas.Add(s);
        return s;
    }

    private static void Set(object obj, string prop, object? valor)
    {
        typeof(SessaoLogin)
            .GetProperty(prop,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance)
            ?.SetValue(obj, valor);
    }
}
