using DbUp;
using Microsoft.Extensions.Logging;

namespace ProvaVida.Mobile.Infrastructure.Data;

/// <summary>
/// Responsável por executar as migrations do banco de dados SQLite local via DbUp.
/// </summary>
public class DatabaseMigrator : IDatabaseMigrator
{
    private readonly string _dbPath;
    private readonly ILogger<DatabaseMigrator> _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DatabaseMigrator"/>.
    /// </summary>
    /// <param name="dbPath">Caminho completo para o arquivo do banco de dados SQLite.</param>
    /// <param name="logger">Logger para registrar informações sobre as migrations.</param>
    public DatabaseMigrator(string dbPath, ILogger<DatabaseMigrator> logger)
    {
        _dbPath = dbPath;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool Migrate()
    {
        try
        {
            // SQLite cria o arquivo automaticamente; garantimos apenas que o diretório exista
            var directory = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var connectionString = $"Data Source={_dbPath}";

            var upgrader = DeployChanges.To
                .SqliteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(
                    typeof(DatabaseMigrator).Assembly,
                    script => script.StartsWith("ProvaVida.Mobile.Infrastructure.Migrations."))
                .WithTransaction()
                .LogTo(_logger)
                .Build();

            if (!upgrader.IsUpgradeRequired())
            {
                _logger.LogInformation("Banco de dados SQLite já está atualizado.");
                return true;
            }

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                _logger.LogError(result.Error, "Falha ao executar migrations do SQLite.");
                return false;
            }

            _logger.LogInformation("Migrations do SQLite executadas com sucesso.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar migrations do SQLite.");
            return false;
        }
    }
}
