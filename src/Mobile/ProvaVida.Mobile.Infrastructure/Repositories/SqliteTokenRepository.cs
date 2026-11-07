using Dapper;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Infrastructure.Data;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Infrastructure.Repositories;

/// <summary>
/// Repositório SQLite de tokens de autenticação, implementado via Dapper.
/// </summary>
/// <remarks>
/// As SQLs usam snake_case para corresponder às colunas da tabela <c>tokens</c>.
/// O upsert utiliza <c>INSERT OR REPLACE</c> para garantir idempotência no SQLite.
/// O campo <c>id</c> e <c>usuario_id</c> são armazenados como TEXT (representação do <see cref="Guid"/>).
/// Registra <see cref="GuidTypeHandler"/> e <see cref="DateTimeOffsetTypeHandler"/>
/// para suporte correto de tipos no Dapper com SQLite.
/// </remarks>
public class SqliteTokenRepository : DapperRepository<TokenLocal>, ITokenRepository
{
    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões SQLite.
    /// Registra os type handlers necessários para Dapper com SQLite.
    /// </summary>
    /// <param name="factory">Fábrica de conexões SQLite.</param>
    public SqliteTokenRepository(IDbConnectionFactory factory) : base(factory)
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
    }

    /// <inheritdoc/>
    protected override string SelectByIdSql => @"
        SELECT id, usuario_id AS UsuarioId, access_token AS AccessToken,
               refresh_token AS RefreshToken, expires_at AS ExpiresAt,
               senha_hash AS SenhaHash, criado_em AS CriadoEm
        FROM tokens
        WHERE id = @Id";

    /// <inheritdoc/>
    protected override string SelectAllSql => @"
        SELECT id, usuario_id AS UsuarioId, access_token AS AccessToken,
               refresh_token AS RefreshToken, expires_at AS ExpiresAt,
               senha_hash AS SenhaHash, criado_em AS CriadoEm
        FROM tokens
        ORDER BY criado_em DESC";

    /// <inheritdoc/>
    protected override string UpsertSql => @"
        INSERT OR REPLACE INTO tokens (
            id, usuario_id, access_token, refresh_token,
            expires_at, senha_hash, criado_em
        ) VALUES (
            @Id, @UsuarioId, @AccessToken, @RefreshToken,
            @ExpiresAt, @SenhaHash, @CriadoEm
        )";

    /// <inheritdoc/>
    protected override string DeleteSql => "DELETE FROM tokens WHERE id = @Id";

    /// <inheritdoc/>
    public async Task SaveAsync(TokenLocal token)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(UpsertSql, token);
    }

    /// <inheritdoc/>
    public async Task<TokenLocal?> GetByUsuarioIdAsync(Guid usuarioId)
    {
        const string sql = @"
            SELECT id, usuario_id AS UsuarioId, access_token AS AccessToken,
                   refresh_token AS RefreshToken, expires_at AS ExpiresAt,
                   senha_hash AS SenhaHash, criado_em AS CriadoEm
            FROM tokens
            WHERE usuario_id = @UsuarioId
            LIMIT 1";

        using var conn = _factory.Create();
        return await conn.QueryFirstOrDefaultAsync<TokenLocal>(sql, new { UsuarioId = usuarioId });
    }

    /// <inheritdoc/>
    public async Task DeleteByUsuarioIdAsync(Guid usuarioId)
    {
        const string sql = "DELETE FROM tokens WHERE usuario_id = @UsuarioId";

        using var conn = _factory.Create();
        await conn.ExecuteAsync(sql, new { UsuarioId = usuarioId });
    }
}
