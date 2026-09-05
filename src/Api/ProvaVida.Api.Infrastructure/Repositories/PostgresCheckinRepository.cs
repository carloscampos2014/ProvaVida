using Dapper;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Infrastructure.Repositories;

/// <summary>
/// Repositório PostgreSQL de check-ins, implementado via Dapper.
/// </summary>
/// <remarks>
/// As SQLs usam snake_case para corresponder às colunas da tabela <c>checkins</c>.
/// O upsert utiliza <c>ON CONFLICT (id) DO UPDATE SET</c> para garantir idempotência.
/// Registra <see cref="DateOnlyTypeHandler"/> para suporte ao tipo <c>DateOnly</c> no Dapper.
/// </remarks>
public class PostgresCheckinRepository : DapperRepository<Checkin>, ICheckinRepository
{
    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões PostgreSQL.
    /// Registra o type handler de <see cref="System.DateOnly"/> no Dapper.
    /// </summary>
    /// <param name="factory">Fábrica de conexões PostgreSQL.</param>
    public PostgresCheckinRepository(IDbConnectionFactory factory) : base(factory)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
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
        INSERT INTO checkins (
            id, usuario_id, data, latitude, longitude,
            identificacao_aparelho, sincronizado, criado_em
        ) VALUES (
            @Id, @UsuarioId, @Data, @Latitude, @Longitude,
            @IdentificacaoAparelho, @Sincronizado, @CriadoEm
        )
        ON CONFLICT (id) DO UPDATE SET
            latitude               = EXCLUDED.latitude,
            longitude              = EXCLUDED.longitude,
            identificacao_aparelho = EXCLUDED.identificacao_aparelho,
            sincronizado           = EXCLUDED.sincronizado";

    /// <inheritdoc/>
    protected override string DeleteSql => "DELETE FROM checkins WHERE id = @Id";
}
