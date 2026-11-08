using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Services;
using ProvaVida.Api.Infrastructure;
using ProvaVida.Api.Infrastructure.Repositories;
using ProvaVida.Api.Infrastructure.Services;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProvaVida.Api.IntegrationTests;

/// <summary>
/// Testes de integração do fluxo completo de autenticação: register → login → refresh → logout.
/// Verifica a integração real entre AuthApplicationService, repositórios PostgreSQL e JwtTokenService.
/// Requer Docker disponível via TCP em tcp://localhost:2375 (WSL2).
/// </summary>
[Trait("Category", "Integration")]
public class AuthFlowIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private PostgresConnectionFactory _factory = null!;
    private AuthApplicationService _service = null!;

    public AuthFlowIntegrationTests()
    {
        var dockerEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST") ?? "tcp://localhost:2375";
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDockerEndpoint(dockerEndpoint)
            .WithDatabase("provavida_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    /// <summary>Inicializa container, migrations e service antes dos testes.</summary>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = new PostgresConnectionFactory(_postgres.GetConnectionString());
        await ExecutarMigrationsAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-secret-minimo-32-chars-ok",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        var usuarioRepo = new PostgresUsuarioRepository(_factory);
        var refreshTokenRepo = new PostgresRefreshTokenRepository(
            _factory, new Mock<ILogger<PostgresRefreshTokenRepository>>().Object);
        var tokenService = new JwtTokenService(
            config, new Mock<ILogger<JwtTokenService>>().Object);
        var logger = new Mock<ILogger<AuthApplicationService>>().Object;

        _service = new AuthApplicationService(usuarioRepo, refreshTokenRepo, tokenService, logger);
    }

    /// <summary>Para o container após os testes.</summary>
    public async Task DisposeAsync() => await _postgres.StopAsync();

    [Fact]
    public async Task FluxoCompleto_RegisterLoginRefreshLogout_DeveFuncionar()
    {
        // Arrange — SHA-256 válido (64 chars)
        var sha256Hash = new string('a', 64);
        var email = $"joao_{Guid.NewGuid():N}@teste.com";
        var registerCommand = new CadastrarUsuarioCommand(
            Nome: "João Integração",
            Email: email,
            Whatsapp: "11999998888",
            SenhaHash: sha256Hash,
            ContatoEmergenciaNome: "Maria",
            ContatoEmergenciaEmail: "maria@teste.com",
            ContatoEmergenciaWhatsapp: "11988887777"
        );

        // Act — Register
        var registerResult = await _service.RegisterAsync(registerCommand);
        registerResult.Success.Should().BeTrue(because: registerResult.MessageErro);

        // Act — Login
        var loginResult = await _service.LoginAsync(new LoginCommand(email, sha256Hash));
        loginResult.Success.Should().BeTrue(because: loginResult.MessageErro);
        loginResult.Data.Should().NotBeNull();
        loginResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var refreshToken = loginResult.Data.RefreshToken;

        // Act — Refresh (deve rotacionar o token)
        var refreshResult = await _service.RefreshTokenAsync(new RefreshTokenCommand(refreshToken));
        refreshResult.Success.Should().BeTrue(because: refreshResult.MessageErro);
        refreshResult.Data!.RefreshToken.Should().NotBe(refreshToken, "token deve ser rotacionado");

        var novoRefreshToken = refreshResult.Data.RefreshToken;

        // Act — Logout
        var logoutResult = await _service.LogoutAsync(new LogoutCommand(novoRefreshToken));
        logoutResult.Success.Should().BeTrue(because: logoutResult.MessageErro);

        // Assert — token revogado não deve funcionar
        var refreshAposLogout = await _service.RefreshTokenAsync(new RefreshTokenCommand(novoRefreshToken));
        refreshAposLogout.Success.Should().BeFalse("token revogado não deve funcionar");
    }

    [Fact]
    public async Task Register_ComEmailDuplicado_DeveRetornarFalha()
    {
        var sha256Hash = new string('b', 64);
        var email = $"dup_{Guid.NewGuid():N}@teste.com";
        var command = new CadastrarUsuarioCommand(
            "João", email, "11999998888", sha256Hash, "Maria", "maria@teste.com", "11988887777");

        var primeiro = await _service.RegisterAsync(command);
        primeiro.Success.Should().BeTrue(because: primeiro.MessageErro);

        var segundo = await _service.RegisterAsync(command);
        segundo.Success.Should().BeFalse();
        segundo.MessageErro.Should().Contain("E-mail já cadastrado");
    }

    [Fact]
    public async Task Login_ComSenhaErrada_DeveRetornarFalha()
    {
        var sha256HashCorreto = new string('c', 64);
        var email = $"login_{Guid.NewGuid():N}@teste.com";
        var command = new CadastrarUsuarioCommand(
            "João", email, "11999998888", sha256HashCorreto, "Maria", "maria@teste.com", "11988887777");
        await _service.RegisterAsync(command);

        var loginResult = await _service.LoginAsync(new LoginCommand(email, new string('x', 64)));
        loginResult.Success.Should().BeFalse("senha errada deve falhar");
        loginResult.MessageErro.Should().Be("Credenciais inválidas.");
    }

    private async Task ExecutarMigrationsAsync()
    {
        var assembly = typeof(PostgresConnectionFactory).Assembly;
        var migrationScripts = assembly
            .GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql"))
            .OrderBy(n => n)
            .ToList();

        using var conn = _factory.Create();
        conn.Open();
        foreach (var resourceName in migrationScripts)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new System.IO.StreamReader(stream);
            var sql = await reader.ReadToEndAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
