using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Application.Http;
using ProvaVida.Mobile.Application.Navigation;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.Tests.Auth;

/// <summary>
/// Testes unitários para <see cref="TokenManager"/> cobrindo os 4 caminhos do fluxo de renovação.
/// </summary>
public class TokenManagerTests
{
    private readonly Mock<ITokenRepository> _mockTokenRepo = new();
    private readonly Mock<ISessionManager> _mockSession = new();
    private readonly Mock<IApiClient> _mockApiClient = new();
    private readonly Mock<IAppNavigation> _mockNavigation = new();
    private readonly Mock<ILogger<TokenManager>> _mockLogger = new();

    private TokenManager CriarSut() => new(
        _mockTokenRepo.Object,
        _mockSession.Object,
        _mockApiClient.Object,
        _mockNavigation.Object,
        _mockLogger.Object);

    // ── Caminho 1: token válido ───────────────────────────────────────────────

    [Fact]
    public async Task TokenManager_GetAccessToken_DeveRetornarToken_QuandoTokenValido()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var token = CriarTokenValido(usuario.Id);

        _mockSession
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(usuario);
        _mockTokenRepo
            .Setup(r => r.GetByUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(token);

        var sut = CriarSut();

        // Act
        var resultado = await sut.GetAccessTokenAsync();

        // Assert — deve retornar o token sem chamar a API
        resultado.Should().Be(token.AccessToken);
        _mockApiClient.Verify(
            c => c.PostAsync<RefreshTokenRequest, TokenResponse>(
                It.IsAny<string>(), It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mockTokenRepo.Verify(r => r.SaveAsync(It.IsAny<TokenLocal>()), Times.Never);
    }

    // ── Caminho 2: token expirado → refresh bem-sucedido ─────────────────────

    [Fact]
    public async Task TokenManager_GetAccessToken_DeveRenovar_QuandoTokenExpirado()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var tokenExpirado = CriarTokenExpirado(usuario.Id);
        var novoAccessToken = "novo-access-token-renovado";
        var tokenRenovado = CriarTokenValido(usuario.Id, novoAccessToken);
        var apiResponse = new TokenResponse(novoAccessToken, "novo-refresh-token", "Bearer", 3600);

        _mockSession
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(usuario);

        // Chamadas 1 e 2 retornam token expirado (GetAccessTokenAsync + RefreshTokenAsync)
        // Chamada 3 retorna token renovado (após salvar, GetAccessTokenAsync busca novamente)
        var chamadas = 0;
        _mockTokenRepo
            .Setup(r => r.GetByUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(() => ++chamadas < 3 ? tokenExpirado : tokenRenovado);

        _mockApiClient
            .Setup(c => c.PostAsync<RefreshTokenRequest, TokenResponse>(
                "auth/refresh", It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        var sut = CriarSut();

        // Act
        var resultado = await sut.GetAccessTokenAsync();

        // Assert
        resultado.Should().Be(novoAccessToken);
        _mockTokenRepo.Verify(r => r.SaveAsync(It.IsAny<TokenLocal>()), Times.Once);
        _mockApiClient.Verify(
            c => c.PostAsync<LoginRequest, TokenResponse>(
                It.IsAny<string>(), It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Caminho 3: refresh inválido → login silencioso bem-sucedido ───────────

    [Fact]
    public async Task TokenManager_GetAccessToken_DeveFazerLoginSilencioso_QuandoRefreshInvalido()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var tokenExpirado = CriarTokenExpirado(usuario.Id);
        var novoAccessToken = "access-token-via-silent-login";
        var tokenRenovado = CriarTokenValido(usuario.Id, novoAccessToken);
        var apiResponse = new TokenResponse(novoAccessToken, "novo-refresh-silent", "Bearer", 3600);

        _mockSession
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(usuario);

        // Chamadas 1, 2 e 3 retornam token expirado; chamada 4 retorna token renovado
        var chamadas = 0;
        _mockTokenRepo
            .Setup(r => r.GetByUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(() => ++chamadas < 4 ? tokenExpirado : tokenRenovado);

        // Refresh falha
        _mockApiClient
            .Setup(c => c.PostAsync<RefreshTokenRequest, TokenResponse>(
                "auth/refresh", It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenResponse?)null);

        // Login silencioso retorna tokens novos
        _mockApiClient
            .Setup(c => c.PostAsync<LoginRequest, TokenResponse>(
                "auth/login", It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        var sut = CriarSut();

        // Act
        var resultado = await sut.GetAccessTokenAsync();

        // Assert
        resultado.Should().Be(novoAccessToken);
        _mockTokenRepo.Verify(r => r.SaveAsync(It.IsAny<TokenLocal>()), Times.Once);
        _mockApiClient.Verify(
            c => c.PostAsync<LoginRequest, TokenResponse>(
                "auth/login", It.Is<LoginRequest>(r => r.Email == usuario.Email && r.SenhaHash == tokenExpirado.SenhaHash),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mockSession.Verify(s => s.ClearSessionAsync(), Times.Never);
    }

    // ── Caminho 4: tudo falha → limpar sessão + navegar para Login ────────────

    [Fact]
    public async Task TokenManager_GetAccessToken_DeveLimparSessao_QuandoTudoFalha()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var tokenExpirado = CriarTokenExpirado(usuario.Id);

        _mockSession
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(usuario);
        _mockTokenRepo
            .Setup(r => r.GetByUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(tokenExpirado);

        // Refresh falha
        _mockApiClient
            .Setup(c => c.PostAsync<RefreshTokenRequest, TokenResponse>(
                "auth/refresh", It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenResponse?)null);

        // Login silencioso falha
        _mockApiClient
            .Setup(c => c.PostAsync<LoginRequest, TokenResponse>(
                "auth/login", It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenResponse?)null);

        _mockSession.Setup(s => s.ClearSessionAsync()).Returns(Task.CompletedTask);
        _mockNavigation.Setup(n => n.NavigateToLoginAsync()).Returns(Task.CompletedTask);

        var sut = CriarSut();

        // Act
        var resultado = await sut.GetAccessTokenAsync();

        // Assert
        resultado.Should().BeEmpty();
        _mockSession.Verify(s => s.ClearSessionAsync(), Times.Once);
        _mockNavigation.Verify(n => n.NavigateToLoginAsync(), Times.Once);
        _mockTokenRepo.Verify(r => r.SaveAsync(It.IsAny<TokenLocal>()), Times.Never);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Usuario CriarUsuarioValido() => new()
    {
        Id = Guid.NewGuid(),
        Nome = "João Teste",
        Email = "joao@teste.com",
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Maria Teste",
        ContatoEmergenciaEmail = "maria@teste.com",
        ContatoEmergenciaWhatsapp = "11988888888",
        CriadoEm = DateTimeOffset.UtcNow,
        AtualizadoEm = DateTimeOffset.UtcNow
    };

    private static TokenLocal CriarTokenValido(Guid usuarioId, string accessToken = "access-token-valido") => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        AccessToken = accessToken,
        RefreshToken = "refresh-token-valido",
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        SenhaHash = new string('a', 64),
        CriadoEm = DateTime.UtcNow
    };

    private static TokenLocal CriarTokenExpirado(Guid usuarioId) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        AccessToken = "access-token-expirado",
        RefreshToken = "refresh-token-expirado",
        ExpiresAt = DateTime.UtcNow.AddHours(-1),
        SenhaHash = new string('a', 64),
        CriadoEm = DateTime.UtcNow.AddHours(-2)
    };
}
