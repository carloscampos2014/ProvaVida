using Dapper;
using Microsoft.Extensions.Logging;
using ProvaVida.Api.Domain.Interfaces;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Infrastructure.Repositories;

/// <summary>
/// Repositório PostgreSQL de refresh tokens de autenticação, implementado via Dapper.
/// </summary>
/// <remarks>
/// Todas as operações abrem e fecham a conexão por demanda via <see cref="IDbConnectionFactory"/>.
/// A revogação é lógica: o registro é mantido para fins de auditoria com <c>revogado = true</c>
/// e <c>revogado_em</c> preenchido com o instante da revogação.
/// </remarks>
public class PostgresRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _factory;
    private readonly ILogger<PostgresRefreshTokenRepository> _logger;

    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões e o logger.
    /// </summary>
    /// <param name="factory">Fábrica de conexões PostgreSQL.</param>
    /// <param name="logger">Logger para registrar eventos do repositório.</param>
    public PostgresRefreshTokenRepository(
        IDbConnectionFactory factory,
        ILogger<PostgresRefreshTokenRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(RefreshToken refreshToken)
    {
        const string sql = @"
            INSERT INTO refresh_tokens (
                id, usuario_id, token, expira_em,
                revogado, criado_em, revogado_em
            ) VALUES (
                @Id, @UsuarioId, @Token, @ExpiraEm,
                @Revogado, @CriadoEm, @RevogadoEm
            )";

        try
        {
            using var conn = _factory.Create();
            await conn.ExecuteAsync(sql, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar refresh token para o usuário {UsuarioId}", refreshToken.UsuarioId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        const string sql = @"
            SELECT id,
                   usuario_id  AS UsuarioId,
                   token,
                   expira_em   AS ExpiraEm,
                   revogado,
                   criado_em   AS CriadoEm,
                   revogado_em AS RevogadoEm
            FROM refresh_tokens
            WHERE token = @Token";

        try
        {
            using var conn = _factory.Create();
            return await conn.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar refresh token");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RevokeAsync(string token)
    {
        const string sql = @"
            UPDATE refresh_tokens
            SET revogado    = TRUE,
                revogado_em = NOW()
            WHERE token = @Token
              AND revogado  = FALSE";

        try
        {
            using var conn = _factory.Create();
            var linhasAfetadas = await conn.ExecuteAsync(sql, new { Token = token });

            if (linhasAfetadas == 0)
                _logger.LogWarning("Tentativa de revogar token inexistente ou já revogado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar refresh token");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RevokeAllByUsuarioIdAsync(Guid usuarioId)
    {
        const string sql = @"
            UPDATE refresh_tokens
            SET revogado    = TRUE,
                revogado_em = NOW()
            WHERE usuario_id = @UsuarioId
              AND revogado   = FALSE";

        try
        {
            using var conn = _factory.Create();
            var linhasAfetadas = await conn.ExecuteAsync(sql, new { UsuarioId = usuarioId });
            _logger.LogWarning("Revogados {Quantidade} refresh tokens do usuário {UsuarioId}", linhasAfetadas, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar todos os refresh tokens do usuário {UsuarioId}", usuarioId);
            throw;
        }
    }
}
