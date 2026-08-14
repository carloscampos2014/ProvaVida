using Dapper;
using Npgsql;

namespace ProvaVida.IntegrationTests.Infrastructure;

/// <summary>
/// Limpa as tabelas de teste antes de cada cenário.
/// Garante isolamento sem precisar recriar o banco.
/// </summary>
public class DatabaseCleaner
{
    private readonly string _connectionString;

    public DatabaseCleaner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task LimparAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Ordem respeitando FK (filhos antes dos pais)
        await conn.ExecuteAsync("""
            TRUNCATE TABLE notificacoes_emergencia,
                           heartbeats,
                           checkins,
                           sessoes_login,
                           usuarios
            RESTART IDENTITY CASCADE
            """);
    }
}
