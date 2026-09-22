using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Services;
using ProvaVida.Api.Domain.Interfaces;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Tests.UseCases;

/// <summary>
/// Testes unitários para <see cref="AuthApplicationService"/>.
/// Cobre todos os fluxos de RegisterAsync, LoginAsync, RefreshTokenAsync e LogoutAsync.
/// </summary>
public class AuthApplicationServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthApplicationService>> _loggerMock;
    private readonly AuthApplicationService _service;
    private readonly Faker _faker;

    public AuthApplicationServiceTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthApplicationService>>();
        _faker = new Faker("pt_BR");

        _service = new AuthApplicationService(
            _usuarioRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    private CadastrarUsuarioCommand CriarComandoCadastroValido() =>
        new(
            Nome: _faker.Name.FullName(),
            Email: _faker.Internet.Email(),
            Whatsapp: _faker.Random.ReplaceNumbers("##########"),
            SenhaHash: _faker.Random.AlphaNumeric(64),
            ContatoEmergenciaNome: _faker.Name.FullName(),
            ContatoEmergenciaEmail: _faker.Internet.Email(),
            ContatoEmergenciaWhatsapp: _faker.Random.ReplaceNumbers("##########")
        );

    private Usuario CriarUsuario(string email = "", string senhaHash = "") =>
        new()
        {
            Id = Guid.NewGuid(),
            Nome = _faker.Name.FullName(),
            Email = string.IsNullOrEmpty(email) ? _faker.Internet.Email() : email,
            Whatsapp = _faker.Random.ReplaceNumbers("##########"),
            SenhaHash = string.IsNullOrEmpty(senhaHash) ? _faker.Random.AlphaNumeric(64) : senhaHash,
            ContatoEmergenciaNome = _faker.Name.FullName(),
            ContatoEmergenciaEmail = _faker.Internet.Email(),
            ContatoEmergenciaWhatsapp = _faker.Random.ReplaceNumbers("##########"),
            CriadoEm = DateTimeOffset.UtcNow.AddDays(-1),
            AtualizadoEm = DateTimeOffset.UtcNow
        };

    // ─── RegisterAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var command = CriarComandoCadastroValido();
        _usuarioRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([]));
        _usuarioRepositoryMock
            .Setup(r => r.UpsertAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _service.RegisterAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(command.Email.ToLowerInvariant());
        result.Data.Nome.Should().Be(command.Nome);
    }

    [Fact]
    public async Task RegisterAsync_ComEmailDuplicado_DeveRetornarFalha()
    {
        // Arrange
        var command = CriarComandoCadastroValido();
        var usuarioExistente = CriarUsuario(email: command.Email.ToLowerInvariant());

        _usuarioRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([usuarioExistente]));

        // Act
        var result = await _service.RegisterAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("E-mail já cadastrado");
    }

    [Fact]
    public async Task RegisterAsync_ComEmailInvalido_DeveRetornarFalhaDeValidacao()
    {
        // Arrange
        var command = new CadastrarUsuarioCommand(
            Nome: _faker.Name.FullName(),
            Email: "email-invalido",
            Whatsapp: _faker.Random.ReplaceNumbers("##########"),
            SenhaHash: _faker.Random.AlphaNumeric(64),
            ContatoEmergenciaNome: _faker.Name.FullName(),
            ContatoEmergenciaEmail: _faker.Internet.Email(),
            ContatoEmergenciaWhatsapp: _faker.Random.ReplaceNumbers("##########")
        );

        // Act
        var result = await _service.RegisterAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ComSenhaHashInvalida_DeveRetornarFalha()
    {
        // Arrange — hash com tamanho errado (deve ter 64 chars)
        var command = new CadastrarUsuarioCommand(
            Nome: _faker.Name.FullName(),
            Email: _faker.Internet.Email(),
            Whatsapp: _faker.Random.ReplaceNumbers("##########"),
            SenhaHash: "hash-curto-demais",
            ContatoEmergenciaNome: _faker.Name.FullName(),
            ContatoEmergenciaEmail: _faker.Internet.Email(),
            ContatoEmergenciaWhatsapp: _faker.Random.ReplaceNumbers("##########")
        );

        // Act
        var result = await _service.RegisterAsync(command);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_QuandoRepositorioFalha_DeveRetornarFalha()
    {
        // Arrange
        var command = CriarComandoCadastroValido();
        _usuarioRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([]));
        _usuarioRepositoryMock
            .Setup(r => r.UpsertAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(Result.Fail("Erro de banco de dados"));

        // Act
        var result = await _service.RegisterAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrEmpty();
    }

    // ─── LoginAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarTokens()
    {
        // Arrange
        var senhaHash = _faker.Random.AlphaNumeric(64);
        var usuario = CriarUsuario(senhaHash: senhaHash);
        var command = new LoginCommand(Email: usuario.Email, SenhaHash: senhaHash);

        _usuarioRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([usuario]));
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<Usuario>()))
            .Returns("access-token-fake");
        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("refresh-token-fake");
        _refreshTokenRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.LoginAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access-token-fake");
        result.Data.RefreshToken.Should().Be("refresh-token-fake");
        result.Data.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task LoginAsync_ComEmailInexistente_DeveRetornarFalha()
    {
        // Arrange
        var command = new LoginCommand(
            Email: _faker.Internet.Email(),
            SenhaHash: _faker.Random.AlphaNumeric(64));

        _usuarioRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([]));

        // Act
        var result = await _service.LoginAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Credenciais inválidas.");
    }

    [Fact]
    public async Task LoginAsync_ComSenhaErrada_DeveRetornarFalha()
    {
        // Arrange
        var usuario = CriarUsuario(senhaHash: _faker.Random.AlphaNumeric(64));
        var command = new LoginCommand(
            Email: usuario.Email,
            SenhaHash: _faker.Random.AlphaNumeric(64)); // hash diferente

        _usuarioRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([usuario]));

        // Act
        var result = await _service.LoginAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Credenciais inválidas.");
    }

    [Fact]
    public async Task LoginAsync_ComEmailInvalido_DeveRetornarFalha()
    {
        // Arrange
        var command = new LoginCommand(Email: "nao-e-email", SenhaHash: _faker.Random.AlphaNumeric(64));

        // Act
        var result = await _service.LoginAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Credenciais inválidas.");
    }

    // ─── RefreshTokenAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_ComTokenValido_DeveRetornarNovosTokens()
    {
        // Arrange
        var usuario = CriarUsuario();
        var tokenAtual = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            Token = "refresh-token-atual",
            ExpiraEm = DateTimeOffset.UtcNow.AddDays(7),
            Revogado = false,
            CriadoEm = DateTimeOffset.UtcNow.AddHours(-1)
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("refresh-token-atual"))
            .ReturnsAsync(tokenAtual);
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(usuario.Id))
            .ReturnsAsync(Result<Usuario>.Ok(usuario));
        _refreshTokenRepositoryMock
            .Setup(r => r.RevokeAsync("refresh-token-atual"))
            .Returns(Task.CompletedTask);
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(usuario))
            .Returns("novo-access-token");
        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("novo-refresh-token");
        _refreshTokenRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RefreshTokenAsync(new RefreshTokenCommand("refresh-token-atual"));

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("novo-access-token");
        result.Data.RefreshToken.Should().Be("novo-refresh-token");
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAsync("refresh-token-atual"), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ComTokenRevogado_DeveRetornarFalha()
    {
        // Arrange
        var tokenRevogado = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Token = "token-revogado",
            ExpiraEm = DateTimeOffset.UtcNow.AddDays(7),
            Revogado = true,
            CriadoEm = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("token-revogado"))
            .ReturnsAsync(tokenRevogado);

        // Act
        var result = await _service.RefreshTokenAsync(new RefreshTokenCommand("token-revogado"));

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("revogado");
    }

    [Fact]
    public async Task RefreshTokenAsync_ComTokenExpirado_DeveRetornarFalha()
    {
        // Arrange
        var tokenExpirado = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Token = "token-expirado",
            ExpiraEm = DateTimeOffset.UtcNow.AddDays(-1), // expirado
            Revogado = false,
            CriadoEm = DateTimeOffset.UtcNow.AddDays(-10)
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("token-expirado"))
            .ReturnsAsync(tokenExpirado);

        // Act
        var result = await _service.RefreshTokenAsync(new RefreshTokenCommand("token-expirado"));

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("expirado");
    }

    [Fact]
    public async Task RefreshTokenAsync_ComTokenInexistente_DeveRetornarFalha()
    {
        // Arrange
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _service.RefreshTokenAsync(new RefreshTokenCommand("token-inexistente"));

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("não encontrado");
    }

    // ─── LogoutAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ComTokenValido_DeveRevogarERetornarSucesso()
    {
        // Arrange
        var tokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Token = "refresh-token-logout",
            ExpiraEm = DateTimeOffset.UtcNow.AddDays(7),
            Revogado = false,
            CriadoEm = DateTimeOffset.UtcNow.AddHours(-1)
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("refresh-token-logout"))
            .ReturnsAsync(tokenEntity);
        _refreshTokenRepositoryMock
            .Setup(r => r.RevokeAsync("refresh-token-logout"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.LogoutAsync(new LogoutCommand("refresh-token-logout"));

        // Assert
        result.Success.Should().BeTrue();
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAsync("refresh-token-logout"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ComTokenInexistente_DeveRetornarFalha()
    {
        // Arrange
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _service.LogoutAsync(new LogoutCommand("token-inexistente"));

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task LogoutAsync_ComTokenVazio_DeveRetornarFalha()
    {
        // Act
        var result = await _service.LogoutAsync(new LogoutCommand(""));

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrEmpty();
    }
}
