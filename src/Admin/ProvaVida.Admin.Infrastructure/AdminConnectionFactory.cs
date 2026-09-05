using System.Data;
using Npgsql;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Admin.Infrastructure;

/// <summary>
/// Fábrica de conexões PostgreSQL para uso no Admin.
/// </summary>
/// <remarks>
/// Encapsula a connection string e o driver Npgsql, mantendo <see cref="ProvaVida.Shared.Repositories.DapperRepository{T}"/>
/// desacoplado do banco de dados subjacente.
/// </remarks>
public class AdminConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Inicializa a fábrica com a connection string do PostgreSQL.
    /// </summary>
    /// <param name="connectionString">Connection string no formato Npgsql.</param>
    public AdminConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc/>
    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}
