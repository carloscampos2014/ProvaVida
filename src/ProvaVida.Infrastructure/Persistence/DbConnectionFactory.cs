using Npgsql;
using System.Data;

namespace ProvaVida.Infrastructure.Persistence;

/// <summary>
/// Cria conexões Npgsql para uso com Dapper.
/// Registrado como Singleton no DI — a string de conexão é imutável por request.
/// </summary>
public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string não pode ser vazia.", nameof(connectionString));

        _connectionString = connectionString;
    }

    /// <summary>
    /// Abre e retorna uma nova conexão. O chamador é responsável por fazer Dispose.
    /// Use sempre em um bloco using.
    /// </summary>
    public IDbConnection CreateConnection()
    {
        var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        return conn;
    }
}
