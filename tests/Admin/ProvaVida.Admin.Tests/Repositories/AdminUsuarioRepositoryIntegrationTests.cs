using DotNet.Testcontainers.Builders;
using FluentAssertions;
using ProvaVida.Admin.Infrastructure;
using ProvaVida.Admin.Infrastructure.Repositories;
using ProvaVida.Shared.Entities;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProvaVida.Admin.Tests.Repositories;

/// <summary>
/// Testes de integração para <see cref="AdminUsuarioRepository"/> usando Testcontainers.
/// Requer Docker disponível via TCP em tcp://localhost:2375 (WSL2).
/// Use <c>--filter "Category!=Integration"</c> para pular em ambientes sem Docker.
/// </summary>
[Trait("Category", "Integration")]
public class AdminUsuarioRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private AdminConnectionFactory _factory = null!;

    public AdminUsuarioRepositoryIntegrationTests()
    {
        var dockerEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST")
            ?? "tcp://localhost:2375";

        _postgres = new PostgreSqlBuilder()
            .WithDockerEndpoint(dockerEndpoint)
            .WithImage("postgres:16-alpine")
            .WithDatabase("provavida_admin_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    /// <summary>Inicializa o container e cria as tabelas antes dos testes.</summary>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = new AdminConnectionFactory(_postgres.GetConnectionString());
        await CriarTabelasAsync();
    }

    /// <summary>Para o container após os testes.</summary>
    public async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }

    [Fact]
    public async Task AdminUsuarioRepository_GetAllAsync_DeveRetornarListaVaziaInicial()
    {
        var repo = new AdminUsuarioRepository(_factory);

        var result = await repo.GetAllAsync();

        result.Success.Should().BeTrue(because: result.MessageErro);
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task AdminUsuarioRepository_UpsertAsync_DeveInserirERecuperar()
    {
        var repo = new AdminUsuarioRepository(_factory);
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
    public async Task AdminUsuarioRepository_GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new AdminUsuarioRepository(_factory);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AdminUsuarioRepository_GetAllAsync_DeveRetornarTodosOsInseridos()
    {
        var repo = new AdminUsuarioRepository(_factory);
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
    public async Task AdminUsuarioRepository_DeleteAsync_DeveRemoverEntidade()
    {
        var repo = new AdminUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();
        await repo.UpsertAsync(usuario);

        var deleteResult = await repo.DeleteAsync(usuario.Id);
        var getResult = await repo.GetByIdAsync(usuario.Id);

        deleteResult.Success.Should().BeTrue(because: deleteResult.MessageErro);
        getResult.Success.Should().BeFalse("entidade foi removida");
    }

    [Fact]
    public async Task AdminUsuarioRepository_UpsertAsync_DeveAtualizarEntidadeExistente()
    {
        var repo = new AdminUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();
        await repo.UpsertAsync(usuario);

        usuario.Nome = "Nome Atualizado Admin";
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        await repo.UpsertAsync(usuario);

        var getResult = await repo.GetByIdAsync(usuario.Id);

        getResult.Success.Should().BeTrue();
        getResult.Data!.Nome.Should().Be("Nome Atualizado Admin");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Usuario CriarUsuarioValido() => new()
    {
        Id = Guid.NewGuid(),
        Nome = "Admin Teste",
        Email = $"admin_{Guid.NewGuid():N}@teste.com",
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Contato Admin",
        ContatoEmergenciaEmail = "contato@teste.com",
        ContatoEmergenciaWhatsapp = "11988888888",
        CriadoEm = DateTimeOffset.UtcNow,
        AtualizadoEm = DateTimeOffset.UtcNow
    };

    private async Task CriarTabelasAsync()
    {
        using var conn = _factory.Create();
        conn.Open();

        const string sql = @"
            CREATE TABLE IF NOT EXISTS usuarios (
                id                          UUID        PRIMARY KEY,
                nome                        TEXT        NOT NULL,
                email                       TEXT        NOT NULL UNIQUE,
                whatsapp                    TEXT        NOT NULL,
                senha_hash                  TEXT        NOT NULL,
                contato_emergencia_nome     TEXT        NOT NULL DEFAULT '',
                contato_emergencia_email    TEXT        NOT NULL DEFAULT '',
                contato_emergencia_whatsapp TEXT        NOT NULL DEFAULT '',
                criado_em                   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                atualizado_em               TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();

        await Task.CompletedTask;
    }
}
