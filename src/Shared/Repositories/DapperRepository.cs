using System.Data;
using Dapper;
using ProvaVida.Shared.Common;

namespace ProvaVida.Shared.Repositories;

/// <summary>
/// Classe base abstrata para repositórios que utilizam Dapper como acesso a dados.
/// </summary>
/// <remarks>
/// As subclasses devem fornecer as SQLs específicas de cada entidade via propriedades abstratas.
/// A conexão é injetada via construtor e não é gerenciada pelo repositório (sem <c>using</c>).
/// </remarks>
/// <typeparam name="T">Tipo da entidade gerenciada pelo repositório.</typeparam>
public abstract class DapperRepository<T> : IRepository<T>
{
    /// <summary>Conexão de banco de dados injetada via DI.</summary>
    protected readonly IDbConnection Connection;

    /// <summary>SQL para buscar uma entidade por <c>@Id</c>.</summary>
    protected abstract string SelectByIdSql { get; }

    /// <summary>SQL para buscar todas as entidades.</summary>
    protected abstract string SelectAllSql { get; }

    /// <summary>SQL de upsert (insert or update) da entidade.</summary>
    protected abstract string UpsertSql { get; }

    /// <summary>SQL para deletar a entidade por <c>@Id</c>.</summary>
    protected abstract string DeleteSql { get; }

    /// <summary>
    /// Inicializa o repositório com a conexão fornecida.
    /// </summary>
    /// <param name="connection">Conexão de banco de dados.</param>
    protected DapperRepository(IDbConnection connection)
    {
        Connection = connection;
    }

    /// <inheritdoc/>
    public async Task<Result<T>> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await Connection.QueryFirstOrDefaultAsync<T>(SelectByIdSql, new { Id = id });
            return entity is null
                ? Result<T>.Fail("Entidade não encontrada")
                : Result<T>.Ok(entity);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var items = await Connection.QueryAsync<T>(SelectAllSql);
            return Result<IEnumerable<T>>.Ok(items);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Fail(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> UpsertAsync(T entity)
    {
        try
        {
            await Connection.ExecuteAsync(UpsertSql, entity);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            await Connection.ExecuteAsync(DeleteSql, new { Id = id });
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
