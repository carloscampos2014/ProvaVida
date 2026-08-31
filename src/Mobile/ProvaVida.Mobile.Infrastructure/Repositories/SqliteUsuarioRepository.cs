using Dapper;
using ProvaVida.Mobile.Infrastructure.Data;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Infrastructure.Repositories;

/// <summary>
/// Repositório SQLite de usuários, implementado via Dapper.
/// </summary>
/// <remarks>
/// As SQLs usam snake_case para corresponder às colunas da tabela <c>usuarios</c>.
/// O upsert utiliza <c>INSERT OR REPLACE</c> para garantir idempotência no SQLite.
/// O campo <c>id</c> é armazenado como TEXT (representação do <see cref="Guid"/>).
/// Registra <see cref="GuidTypeHandler"/> e <see cref="DateTimeOffsetTypeHandler"/>
/// para suporte correto de tipos no Dapper com SQLite.
/// </remarks>
public class SqliteUsuarioRepository : DapperRepository<Usuario>, IUsuarioRepository
{
    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões SQLite.
    /// Registra os type handlers necessários para Dapper com SQLite.
    /// </summary>
    /// <param name="factory">Fábrica de conexões SQLite.</param>
    public SqliteUsuarioRepository(IDbConnectionFactory factory) : base(factory)
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
    }

    /// <inheritdoc/>
    protected override string SelectByIdSql => @"
        SELECT id, nome, email, whatsapp,
               senha_hash                  AS SenhaHash,
               contato_emergencia_nome     AS ContatoEmergenciaNome,
               contato_emergencia_email    AS ContatoEmergenciaEmail,
               contato_emergencia_whatsapp AS ContatoEmergenciaWhatsapp,
               criado_em                  AS CriadoEm,
               atualizado_em              AS AtualizadoEm
        FROM usuarios
        WHERE id = @Id";

    /// <inheritdoc/>
    protected override string SelectAllSql => @"
        SELECT id, nome, email, whatsapp,
               senha_hash                  AS SenhaHash,
               contato_emergencia_nome     AS ContatoEmergenciaNome,
               contato_emergencia_email    AS ContatoEmergenciaEmail,
               contato_emergencia_whatsapp AS ContatoEmergenciaWhatsapp,
               criado_em                  AS CriadoEm,
               atualizado_em              AS AtualizadoEm
        FROM usuarios
        ORDER BY criado_em DESC";

    /// <inheritdoc/>
    protected override string UpsertSql => @"
        INSERT OR REPLACE INTO usuarios (
            id, nome, email, whatsapp, senha_hash,
            contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp,
            criado_em, atualizado_em
        ) VALUES (
            @Id, @Nome, @Email, @Whatsapp, @SenhaHash,
            @ContatoEmergenciaNome, @ContatoEmergenciaEmail, @ContatoEmergenciaWhatsapp,
            @CriadoEm, @AtualizadoEm
        )";

    /// <inheritdoc/>
    protected override string DeleteSql => "DELETE FROM usuarios WHERE id = @Id";
}
