using System.Data;
using Dapper;
using ProvaVida.Shared.Common;

namespace ProvaVida.Shared.Repositories;

/// <summary>
/// Classe base abstrata para repositórios que utilizam Dapper como acesso a dados.
/// </summary>
/// <remarks>
/// A conexão é criada e descartada por operação via <see cref="IDbConnectionFactory"/>,
/// evitando conexões de longa duração. As subclasses fornecem as SQLs específicas
/// de cada entidade via propriedades abstratas.
/// </remarks>
/// <typeparam name="T">Tipo da entidade gerenciada pelo repositório.</typeparam>
public abstract class DapperRepository<T> : IRepository<T>
{
    private readonly IDbConnectionFactory _factory;

    /// <summary>SQL para buscar uma entidade por <c>@Id</c>.</summary>
    protected abstract string SelectByIdSql { get; }

    /// <summary>SQL para buscar todas as entidades.</summary>
    protected abstract string SelectAllSql { get; }

    /// <summary>SQL de upsert (insert or update) da entidade.</summary>
    protected abstract string UpsertSql { get; }

    /// <summary>SQL para deletar a entidade por <c>@Id</c>.</summary>
    protected abstract string DeleteSql { get; }

    /// <summary>
    /// Inicializa o repositório com a fábrica de conexões fornecida.
    /// </summary>
    /// <param name="factory">Fábrica responsável por criar conexões de banco de dados.</param>
    protected DapperRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<Result<T>> GetByIdAsync(Guid id)
    {
        try
        {
            using var conn = _factory.Create();
            var entity = await conn.QueryFirstOrDefaultAsync<T>(SelectByIdSql, new { Id = id });
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
            using var conn = _factory.Create();
            var items = await conn.QueryAsync<T>(SelectAllSql);
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
            using var conn = _factory.Create();
            await conn.ExecuteAsync(UpsertSql, entity);
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
            using var conn = _factory.Create();
            await conn.ExecuteAsync(DeleteSql, new { Id = id });
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
