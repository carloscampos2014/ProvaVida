namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Contrato para persistência e recuperação de tokens de autenticação no SQLite local.
/// </summary>
/// <remarks>
/// A implementação concreta (<c>SqliteTokenRepository</c>) fica em <c>Mobile.Infrastructure</c>,
/// mantendo a interface em <c>Mobile.Application</c> para evitar ciclo de referência.
/// </remarks>
public interface ITokenRepository
{
    /// <summary>
    /// Persiste ou substitui o token de autenticação do usuário no SQLite.
    /// A operação é idempotente — usa <c>INSERT OR REPLACE</c>.
    /// </summary>
    /// <param name="token">Dados do token a ser salvo.</param>
    Task SaveAsync(TokenLocal token);

    /// <summary>
    /// Recupera o token de autenticação associado ao usuário informado.
    /// </summary>
    /// <param name="usuarioId">Identificador do usuário.</param>
    /// <returns>O token encontrado, ou <c>null</c> se não existir registro.</returns>
    Task<TokenLocal?> GetByUsuarioIdAsync(Guid usuarioId);

    /// <summary>
    /// Remove o token de autenticação do usuário informado do SQLite.
    /// </summary>
    /// <param name="usuarioId">Identificador do usuário cujo token será removido.</param>
    Task DeleteByUsuarioIdAsync(Guid usuarioId);
}
