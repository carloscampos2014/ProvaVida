using System.Data;

namespace ProvaVida.Shared.Repositories;

/// <summary>
/// Fábrica de conexões de banco de dados.
/// </summary>
/// <remarks>
/// Cada implementação concreta (<c>PostgresConnectionFactory</c>, <c>SqliteConnectionFactory</c>)
/// encapsula a connection string e o tipo de driver, mantendo o repositório desacoplado do banco.
/// </remarks>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Cria e retorna uma nova conexão de banco de dados.
    /// </summary>
    /// <remarks>
    /// O chamador é responsável por descartar a conexão após o uso (<c>await using</c>).
    /// </remarks>
    /// <returns>Uma <see cref="IDbConnection"/> pronta para uso.</returns>
    IDbConnection Create();
}
