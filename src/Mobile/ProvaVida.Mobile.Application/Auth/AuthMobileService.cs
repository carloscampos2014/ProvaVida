using System.Net;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Http;
using ProvaVida.Mobile.Application.Utils;
using ProvaVida.Mobile.Application.Validators;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Implementação de <see cref="IAuthMobileService"/> que orquestra cadastro, login e logoff
/// do app mobile, coordenando o hash SHA-256, a API e a persistência local no SQLite.
/// </summary>
/// <remarks>
/// <para>
/// Fluxo de <see cref="LoginAsync"/>:
/// validação → hash SHA-256 → <c>POST /auth/login</c> → persistir <see cref="Usuario"/> via
/// <see cref="ISessionManager"/> e tokens via <see cref="ITokenRepository"/>.
/// </para>
/// <para>
/// Fluxo de <see cref="LogoutAsync"/>:
/// <c>POST /auth/logout</c> (best-effort) → <see cref="ISessionManager.ClearSessionAsync"/>.
/// A limpeza local ocorre mesmo se a chamada à API falhar.
/// </para>
/// </remarks>
public class AuthMobileService : IAuthMobileService
{
    private readonly IApiClient _apiClient;
    private readonly ISessionManager _sessionManager;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<AuthMobileService> _logger;

    /// <summary>
    /// Inicializa o serviço com suas dependências.
    /// </summary>
    /// <param name="apiClient">Cliente HTTP para chamadas à API ProvaVida.</param>
    /// <param name="sessionManager">Gerenciador de sessão local (SQLite).</param>
    /// <param name="tokenRepository">Repositório de tokens local (SQLite).</param>
    /// <param name="logger">Logger para diagnóstico.</param>
    public AuthMobileService(
        IApiClient apiClient,
        ISessionManager sessionManager,
        ITokenRepository tokenRepository,
        ILogger<AuthMobileService> logger)
    {
        _apiClient = apiClient;
        _sessionManager = sessionManager;
        _tokenRepository = tokenRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var validator = new CadastrarUsuarioMobileValidator();
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var erros = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result.Fail(erros);
        }

        try
        {
            await _apiClient.PostAsync<RegisterRequest, object>("auth/register", request);
            _logger.LogInformation("Cadastro bem-sucedido para {Email}.", request.Email);
            return Result.Ok();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Tentativa de cadastro com e-mail já existente: {Email}.", request.Email);
            return Result.Fail("E-mail já cadastrado.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Cadastro rejeitado pela API para {Email}: {Status}.", request.Email, ex.StatusCode);
            return Result.Fail("Dados inválidos. Verifique os campos e tente novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao cadastrar {Email}.", request.Email);
            return Result.Fail("Erro ao processar o cadastro. Tente novamente.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> LoginAsync(string email, string senha)
    {
        var validator = new LoginMobileValidator();
        var validation = await validator.ValidateAsync(new LoginMobileRequest(email, senha));
        if (!validation.IsValid)
        {
            var erros = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result.Fail(erros);
        }

        var senhaHash = Sha256Helper.ComputeHash(senha);

        try
        {
            var loginRequest = new LoginRequest(email, senhaHash);
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponseDto>("auth/login", loginRequest);

            if (response is null)
            {
                _logger.LogWarning("API retornou resposta nula no login para {Email}.", email);
                return Result.Fail("Login ou senha inválidos.");
            }

            // Persistir dados do usuário no SQLite
            var usuario = new Usuario
            {
                Id = response.UsuarioId,
                Nome = response.Nome,
                Email = response.Email,
                Whatsapp = response.Whatsapp,
                SenhaHash = senhaHash,
                ContatoEmergenciaNome = response.ContatoEmergenciaNome,
                ContatoEmergenciaEmail = response.ContatoEmergenciaEmail,
                ContatoEmergenciaWhatsapp = response.ContatoEmergenciaWhatsapp,
                CriadoEm = response.CriadoEm,
                AtualizadoEm = response.CriadoEm
            };

            await _sessionManager.SaveSessionAsync(usuario, senhaHash);

            // Persistir tokens no SQLite
            var tokenLocal = new TokenLocal
            {
                Id = Guid.NewGuid(),
                UsuarioId = response.UsuarioId,
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresInSeconds),
                SenhaHash = senhaHash,
                CriadoEm = DateTime.UtcNow
            };

            await _tokenRepository.SaveAsync(tokenLocal);

            _logger.LogInformation("Login bem-sucedido para {Email} (usuário {UsuarioId}).", email, response.UsuarioId);
            return Result.Ok();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Falha no login para {Email}: credenciais inválidas.", email);
            return Result.Fail("Login ou senha inválidos.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado no login para {Email}.", email);
            return Result.Fail("Erro ao processar o login. Tente novamente.");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> LogoutAsync()
    {
        try
        {
            var usuario = await _sessionManager.GetCurrentUserAsync();

            if (usuario is not null)
            {
                var token = await _tokenRepository.GetByUsuarioIdAsync(usuario.Id);
                if (token is not null)
                {
                    try
                    {
                        // Best-effort: revogar refresh token na API.
                        // Mesmo que falhe (token expirado, sem rede), a sessão local é limpa.
                        await _apiClient.PostAsync<RefreshTokenRequest, object>(
                            "auth/logout", new RefreshTokenRequest(token.RefreshToken));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Falha ao revogar token na API durante logout. Sessão local será limpa mesmo assim.");
                    }
                }
            }

            await _sessionManager.ClearSessionAsync();
            _logger.LogInformation("Logout realizado com sucesso.");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar logout.");
            return Result.Fail("Erro ao processar o logout.");
        }
    }
}
