using System.Data;
using Microsoft.Data.Sqlite;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Infrastructure.Data;

/// <summary>
/// Fábrica de conexões SQLite para uso no aplicativo Mobile.
/// </summary>
/// <remarks>
/// Encapsula o caminho do arquivo de banco de dados e o driver Microsoft.Data.Sqlite,
/// mantendo <see cref="DapperRepository{T}"/> desacoplado do banco de dados subjacente.
/// O caminho é fornecido via construtor para facilitar a testabilidade
/// (produção usa <c>FileSystem.AppDataDirectory</c>; testes usam <c>Path.GetTempPath()</c>).
/// </remarks>
public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _dbPath;

    /// <summary>
    /// Inicializa a fábrica com o caminho completo do arquivo SQLite.
    /// </summary>
    /// <param name="dbPath">Caminho completo para o arquivo <c>.db</c> do SQLite.</param>
    public SqliteConnectionFactory(string dbPath)
    {
        _dbPath = dbPath;
    }

    /// <inheritdoc/>
    public IDbConnection Create() => new SqliteConnection($"Data Source={_dbPath}");
}
