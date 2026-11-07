using Dapper;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Infrastructure.Repositories;

/// <summary>
/// Repositório PostgreSQL de usuários, implementado via Dapper.
/// </summary>
/// <remarks>
/// As SQLs usam snake_case para corresponder às colunas da tabela <c>usuarios</c>.
/// O upsert utiliza <c>ON CONFLICT (id) DO UPDATE SET</c> para garantir idempotência.
/// </remarks>
public class PostgresUsuarioRepository : DapperRepository<Usuario>, IUsuarioRepository
{
    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões PostgreSQL.
    /// </summary>
    /// <param name="factory">Fábrica de conexões PostgreSQL.</param>
    public PostgresUsuarioRepository(IDbConnectionFactory factory) : base(factory) { }

    /// <inheritdoc/>
    protected override string SelectByIdSql => @"
        SELECT id, nome, email, whatsapp,
               senha_hash                    AS SenhaHash,
               senha_salt                    AS SenhaSalt,
               contato_emergencia_nome       AS ContatoEmergenciaNome,
               contato_emergencia_email      AS ContatoEmergenciaEmail,
               contato_emergencia_whatsapp   AS ContatoEmergenciaWhatsapp,
               criado_em                     AS CriadoEm,
               atualizado_em                 AS AtualizadoEm
        FROM usuarios
        WHERE id = @Id";

    /// <inheritdoc/>
    protected override string SelectAllSql => @"
        SELECT id, nome, email, whatsapp,
               senha_hash                    AS SenhaHash,
               senha_salt                    AS SenhaSalt,
               contato_emergencia_nome       AS ContatoEmergenciaNome,
               contato_emergencia_email      AS ContatoEmergenciaEmail,
               contato_emergencia_whatsapp   AS ContatoEmergenciaWhatsapp,
               criado_em                     AS CriadoEm,
               atualizado_em                 AS AtualizadoEm
        FROM usuarios
        ORDER BY criado_em DESC";

    /// <inheritdoc/>
    protected override string UpsertSql => @"
        INSERT INTO usuarios (
            id, nome, email, whatsapp, senha_hash, senha_salt,
            contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp,
            criado_em, atualizado_em
        ) VALUES (
            @Id, @Nome, @Email, @Whatsapp, @SenhaHash, @SenhaSalt,
            @ContatoEmergenciaNome, @ContatoEmergenciaEmail, @ContatoEmergenciaWhatsapp,
            @CriadoEm, @AtualizadoEm
        )
        ON CONFLICT (id) DO UPDATE SET
            nome                        = EXCLUDED.nome,
            email                       = EXCLUDED.email,
            whatsapp                    = EXCLUDED.whatsapp,
            senha_hash                  = EXCLUDED.senha_hash,
            senha_salt                  = EXCLUDED.senha_salt,
            contato_emergencia_nome     = EXCLUDED.contato_emergencia_nome,
            contato_emergencia_email    = EXCLUDED.contato_emergencia_email,
            contato_emergencia_whatsapp = EXCLUDED.contato_emergencia_whatsapp,
            atualizado_em               = EXCLUDED.atualizado_em";

    /// <inheritdoc/>
    protected override string DeleteSql => "DELETE FROM usuarios WHERE id = @Id";

    /// <inheritdoc/>
    public async Task<Result<Usuario>> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT id, nome, email, whatsapp,
                   senha_hash                    AS SenhaHash,
                   senha_salt                    AS SenhaSalt,
                   contato_emergencia_nome       AS ContatoEmergenciaNome,
                   contato_emergencia_email      AS ContatoEmergenciaEmail,
                   contato_emergencia_whatsapp   AS ContatoEmergenciaWhatsapp,
                   criado_em                     AS CriadoEm,
                   atualizado_em                 AS AtualizadoEm
            FROM usuarios
            WHERE LOWER(email) = LOWER(@Email)
            LIMIT 1";

        try
        {
            using var conn = _factory.Create();
            var usuario = await conn.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
            return usuario is null
                ? Result<Usuario>.Fail("Usuário não encontrado.")
                : Result<Usuario>.Ok(usuario);
        }
        catch (Exception ex)
        {
            return Result<Usuario>.Fail(ex.Message);
        }
    }
}
