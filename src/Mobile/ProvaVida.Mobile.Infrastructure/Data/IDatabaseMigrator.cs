namespace ProvaVida.Mobile.Infrastructure.Data;

/// <summary>
/// Contrato para execução de migrations do banco de dados local SQLite.
/// </summary>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Executa as migrations pendentes no banco de dados SQLite.
    /// </summary>
    /// <returns><c>true</c> se as migrations foram aplicadas com sucesso; <c>false</c> caso contrário.</returns>
    bool Migrate();
}
