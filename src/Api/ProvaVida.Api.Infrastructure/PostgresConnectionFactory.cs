using System.Data;
using Npgsql;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Infrastructure;

/// <summary>
/// Fábrica de conexões PostgreSQL para uso na API e no Admin.
/// </summary>
/// <remarks>
/// Encapsula a connection string e o driver Npgsql, mantendo <see cref="DapperRepository{T}"/>
/// desacoplado do banco de dados subjacente.
/// </remarks>
public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Inicializa a fábrica com a connection string do PostgreSQL.
    /// </summary>
    /// <param name="connectionString">Connection string no formato Npgsql.</param>
    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc/>
    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}
