using ProvaVida.Shared.Entities;

namespace ProvaVida.Api.Domain.Interfaces;

/// <summary>
/// Contrato de repositório para gerenciamento de refresh tokens de autenticação.
/// </summary>
/// <remarks>
/// Opera sobre a tabela <c>refresh_tokens</c> no PostgreSQL. Suporta persistência,
/// busca, revogação individual e revogação em massa para um usuário.
/// </remarks>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Persiste um novo refresh token na base de dados.
    /// </summary>
    /// <param name="refreshToken">Entidade de refresh token a ser salva.</param>
    Task SaveAsync(RefreshToken refreshToken);

    /// <summary>
    /// Busca um refresh token pelo seu valor de token.
    /// </summary>
    /// <param name="token">Valor do token (string Base64) a ser buscado.</param>
    /// <returns>A entidade <see cref="RefreshToken"/> encontrada, ou <c>null</c> se não existir.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Revoga um refresh token específico, marcando-o como inativo.
    /// </summary>
    /// <param name="token">Valor do token a ser revogado.</param>
    Task RevokeAsync(string token);

    /// <summary>
    /// Revoga todos os refresh tokens ativos de um usuário.
    /// Utilizado em cenários de logout completo ou suspeita de comprometimento.
    /// </summary>
    /// <param name="usuarioId">Identificador único do usuário cujos tokens serão revogados.</param>
    Task RevokeAllByUsuarioIdAsync(Guid usuarioId);
}
