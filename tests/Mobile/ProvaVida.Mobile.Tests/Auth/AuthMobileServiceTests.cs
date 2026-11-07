using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Application.Http;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.Tests.Auth;

/// <summary>
/// Testes unitários para <see cref="AuthMobileService"/> cobrindo cadastro, login e logout.
/// </summary>
public class AuthMobileServiceTests
{
    private readonly Mock<IApiClient> _mockApiClient = new();
    private readonly Mock<ISessionManager> _mockSession = new();
    private readonly Mock<ITokenRepository> _mockTokenRepo = new();
    private readonly Mock<ILogger<AuthMobileService>> _mockLogger = new();

    private AuthMobileService CriarSut() => new(
        _mockApiClient.Object,
        _mockSession.Object,
        _mockTokenRepo.Object,
        _mockLogger.Object);

    // ── RegisterAsync: sucesso ────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var request = CriarRegisterRequestValido();

        _mockApiClient
            .Setup(c => c.PostAsync<RegisterRequest, object>(
                "auth/register", It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegisterAsync(request);

        // Assert
        resultado.Success.Should().BeTrue();
        _mockApiClient.Verify(
            c => c.PostAsync<RegisterRequest, object>("auth/register", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── RegisterAsync: e-mail duplicado ──────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ComEmailDuplicado_DeveRetornarFalha()
    {
        // Arrange
        var request = CriarRegisterRequestValido();

        _mockApiClient
            .Setup(c => c.PostAsync<RegisterRequest, object>(
                "auth/register", It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Conflict", null, HttpStatusCode.Conflict));

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegisterAsync(request);

        // Assert
        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().Be("E-mail já cadastrado.");
    }

    // ── RegisterAsync: validação ──────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ComCamposObrigatoriosVazios_DeveRetornarFalha()
    {
        // Arrange — nome vazio
        var request = CriarRegisterRequestValido() with { Nome = string.Empty };
        var sut = CriarSut();

        // Act
        var resultado = await sut.RegisterAsync(request);

        // Assert
        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().NotBeNullOrWhiteSpace();
        _mockApiClient.Verify(
            c => c.PostAsync<RegisterRequest, object>(
                It.IsAny<string>(), It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ComSenhaHashInvalida_DeveRetornarFalha()
    {
        // Arrange — hash com comprimento errado (deve ter 64 chars)
        var request = CriarRegisterRequestValido() with { SenhaHash = "hash-curto" };
        var sut = CriarSut();

        // Act
        var resultado = await sut.RegisterAsync(request);

        // Assert
        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().Contain("Hash da senha inválido");
    }

    // ── LoginAsync: sucesso ───────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_DeveSalvarSessaoETokens()
    {
        // Arrange
        var email = "joao@teste.com";
        var senha = "senha123";
        var loginResponse = CriarLoginResponseDto(email);

        _mockApiClient
            .Setup(c => c.PostAsync<LoginRequest, LoginResponseDto>(
                "auth/login", It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        _mockSession
            .Setup(s => s.SaveSessionAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockTokenRepo
            .Setup(r => r.SaveAsync(It.IsAny<TokenLocal>()))
            .Returns(Task.CompletedTask);

        var sut = CriarSut();

        // Act
        var resultado = await sut.LoginAsync(email, senha);

        // Assert
        resultado.Success.Should().BeTrue();
        _mockSession.Verify(
            s => s.SaveSessionAsync(
                It.Is<Usuario>(u => u.Email == email && u.Id == loginResponse.UsuarioId),
                It.Is<string>(h => h.Length == 64)),
            Times.Once);
        _mockTokenRepo.Verify(
            r => r.SaveAsync(It.Is<TokenLocal>(t =>
                t.UsuarioId == loginResponse.UsuarioId &&
                t.AccessToken == loginResponse.AccessToken &&
                t.RefreshToken == loginResponse.RefreshToken)),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_DeveGerarHashAntesDeChamarApi()
    {
        // Arrange — verifica que o hash SHA-256 (64 chars) é enviado, não a senha em texto puro
        var email = "joao@teste.com";
        var senha = "minhasenha";
        var loginResponse = CriarLoginResponseDto(email);

        LoginRequest? requestCapturado = null;
        _mockApiClient
            .Setup(c => c.PostAsync<LoginRequest, LoginResponseDto>(
                "auth/login", It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, LoginRequest, CancellationToken>((_, req, _) => requestCapturado = req)
            .ReturnsAsync(loginResponse);
        _mockSession.Setup(s => s.SaveSessionAsync(It.IsAny<Usuario>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveAsync(It.IsAny<TokenLocal>())).Returns(Task.CompletedTask);

        var sut = CriarSut();

        // Act
        await sut.LoginAsync(email, senha);

        // Assert — hash tem 64 chars e não é igual à senha original
        requestCapturado.Should().NotBeNull();
        requestCapturado!.SenhaHash.Should().HaveLength(64);
        requestCapturado.SenhaHash.Should().NotBe(senha);
    }

    // ── LoginAsync: credenciais inválidas ─────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ComCredenciaisInvalidas_DeveRetornarFalha()
    {
        // Arrange
        _mockApiClient
            .Setup(c => c.PostAsync<LoginRequest, LoginResponseDto>(
                "auth/login", It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));

        var sut = CriarSut();

        // Act
        var resultado = await sut.LoginAsync("joao@teste.com", "senhaerrada");

        // Assert
        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().Be("Login ou senha inválidos.");
        _mockSession.Verify(
            s => s.SaveSessionAsync(It.IsAny<Usuario>(), It.IsAny<string>()),
            Times.Never);
        _mockTokenRepo.Verify(r => r.SaveAsync(It.IsAny<TokenLocal>()), Times.Never);
    }

    // ── LoginAsync: validação ─────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ComEmailVazio_DeveRetornarFalha()
    {
        var sut = CriarSut();

        var resultado = await sut.LoginAsync(string.Empty, "senha123");

        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().Contain("E-mail é obrigatório");
        _mockApiClient.Verify(
            c => c.PostAsync<LoginRequest, LoginResponseDto>(
                It.IsAny<string>(), It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ComSenhaVazia_DeveRetornarFalha()
    {
        var sut = CriarSut();

        var resultado = await sut.LoginAsync("joao@teste.com", string.Empty);

        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().Contain("Senha é obrigatória");
    }

    // ── LogoutAsync: sucesso ──────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_DeveRevogarTokenNaApiELimparSessao()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var token = CriarTokenLocalValido(usuario.Id);

        _mockSession
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(usuario);
        _mockTokenRepo
            .Setup(r => r.GetByUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(token);
        _mockApiClient
            .Setup(c => c.PostAsync<RefreshTokenRequest, object>(
                "auth/logout", It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());
        _mockSession
            .Setup(s => s.ClearSessionAsync())
            .Returns(Task.CompletedTask);

        var sut = CriarSut();

        // Act
        var resultado = await sut.LogoutAsync();

        // Assert
        resultado.Success.Should().BeTrue();
        _mockApiClient.Verify(
            c => c.PostAsync<RefreshTokenRequest, object>(
                "auth/logout",
                It.Is<RefreshTokenRequest>(r => r.RefreshToken == token.RefreshToken),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mockSession.Verify(s => s.ClearSessionAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_QuandoApiFalha_DeveAindaLimparSessaoLocal()
    {
        // Arrange — API falha (ex: token expirado), sessão local deve ser limpa mesmo assim
        var usuario = CriarUsuarioValido();
        var token = CriarTokenLocalValido(usuario.Id);

        _mockSession
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(usuario);
        _mockTokenRepo
            .Setup(r => r.GetByUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(token);
        _mockApiClient
            .Setup(c => c.PostAsync<RefreshTokenRequest, object>(
                "auth/logout", It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));
        _mockSession
            .Setup(s => s.ClearSessionAsync())
            .Returns(Task.CompletedTask);

        var sut = CriarSut();

        // Act
        var resultado = await sut.LogoutAsync();

        // Assert — sucesso local mesmo com falha na API
        resultado.Success.Should().BeTrue();
        _mockSession.Verify(s => s.ClearSessionAsync(), Times.Once);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static RegisterRequest CriarRegisterRequestValido() => new(
        Nome: "João Silva",
        Email: "joao@teste.com",
        Whatsapp: "11999999999",
        SenhaHash: new string('a', 64),
        ContatoEmergenciaNome: "Maria Silva",
        ContatoEmergenciaEmail: "maria@teste.com",
        ContatoEmergenciaWhatsapp: "11988888888"
    );

    private static LoginResponseDto CriarLoginResponseDto(string email) => new(
        UsuarioId: Guid.NewGuid(),
        Nome: "João Silva",
        Email: email,
        Whatsapp: "11999999999",
        ContatoEmergenciaNome: "Maria Silva",
        ContatoEmergenciaEmail: "maria@teste.com",
        ContatoEmergenciaWhatsapp: "11988888888",
        CriadoEm: DateTimeOffset.UtcNow,
        AccessToken: "access-token-teste",
        RefreshToken: "refresh-token-teste",
        TokenType: "Bearer",
        ExpiresInSeconds: 3600
    );

    private static Usuario CriarUsuarioValido() => new()
    {
        Id = Guid.NewGuid(),
        Nome = "João Silva",
        Email = "joao@teste.com",
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Maria Silva",
        ContatoEmergenciaEmail = "maria@teste.com",
        ContatoEmergenciaWhatsapp = "11988888888",
        CriadoEm = DateTimeOffset.UtcNow,
        AtualizadoEm = DateTimeOffset.UtcNow
    };

    private static TokenLocal CriarTokenLocalValido(Guid usuarioId) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        AccessToken = "access-token-valido",
        RefreshToken = "refresh-token-valido",
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        SenhaHash = new string('a', 64),
        CriadoEm = DateTime.UtcNow
    };
}
