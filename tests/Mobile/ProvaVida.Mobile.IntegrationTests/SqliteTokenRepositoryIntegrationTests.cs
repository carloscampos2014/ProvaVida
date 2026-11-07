using Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Infrastructure.Data;
using ProvaVida.Mobile.Infrastructure.Repositories;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.IntegrationTests;

/// <summary>
/// Testes de integração para <see cref="SqliteTokenRepository"/> usando SQLite em arquivo temporário.
/// </summary>
/// <remarks>
/// Cada instância cria um banco SQLite isolado em <c>Path.GetTempPath()</c>.
/// A tabela <c>usuarios</c> é criada para satisfazer a FK de <c>tokens.usuario_id</c>.
/// </remarks>
public class SqliteTokenRepositoryIntegrationTests : IAsyncLifetime
{
    private string _dbPath = string.Empty;
    private SqliteConnectionFactory _factory = null!;

    /// <summary>Cria banco temporário com as tabelas necessárias antes de cada teste.</summary>
    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"provavida_token_test_{Guid.NewGuid()}.db");
        _factory = new SqliteConnectionFactory(_dbPath);

        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();

        // tabela usuarios necessária para a FK de tokens
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
            CREATE TABLE IF NOT EXISTS tokens (
                id            TEXT PRIMARY KEY,
                usuario_id    TEXT NOT NULL,
                access_token  TEXT NOT NULL,
                refresh_token TEXT NOT NULL,
                expires_at    TEXT NOT NULL,
                senha_hash    TEXT NOT NULL,
                criado_em     TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
            )");
    }

    /// <summary>Remove o arquivo de banco temporário após cada teste.</summary>
    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
        return Task.CompletedTask;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static TokenLocal CriarTokenValido(Guid? usuarioId = null) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId ?? Guid.NewGuid(),
        AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.access",
        RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.refresh",
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        SenhaHash = new string('a', 64),
        CriadoEm = DateTime.UtcNow
    };

    /// <summary>
    /// Insere um usuário via <see cref="SqliteUsuarioRepository"/> para garantir
    /// que o mesmo GuidTypeHandler e factory sejam usados, evitando inconsistências de FK.
    /// </summary>
    private async Task InserirUsuarioAsync(Guid usuarioId)
    {
        var usuarioRepo = new SqliteUsuarioRepository(_factory);
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Teste Token",
            Email = $"{usuarioId}@exemplo.com",
            Whatsapp = "11999999999",
            SenhaHash = new string('a', 64),
            ContatoEmergenciaNome = "Contato",
            ContatoEmergenciaEmail = "contato@exemplo.com",
            ContatoEmergenciaWhatsapp = "11988888888",
            CriadoEm = DateTimeOffset.UtcNow,
            AtualizadoEm = DateTimeOffset.UtcNow
        };
        await usuarioRepo.UpsertAsync(usuario);
    }

    // ── SaveAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SqliteTokenRepository_SaveAsync_DevePersistirToken()
    {
        var usuarioId = Guid.NewGuid();
        await InserirUsuarioAsync(usuarioId);

        var repo = new SqliteTokenRepository(_factory);
        var token = CriarTokenValido(usuarioId);

        await repo.SaveAsync(token);

        var recuperado = await repo.GetByUsuarioIdAsync(usuarioId);

        recuperado.Should().NotBeNull();
        recuperado!.Id.Should().Be(token.Id);
        recuperado.UsuarioId.Should().Be(token.UsuarioId);
        recuperado.AccessToken.Should().Be(token.AccessToken);
        recuperado.RefreshToken.Should().Be(token.RefreshToken);
        recuperado.SenhaHash.Should().Be(token.SenhaHash);
    }

    [Fact]
    public async Task SqliteTokenRepository_SaveAsync_DeveSubstituirTokenExistente()
    {
        var usuarioId = Guid.NewGuid();
        await InserirUsuarioAsync(usuarioId);

        var repo = new SqliteTokenRepository(_factory);
        var tokenOriginal = CriarTokenValido(usuarioId);
        await repo.SaveAsync(tokenOriginal);

        // substitui com novo token para o mesmo usuário (mesmo Id)
        var tokenAtualizado = new TokenLocal
        {
            Id = tokenOriginal.Id,
            UsuarioId = usuarioId,
            AccessToken = "novo.access.token",
            RefreshToken = "novo.refresh.token",
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            SenhaHash = tokenOriginal.SenhaHash,
            CriadoEm = tokenOriginal.CriadoEm
        };
        await repo.SaveAsync(tokenAtualizado);

        var recuperado = await repo.GetByUsuarioIdAsync(usuarioId);

        recuperado.Should().NotBeNull();
        recuperado!.AccessToken.Should().Be("novo.access.token");
        recuperado.RefreshToken.Should().Be("novo.refresh.token");
    }

    // ── DeleteByUsuarioIdAsync ─────────────────────────────────────────────

    [Fact]
    public async Task SqliteTokenRepository_DeleteAsync_DeveRemoverToken()
    {
        var usuarioId = Guid.NewGuid();
        await InserirUsuarioAsync(usuarioId);

        var repo = new SqliteTokenRepository(_factory);
        var token = CriarTokenValido(usuarioId);
        await repo.SaveAsync(token);

        await repo.DeleteByUsuarioIdAsync(usuarioId);

        var recuperado = await repo.GetByUsuarioIdAsync(usuarioId);

        recuperado.Should().BeNull();
    }

    [Fact]
    public async Task SqliteTokenRepository_DeleteAsync_NaoDeveErrar_QuandoNaoExisteToken()
    {
        var repo = new SqliteTokenRepository(_factory);

        // deletar id inexistente não deve lançar exceção
        var act = async () => await repo.DeleteByUsuarioIdAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    // ── GetByUsuarioIdAsync ────────────────────────────────────────────────

    [Fact]
    public async Task SqliteTokenRepository_GetByUsuarioIdAsync_DeveRetornarNull_QuandoNaoExiste()
    {
        var repo = new SqliteTokenRepository(_factory);

        var resultado = await repo.GetByUsuarioIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }
}
