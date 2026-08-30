using System.Reflection;
using DbUp;
using Microsoft.Extensions.Logging;

namespace ProvaVida.Api.Infrastructure.Data;

/// <summary>
/// Responsável por executar as migrations do banco de dados PostgreSQL no startup da API.
/// Utiliza DbUp para aplicar scripts SQL versionados de forma idempotente.
/// </summary>
public class DatabaseMigrator
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseMigrator> _logger;

    /// <summary>
    /// Inicializa o migrator com a connection string e o logger.
    /// </summary>
    /// <param name="connectionString">Connection string do banco PostgreSQL.</param>
    /// <param name="logger">Logger para registrar o resultado das migrations.</param>
    public DatabaseMigrator(string connectionString, ILogger<DatabaseMigrator> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Garante que o banco de dados existe e aplica todas as migrations pendentes.
    /// Lança exceção se qualquer migration falhar, impedindo o startup da aplicação.
    /// </summary>
    /// <exception cref="Exception">Lançada quando uma ou mais migrations falharem.</exception>
    public void Migrate()
    {
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                script => script.Contains("Migrations"))
            .WithTransaction()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            _logger.LogError(result.Error, "Falha ao executar migrations do banco de dados.");
            throw new Exception("Falha ao executar migrations. A aplicação não pode iniciar.", result.Error);
        }

        _logger.LogInformation("Migrations executadas com sucesso.");
    }
}
