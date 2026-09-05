using Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using ProvaVida.Mobile.Infrastructure.Data;
using ProvaVida.Mobile.Infrastructure.Repositories;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.IntegrationTests;

/// <summary>
/// Testes de integração para <see cref="SqliteUsuarioRepository"/> e
/// <see cref="SqliteCheckinRepository"/> usando SQLite em arquivo temporário.
/// </summary>
/// <remarks>
/// Cada instância de teste cria um banco SQLite em <c>Path.GetTempPath()</c>
/// com um nome único via <see cref="Guid"/>, garantindo isolamento total entre testes.
/// O arquivo é removido após cada teste via <see cref="IAsyncLifetime"/>.
/// </remarks>
public class SqliteRepositoryIntegrationTests : IAsyncLifetime
{
    private string _dbPath = string.Empty;
    private SqliteConnectionFactory _factory = null!;

    /// <summary>Cria banco temporário e tabelas antes de cada teste.</summary>
    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"provavida_test_{Guid.NewGuid()}.db");
        _factory = new SqliteConnectionFactory(_dbPath);

        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();

        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS usuarios (
                id TEXT PRIMARY KEY NOT NULL,
                nome TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE,
                whatsapp TEXT NOT NULL,
                senha_hash TEXT NOT NULL,
                contato_emergencia_nome TEXT NOT NULL,
                contato_emergencia_email TEXT NOT NULL,
                contato_emergencia_whatsapp TEXT NOT NULL,
                criado_em TEXT NOT NULL,
                atualizado_em TEXT NOT NULL
            )");

        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS checkins (
                id TEXT PRIMARY KEY NOT NULL,
                usuario_id TEXT NOT NULL,
                data TEXT NOT NULL,
                latitude REAL NOT NULL,
                longitude REAL NOT NULL,
                identificacao_aparelho TEXT NOT NULL,
                sincronizado INTEGER NOT NULL DEFAULT 0,
                criado_em TEXT NOT NULL,
                FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE,
                UNIQUE (usuario_id, data)
            )");
    }

    /// <summary>Remove o arquivo de banco temporário após cada teste.</summary>
    public Task DisposeAsync()
    {
        // Libera todos os pools de conexão do SQLite antes de deletar o arquivo,
        // evitando IOException por lock de processo.
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
        return Task.CompletedTask;
    }

    // ── UsuarioRepository ──────────────────────────────────────────────────

    [Fact]
    public async Task UsuarioRepository_UpsertAsync_DeveInserirERecuperarPorId()
    {
        var repo = new SqliteUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();

        var upsertResult = await repo.UpsertAsync(usuario);
        var getResult = await repo.GetByIdAsync(usuario.Id);

        upsertResult.Success.Should().BeTrue();
        getResult.Success.Should().BeTrue();
        getResult.Data!.Id.Should().Be(usuario.Id);
        getResult.Data.Nome.Should().Be(usuario.Nome);
        getResult.Data.Email.Should().Be(usuario.Email);
        getResult.Data.SenhaHash.Should().Be(usuario.SenhaHash);
    }

    [Fact]
    public async Task UsuarioRepository_GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new SqliteUsuarioRepository(_factory);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UsuarioRepository_GetAllAsync_DeveRetornarListaComItensInseridos()
    {
        var repo = new SqliteUsuarioRepository(_factory);
        var u1 = CriarUsuarioValido();
        var u2 = CriarUsuarioValido(email: "outro@exemplo.com");

        await repo.UpsertAsync(u1);
        await repo.UpsertAsync(u2);

        var result = await repo.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task UsuarioRepository_UpsertAsync_DeveAtualizarRegistroExistente()
    {
        var repo = new SqliteUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();
        await repo.UpsertAsync(usuario);

        usuario.Nome = "Nome Atualizado";
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        var upsertResult = await repo.UpsertAsync(usuario);
        var getResult = await repo.GetByIdAsync(usuario.Id);

        upsertResult.Success.Should().BeTrue();
        getResult.Success.Should().BeTrue();
        getResult.Data!.Nome.Should().Be("Nome Atualizado");
    }

    [Fact]
    public async Task UsuarioRepository_DeleteAsync_DeveRemoverRegistro()
    {
        var repo = new SqliteUsuarioRepository(_factory);
        var usuario = CriarUsuarioValido();
        await repo.UpsertAsync(usuario);

        var deleteResult = await repo.DeleteAsync(usuario.Id);
        var getResult = await repo.GetByIdAsync(usuario.Id);

        deleteResult.Success.Should().BeTrue();
        getResult.Success.Should().BeFalse();
    }

    // ── CheckinRepository ──────────────────────────────────────────────────

    [Fact]
    public async Task CheckinRepository_UpsertAsync_DeveInserirERecuperarPorId()
    {
        var usuarioRepo = new SqliteUsuarioRepository(_factory);
        var checkinRepo = new SqliteCheckinRepository(_factory);

        var usuario = CriarUsuarioValido();
        await usuarioRepo.UpsertAsync(usuario);

        var checkin = CriarCheckinValido(usuario.Id);
        var upsertResult = await checkinRepo.UpsertAsync(checkin);
        var getResult = await checkinRepo.GetByIdAsync(checkin.Id);

        upsertResult.Success.Should().BeTrue();
        getResult.Success.Should().BeTrue();
        getResult.Data!.Id.Should().Be(checkin.Id);
        getResult.Data.UsuarioId.Should().Be(usuario.Id);
        getResult.Data.Latitude.Should().BeApproximately(checkin.Latitude, 0.0001);
        getResult.Data.Sincronizado.Should().BeFalse();
    }

    [Fact]
    public async Task CheckinRepository_GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new SqliteCheckinRepository(_factory);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CheckinRepository_GetAllAsync_DeveRetornarListaComItensInseridos()
    {
        var usuarioRepo = new SqliteUsuarioRepository(_factory);
        var checkinRepo = new SqliteCheckinRepository(_factory);

        var usuario = CriarUsuarioValido();
        await usuarioRepo.UpsertAsync(usuario);

        var c1 = CriarCheckinValido(usuario.Id, DateOnly.FromDateTime(DateTime.UtcNow));
        var c2 = CriarCheckinValido(usuario.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        await checkinRepo.UpsertAsync(c1);
        await checkinRepo.UpsertAsync(c2);

        var result = await checkinRepo.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task CheckinRepository_DeleteAsync_DeveRemoverRegistro()
    {
        var usuarioRepo = new SqliteUsuarioRepository(_factory);
        var checkinRepo = new SqliteCheckinRepository(_factory);

        var usuario = CriarUsuarioValido();
        await usuarioRepo.UpsertAsync(usuario);
        var checkin = CriarCheckinValido(usuario.Id);
        await checkinRepo.UpsertAsync(checkin);

        var deleteResult = await checkinRepo.DeleteAsync(checkin.Id);
        var getResult = await checkinRepo.GetByIdAsync(checkin.Id);

        deleteResult.Success.Should().BeTrue();
        getResult.Success.Should().BeFalse();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Usuario CriarUsuarioValido(string email = "joao@exemplo.com") => new()
    {
        Id = Guid.NewGuid(),
        Nome = "João Silva",
        Email = email,
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Maria Silva",
        ContatoEmergenciaEmail = "maria@exemplo.com",
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
        IdentificacaoAparelho = "device-test-123",
        Sincronizado = false,
        CriadoEm = DateTimeOffset.UtcNow
    };
}
