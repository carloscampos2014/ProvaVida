namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Gerencia o ciclo de vida dos tokens JWT, renovando silenciosamente quando necessário.
/// </summary>
/// <remarks>
/// Fluxo de renovação:
/// <list type="number">
///   <item>Token válido → retorna o access token diretamente.</item>
///   <item>Token expirado → tenta <see cref="RefreshTokenAsync"/>.</item>
///   <item>Refresh inválido → tenta <see cref="SilentLoginAsync"/> com o hash salvo no SQLite.</item>
///   <item>Falha total → limpa sessão e redireciona para a tela de Login.</item>
/// </list>
/// </remarks>
public interface ITokenManager
{
    /// <summary>
    /// Retorna o access token válido, renovando silenciosamente se necessário.
    /// </summary>
    /// <returns>
    /// O access token ativo, ou <see cref="string.Empty"/> se não for possível autenticar.
    /// Quando retorna vazio, a sessão foi limpa e o app foi redirecionado para Login.
    /// </returns>
    Task<string> GetAccessTokenAsync();

    /// <summary>
    /// Tenta renovar os tokens via <c>POST /auth/refresh</c>.
    /// </summary>
    /// <returns><c>true</c> se a renovação foi bem-sucedida e os novos tokens foram salvos; <c>false</c> caso contrário.</returns>
    Task<bool> RefreshTokenAsync();

    /// <summary>
    /// Tenta login silencioso usando o hash SHA-256 salvo no SQLite local.
    /// </summary>
    /// <returns><c>true</c> se o login silencioso foi bem-sucedido e os novos tokens foram salvos; <c>false</c> caso contrário.</returns>
    Task<bool> SilentLoginAsync();
}
