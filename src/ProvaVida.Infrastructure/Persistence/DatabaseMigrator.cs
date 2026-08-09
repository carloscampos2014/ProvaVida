using DbUp;

namespace ProvaVida.Infrastructure.Persistence;

/// <summary>
/// Aplica migrations SQL versionadas usando DbUp.
/// Scripts ficam em Persistence/Migrations/ embarcados no assembly como EmbeddedResource.
/// Ordem de execução: alfabética pelo nome do arquivo (V001_, V002_, ...).
/// </summary>
public sealed class DatabaseMigrator
{
    private readonly string _connectionString;

    public DatabaseMigrator(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string não pode ser vazia.", nameof(connectionString));

        _connectionString = connectionString;
    }

    /// <summary>
    /// Executa todas as migrations pendentes.
    /// Idempotente — scripts já aplicados são ignorados (tabela schemaversions).
    /// </summary>
    /// <exception cref="MigrationException">Lançada se algum script falhar.</exception>
    public void MigrateUp()
    {
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
            throw new MigrationException(
                $"Falha ao aplicar migration '{result.ErrorScript?.Name}': {result.Error?.Message}",
                result.Error);
    }

    public bool HasPendingMigrations()
    {
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .Build();

        return upgrader.IsUpgradeRequired();
    }
}
