using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Http;
using ProvaVida.Mobile.Application.Navigation;
using ProvaVida.Shared.Dtos;

namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Implementação de <see cref="ITokenManager"/> que gerencia o ciclo de vida dos tokens JWT
/// com renovação silenciosa, fallback para login com hash e limpeza de sessão em caso de falha total.
/// </summary>
/// <remarks>
/// Fluxo de <see cref="GetAccessTokenAsync"/>:
/// <list type="number">
///   <item>Token presente e válido → retorna o access token.</item>
///   <item>Token expirado → chama <see cref="RefreshTokenAsync"/>; sucesso → salva e retorna.</item>
///   <item>Refresh inválido → chama <see cref="SilentLoginAsync"/> com hash do SQLite; sucesso → salva e retorna.</item>
///   <item>Falha total → <c>ClearSessionAsync</c> + navegar para Login → retorna <see cref="string.Empty"/>.</item>
/// </list>
/// <para>
/// <c>TokenManager</c> fica em <c>Mobile.Application</c> e orquestra apenas abstrações —
/// sem dependência de MAUI, Infrastructure ou banco de dados.
/// </para>
/// </remarks>
public class TokenManager : ITokenManager
{
    private readonly ITokenRepository _tokenRepository;
    private readonly ISessionManager _sessionManager;
    private readonly IApiClient _apiClient;
    private readonly IAppNavigation _navigation;
    private readonly ILogger<TokenManager> _logger;

    /// <summary>
    /// Inicializa o gerenciador de tokens com suas dependências.
    /// </summary>
    /// <param name="tokenRepository">Repositório SQLite de tokens.</param>
    /// <param name="sessionManager">Gerenciador de sessão do usuário.</param>
    /// <param name="apiClient">Cliente HTTP para chamar a API.</param>
    /// <param name="navigation">Abstração de navegação para redirecionar ao Login.</param>
    /// <param name="logger">Logger para diagnóstico de falhas de autenticação.</param>
    public TokenManager(
        ITokenRepository tokenRepository,
        ISessionManager sessionManager,
        IApiClient apiClient,
        IAppNavigation navigation,
        ILogger<TokenManager> logger)
    {
        _tokenRepository = tokenRepository;
        _sessionManager = sessionManager;
        _apiClient = apiClient;
        _navigation = navigation;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GetAccessTokenAsync()
    {
        var usuario = await _sessionManager.GetCurrentUserAsync();
        if (usuario is null)
        {
            _logger.LogWarning("GetAccessTokenAsync chamado sem usuário ativo na sessão.");
            await ExecutarFalhaAuthAsync();
            return string.Empty;
        }

        var token = await _tokenRepository.GetByUsuarioIdAsync(usuario.Id);

        // Caminho 1: token presente e ainda válido
        if (token is not null && token.ExpiresAt > DateTime.UtcNow)
            return token.AccessToken;

        // Caminho 2: token expirado — tentar refresh
        if (token is not null)
        {
            var renovado = await RefreshTokenAsync();
            if (renovado)
            {
                var novoToken = await _tokenRepository.GetByUsuarioIdAsync(usuario.Id);
                return novoToken?.AccessToken ?? string.Empty;
            }
        }

        // Caminho 3: refresh falhou — tentar login silencioso
        var loginSilencioso = await SilentLoginAsync();
        if (loginSilencioso)
        {
            var novoToken = await _tokenRepository.GetByUsuarioIdAsync(usuario.Id);
            return novoToken?.AccessToken ?? string.Empty;
        }

        // Caminho 4: tudo falhou — limpar sessão e redirecionar para Login
        await ExecutarFalhaAuthAsync();
        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var usuario = await _sessionManager.GetCurrentUserAsync();
            if (usuario is null) return false;

            var token = await _tokenRepository.GetByUsuarioIdAsync(usuario.Id);
            if (token is null) return false;

            var request = new RefreshTokenRequest(token.RefreshToken);
            var response = await _apiClient.PostAsync<RefreshTokenRequest, TokenResponse>(
                "auth/refresh", request);

            if (response is null)
            {
                _logger.LogWarning("Refresh token inválido ou expirado para usuário {UsuarioId}.", usuario.Id);
                return false;
            }

            await SalvarNovosTokensAsync(usuario.Id, response, token.SenhaHash);
            _logger.LogInformation("Tokens renovados via refresh para usuário {UsuarioId}.", usuario.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao renovar token via refresh.");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SilentLoginAsync()
    {
        try
        {
            var usuario = await _sessionManager.GetCurrentUserAsync();
            if (usuario is null) return false;

            var token = await _tokenRepository.GetByUsuarioIdAsync(usuario.Id);
            if (token is null || string.IsNullOrEmpty(token.SenhaHash)) return false;

            var request = new LoginRequest(usuario.Email, token.SenhaHash);
            var response = await _apiClient.PostAsync<LoginRequest, TokenResponse>(
                "auth/login", request);

            if (response is null)
            {
                _logger.LogWarning("Login silencioso falhou para usuário {UsuarioId}.", usuario.Id);
                return false;
            }

            await SalvarNovosTokensAsync(usuario.Id, response, token.SenhaHash);
            _logger.LogInformation("Login silencioso bem-sucedido para usuário {UsuarioId}.", usuario.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao tentar login silencioso.");
            return false;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task SalvarNovosTokensAsync(Guid usuarioId, TokenResponse response, string senhaHash)
    {
        var novoToken = new TokenLocal
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresInSeconds),
            SenhaHash = senhaHash,
            CriadoEm = DateTime.UtcNow
        };

        await _tokenRepository.SaveAsync(novoToken);
    }

    private async Task ExecutarFalhaAuthAsync()
    {
        _logger.LogWarning("Falha total de autenticação. Limpando sessão e redirecionando para Login.");
        await _sessionManager.ClearSessionAsync();
        await _navigation.NavigateToLoginAsync();
    }
}
