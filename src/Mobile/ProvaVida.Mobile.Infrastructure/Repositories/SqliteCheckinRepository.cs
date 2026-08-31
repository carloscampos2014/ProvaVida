using Dapper;
using ProvaVida.Mobile.Infrastructure.Data;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Infrastructure.Repositories;

/// <summary>
/// Repositório SQLite de check-ins, implementado via Dapper.
/// </summary>
/// <remarks>
/// As SQLs usam snake_case para corresponder às colunas da tabela <c>checkins</c>.
/// O upsert utiliza <c>INSERT OR REPLACE</c> para garantir idempotência no SQLite.
/// O campo <c>id</c> e <c>usuario_id</c> são armazenados como TEXT (representação do <see cref="Guid"/>).
/// O campo <c>data</c> é armazenado como TEXT no formato <c>yyyy-MM-dd</c>.
/// O campo <c>sincronizado</c> é armazenado como INTEGER (0 = false, 1 = true).
/// Registra <see cref="GuidTypeHandler"/>, <see cref="DateOnlyTypeHandler"/>
/// e <see cref="DateTimeOffsetTypeHandler"/> para suporte correto de tipos no Dapper com SQLite.
/// </remarks>
public class SqliteCheckinRepository : DapperRepository<Checkin>, ICheckinRepository
{
    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões SQLite.
    /// Registra os type handlers necessários para Dapper com SQLite.
    /// </summary>
    /// <param name="factory">Fábrica de conexões SQLite.</param>
    public SqliteCheckinRepository(IDbConnectionFactory factory) : base(factory)
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
    }

    /// <inheritdoc/>
    protected override string SelectByIdSql => @"
        SELECT id,
               usuario_id             AS UsuarioId,
               data, latitude, longitude,
               identificacao_aparelho AS IdentificacaoAparelho,
               sincronizado,
               criado_em              AS CriadoEm
        FROM checkins
        WHERE id = @Id";

    /// <inheritdoc/>
    protected override string SelectAllSql => @"
        SELECT id,
               usuario_id             AS UsuarioId,
               data, latitude, longitude,
               identificacao_aparelho AS IdentificacaoAparelho,
               sincronizado,
               criado_em              AS CriadoEm
        FROM checkins
        ORDER BY criado_em DESC";

    /// <inheritdoc/>
    protected override string UpsertSql => @"
        INSERT OR REPLACE INTO checkins (
            id, usuario_id, data, latitude, longitude,
            identificacao_aparelho, sincronizado, criado_em
        ) VALUES (
            @Id, @UsuarioId, @Data, @Latitude, @Longitude,
            @IdentificacaoAparelho, @Sincronizado, @CriadoEm
        )";

    /// <inheritdoc/>
    protected override string DeleteSql => "DELETE FROM checkins WHERE id = @Id";
}
