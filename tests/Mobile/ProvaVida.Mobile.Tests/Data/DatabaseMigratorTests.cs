using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Mobile.Infrastructure.Data;

namespace ProvaVida.Mobile.Tests.Data;

/// <summary>
/// Testes unitários para <see cref="DatabaseMigrator"/>.
/// Usa SQLite em arquivo temporário para exercitar os caminhos reais do DbUp.
/// </summary>
public class DatabaseMigratorTests : IDisposable
{
    private readonly string _dbPath;
    private readonly Mock<ILogger<DatabaseMigrator>> _loggerMock;

    public DatabaseMigratorTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"provavida_migrator_test_{Guid.NewGuid()}.db");
        _loggerMock = new Mock<ILogger<DatabaseMigrator>>();
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    // ── Caminho feliz ─────────────────────────────────────────────────────

    [Fact]
    public void Migrate_DeveRetornarTrue_QuandoBancoNovoCriado()
    {
        var migrator = new DatabaseMigrator(_dbPath, _loggerMock.Object);

        var result = migrator.Migrate();

        result.Should().BeTrue();
        File.Exists(_dbPath).Should().BeTrue();
    }

    [Fact]
    public void Migrate_DeveRetornarTrue_QuandoChamadoDuasVezes_BancoJaAtualizado()
    {
        var migrator = new DatabaseMigrator(_dbPath, _loggerMock.Object);

        var primeiraExecucao = migrator.Migrate();
        var segundaExecucao = migrator.Migrate();

        primeiraExecucao.Should().BeTrue();
        segundaExecucao.Should().BeTrue("segundo Migrate() deve retornar true quando banco já está atualizado");
    }

    [Fact]
    public void Migrate_DeveCriarDiretorio_QuandoNaoExiste()
    {
        // Coloca o db dentro de um subdiretório que ainda não existe
        var subDir = Path.Combine(Path.GetTempPath(), $"pv_dir_{Guid.NewGuid()}");
        var dbPath = Path.Combine(subDir, "test.db");
        var migrator = new DatabaseMigrator(dbPath, _loggerMock.Object);

        try
        {
            var result = migrator.Migrate();

            result.Should().BeTrue();
            Directory.Exists(subDir).Should().BeTrue();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (Directory.Exists(subDir)) Directory.Delete(subDir);
        }
    }

    // ── Caminho de erro ───────────────────────────────────────────────────

    [Fact]
    public void Migrate_DeveRetornarFalse_QuandoCaminhoEInvalido()
    {
        // Caminho com caractere nulo é inválido em qualquer sistema operacional
        // e força a exceção no bloco catch do DatabaseMigrator
        var caminhoInvalido = Path.Combine(Path.GetTempPath(), "\0invalid.db");
        var migrator = new DatabaseMigrator(caminhoInvalido, _loggerMock.Object);

        var result = migrator.Migrate();

        result.Should().BeFalse();
    }
}
