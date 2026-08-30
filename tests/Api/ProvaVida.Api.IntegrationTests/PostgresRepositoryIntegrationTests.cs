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
/// Requer Docker disponível via TCP em tcp://localhost:2375 (WSL2).
/// Use <c>--filter "Category!=Integration"</c> para pular em ambientes sem Docker.
/// </summary>
[Trait("Category", "Integration")]
public class PostgresRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private PostgresConnectionFactory _factory = null!;

    public PostgresRepositoryIntegrationTests()
    {
        var dockerEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST")
            ?? "tcp://localhost:2375";

        _postgres = new PostgreSqlBuilder()
            .WithDockerEndpoint(dockerEndpoint)
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

    // ── Usuario ────────────────────────────────────────────────────────────

    [Fact]
    public async Task PostgresUsuarioRepository_UpsertAsync_DeveInserirERecuperar()
    {
        var repo = new PostgresUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();

        var upsertResult = await repo.UpsertAsync(usuario);
        var getResult = await repo.GetByIdAsync(usuario.Id);

        upsertResult.Success.Should().BeTrue(because: upsertResult.MessageErro);
        getResult.Success.Should().BeTrue(because: getResult.MessageErro);
        getResult.Data!.Id.Should().Be(usuario.Id);
        getResult.Data.Nome.Should().Be(usuario.Nome);
        getResult.Data.Email.Should().Be(usuario.Email);
        getResult.Data.SenhaHash.Should().Be(usuario.SenhaHash);
    }

    [Fact]
    public async Task PostgresUsuarioRepository_GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new PostgresUsuarioRepository(_factory);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PostgresUsuarioRepository_GetAllAsync_DeveRetornarListaComItensInseridos()
    {
        var repo = new PostgresUsuarioRepository(_factory);
        var u1 = CriarUsuarioValido();
        var u2 = CriarUsuarioValido();
        await repo.UpsertAsync(u1);
        await repo.UpsertAsync(u2);

        var result = await repo.GetAllAsync();

        result.Success.Should().BeTrue(because: result.MessageErro);
        result.Data.Should().NotBeNull();
        result.Data!.Should().Contain(u => u.Id == u1.Id);
        result.Data!.Should().Contain(u => u.Id == u2.Id);
    }

    [Fact]
    public async Task PostgresUsuarioRepository_DeleteAsync_DeveRemoverEntidade()
    {
        var repo = new PostgresUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();
        await repo.UpsertAsync(usuario);

        var deleteResult = await repo.DeleteAsync(usuario.Id);
        var getResult = await repo.GetByIdAsync(usuario.Id);

        deleteResult.Success.Should().BeTrue(because: deleteResult.MessageErro);
        getResult.Success.Should().BeFalse("entidade foi removida");
    }

    [Fact]
    public async Task PostgresUsuarioRepository_UpsertAsync_DeveAtualizarEntidadeExistente()
    {
        var repo = new PostgresUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();
        await repo.UpsertAsync(usuario);

        // Atualiza o nome
        usuario.Nome = "Nome Atualizado";
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        await repo.UpsertAsync(usuario);

        var getResult = await repo.GetByIdAsync(usuario.Id);

        getResult.Success.Should().BeTrue();
        getResult.Data!.Nome.Should().Be("Nome Atualizado");
    }

    // ── Checkin ────────────────────────────────────────────────────────────

    [Fact]
    public async Task PostgresCheckinRepository_UpsertAsync_DeveInserirERecuperar()
    {
        var usuarioRepo = new PostgresUsuarioRepository(_factory);
        var checkinRepo = new PostgresCheckinRepository(_factory);

        var usuario = CriarUsuarioValido();
        await usuarioRepo.UpsertAsync(usuario);
        var checkin = CriarCheckinValido(usuario.Id);

        var upsertResult = await checkinRepo.UpsertAsync(checkin);
        var getResult = await checkinRepo.GetByIdAsync(checkin.Id);

        upsertResult.Success.Should().BeTrue(because: upsertResult.MessageErro);
        getResult.Success.Should().BeTrue(because: getResult.MessageErro);
        getResult.Data!.Id.Should().Be(checkin.Id);
        getResult.Data.UsuarioId.Should().Be(usuario.Id);
        getResult.Data.Latitude.Should().BeApproximately(checkin.Latitude, 0.0001);
        getResult.Data.Longitude.Should().BeApproximately(checkin.Longitude, 0.0001);
        getResult.Data.IdentificacaoAparelho.Should().Be(checkin.IdentificacaoAparelho);
        getResult.Data.Sincronizado.Should().BeFalse();
    }

    [Fact]
    public async Task PostgresCheckinRepository_GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new PostgresCheckinRepository(_factory);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PostgresCheckinRepository_GetAllAsync_DeveRetornarListaComItensInseridos()
    {
        var usuarioRepo = new PostgresUsuarioRepository(_factory);
        var checkinRepo = new PostgresCheckinRepository(_factory);

        var usuario = CriarUsuarioValido();
        await usuarioRepo.UpsertAsync(usuario);

        var c1 = CriarCheckinValido(usuario.Id);
        var c2 = CriarCheckinValido(usuario.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        await checkinRepo.UpsertAsync(c1);
        await checkinRepo.UpsertAsync(c2);

        var result = await checkinRepo.GetAllAsync();

        result.Success.Should().BeTrue(because: result.MessageErro);
        result.Data.Should().NotBeNull();
        result.Data!.Should().Contain(c => c.Id == c1.Id);
        result.Data!.Should().Contain(c => c.Id == c2.Id);
    }

    [Fact]
    public async Task PostgresCheckinRepository_DeleteAsync_DeveRemoverEntidade()
    {
        var usuarioRepo = new PostgresUsuarioRepository(_factory);
        var checkinRepo = new PostgresCheckinRepository(_factory);

        var usuario = CriarUsuarioValido();
        await usuarioRepo.UpsertAsync(usuario);
        var checkin = CriarCheckinValido(usuario.Id);
        await checkinRepo.UpsertAsync(checkin);

        var deleteResult = await checkinRepo.DeleteAsync(checkin.Id);
        var getResult = await checkinRepo.GetByIdAsync(checkin.Id);

        deleteResult.Success.Should().BeTrue(because: deleteResult.MessageErro);
        getResult.Success.Should().BeFalse("checkin foi removido");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

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

    private static Checkin CriarCheckinValido(Guid usuarioId, DateOnly? data = null) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        Data = data ?? DateOnly.FromDateTime(DateTime.UtcNow),
        Latitude = -23.5505,
        Longitude = -46.6333,
        IdentificacaoAparelho = $"device-{Guid.NewGuid():N}",
        Sincronizado = false,
        CriadoEm = DateTimeOffset.UtcNow
    };

    private async Task ExecutarMigrationsAsync()
    {
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
