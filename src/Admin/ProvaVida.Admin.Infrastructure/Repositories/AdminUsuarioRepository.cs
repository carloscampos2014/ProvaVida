using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Admin.Infrastructure.Repositories;

/// <summary>
/// Repositório PostgreSQL de usuários para o painel Admin, implementado via Dapper.
/// </summary>
/// <remarks>
/// As SQLs usam snake_case para corresponder às colunas da tabela <c>usuarios</c>.
/// O upsert utiliza <c>ON CONFLICT (id) DO UPDATE SET</c> para garantir idempotência.
/// Somente leitura é necessária para o Admin (RF-04), mas a implementação completa do
/// contrato <see cref="IUsuarioRepository"/> é mantida para consistência arquitetural.
/// </remarks>
public class AdminUsuarioRepository : DapperRepository<Usuario>, IUsuarioRepository
{
    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões PostgreSQL do Admin.
    /// </summary>
    /// <param name="factory">Fábrica de conexões PostgreSQL.</param>
    public AdminUsuarioRepository(IDbConnectionFactory factory) : base(factory) { }

    /// <inheritdoc/>
    protected override string SelectByIdSql => @"
        SELECT id, nome, email, whatsapp,
               senha_hash                      AS SenhaHash,
               contato_emergencia_nome         AS ContatoEmergenciaNome,
               contato_emergencia_email        AS ContatoEmergenciaEmail,
               contato_emergencia_whatsapp     AS ContatoEmergenciaWhatsapp,
               criado_em                       AS CriadoEm,
               atualizado_em                   AS AtualizadoEm
        FROM usuarios
        WHERE id = @Id";

    /// <inheritdoc/>
    protected override string SelectAllSql => @"
        SELECT id, nome, email, whatsapp,
               senha_hash                      AS SenhaHash,
               contato_emergencia_nome         AS ContatoEmergenciaNome,
               contato_emergencia_email        AS ContatoEmergenciaEmail,
               contato_emergencia_whatsapp     AS ContatoEmergenciaWhatsapp,
               criado_em                       AS CriadoEm,
               atualizado_em                   AS AtualizadoEm
        FROM usuarios
        ORDER BY criado_em DESC";

    /// <inheritdoc/>
    protected override string UpsertSql => @"
        INSERT INTO usuarios (
            id, nome, email, whatsapp, senha_hash,
            contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp,
            criado_em, atualizado_em
        ) VALUES (
            @Id, @Nome, @Email, @Whatsapp, @SenhaHash,
            @ContatoEmergenciaNome, @ContatoEmergenciaEmail, @ContatoEmergenciaWhatsapp,
            @CriadoEm, @AtualizadoEm
        )
        ON CONFLICT (id) DO UPDATE SET
            nome                        = EXCLUDED.nome,
            email                       = EXCLUDED.email,
            whatsapp                    = EXCLUDED.whatsapp,
            senha_hash                  = EXCLUDED.senha_hash,
            contato_emergencia_nome     = EXCLUDED.contato_emergencia_nome,
            contato_emergencia_email    = EXCLUDED.contato_emergencia_email,
            contato_emergencia_whatsapp = EXCLUDED.contato_emergencia_whatsapp,
            atualizado_em               = EXCLUDED.atualizado_em";

    /// <inheritdoc/>
    protected override string DeleteSql => "DELETE FROM usuarios WHERE id = @Id";
}
