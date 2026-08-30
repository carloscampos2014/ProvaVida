using DotNet.Testcontainers.Builders;
using FluentAssertions;
using ProvaVida.Api.Infrastructure;
using ProvaVida.Api.Infrastructure.Repositories;
using ProvaVida.Shared.Entities;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProvaVida.Api.IntegrationTests;

/// <summary>
/// Testes de integração para os repositórios PostgreSQL usando Testcontainers.
/// Requer Docker disponível na máquina. Use <c>--filter "Category!=Integration"</c>
/// para pular estes testes em ambientes sem Docker.
/// </summary>
[Trait("Category", "Integration")]
public class PostgresRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private PostgresConnectionFactory _factory = null!;

    public PostgresRepositoryIntegrationTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("provavida_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    /// <summary>Inicializa o container e executa as migrations antes dos testes.</summary>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = new PostgresConnectionFactory(_postgres.GetConnectionString());
        await ExecutarMigrationsAsync();
    }

    /// <summary>Para o container após os testes.</summary>
    public async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }

    [Fact]
    public async Task PostgresUsuarioRepository_UpsertAsync_DeveInserirERecuperar()
    {
        // Arrange
        var repo = new PostgresUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();

        // Act
        var upsertResult = await repo.UpsertAsync(usuario);
        var getResult = await repo.GetByIdAsync(usuario.Id);

        // Assert
        upsertResult.Success.Should().BeTrue(because: upsertResult.MessageErro);
        getResult.Success.Should().BeTrue(because: getResult.MessageErro);
        getResult.Data.Should().NotBeNull();
        getResult.Data!.Id.Should().Be(usuario.Id);
        getResult.Data.Nome.Should().Be(usuario.Nome);
        getResult.Data.Email.Should().Be(usuario.Email);
        getResult.Data.SenhaHash.Should().Be(usuario.SenhaHash);
    }

    [Fact]
    public async Task PostgresUsuarioRepository_GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        // Arrange
        var repo = new PostgresUsuarioRepository(_factory);
        var idInexistente = Guid.NewGuid();

        // Act
        var result = await repo.GetByIdAsync(idInexistente);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PostgresCheckinRepository_UpsertAsync_DeveInserirERecuperar()
    {
        // Arrange
        var usuarioRepo = new PostgresUsuarioRepository(_factory);
        var checkinRepo = new PostgresCheckinRepository(_factory);

        var usuario = CriarUsuarioValido();
        await usuarioRepo.UpsertAsync(usuario);

        var checkin = CriarCheckinValido(usuario.Id);

        // Act
        var upsertResult = await checkinRepo.UpsertAsync(checkin);
        var getResult = await checkinRepo.GetByIdAsync(checkin.Id);

        // Assert
        upsertResult.Success.Should().BeTrue(because: upsertResult.MessageErro);
        getResult.Success.Should().BeTrue(because: getResult.MessageErro);
        getResult.Data.Should().NotBeNull();
        getResult.Data!.Id.Should().Be(checkin.Id);
        getResult.Data.UsuarioId.Should().Be(usuario.Id);
        getResult.Data.Latitude.Should().BeApproximately(checkin.Latitude, 0.0001);
        getResult.Data.Longitude.Should().BeApproximately(checkin.Longitude, 0.0001);
        getResult.Data.IdentificacaoAparelho.Should().Be(checkin.IdentificacaoAparelho);
    }

    // --- Helpers ---

    private static Usuario CriarUsuarioValido() => new()
    {
        Id = Guid.NewGuid(),
        Nome = "João Teste",
        Email = $"joao_{Guid.NewGuid():N}@teste.com",
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Maria Teste",
        ContatoEmergenciaEmail = "maria@teste.com",
        ContatoEmergenciaWhatsapp = "11988888888",
        CriadoEm = DateTimeOffset.UtcNow,
        AtualizadoEm = DateTimeOffset.UtcNow
    };

    private static Checkin CriarCheckinValido(Guid usuarioId) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        Data = DateOnly.FromDateTime(DateTime.UtcNow),
        Latitude = -23.5505,
        Longitude = -46.6333,
        IdentificacaoAparelho = "device-integration-test",
        Sincronizado = false,
        CriadoEm = DateTimeOffset.UtcNow
    };

    private async Task ExecutarMigrationsAsync()
    {
        // Lê os scripts de migration do assembly de Infrastructure via recursos embarcados
        // e executa diretamente na conexão do container de teste.
        using var conn = _factory.Create();
        conn.Open();

        var assembly = typeof(PostgresConnectionFactory).Assembly;
        var migrationScripts = assembly
            .GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql"))
            .OrderBy(n => n)
            .ToList();

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
