using ProvaVida.Shared.Common;

namespace ProvaVida.Shared.Repositories;

/// <summary>
/// Interface genérica de repositório com operações CRUD assíncronas.
/// </summary>
/// <remarks>
/// Métodos de leitura retornam <see cref="Result{T}"/> — <c>Success=true</c> com <c>Data</c>
/// preenchido quando encontrado; <c>Success=false</c> com <c>MessageErro</c> quando não encontrado
/// ou em caso de erro de infraestrutura.
/// </remarks>
/// <typeparam name="T">Tipo da entidade gerenciada pelo repositório.</typeparam>
public interface IRepository<T>
{
    /// <summary>
    /// Busca uma entidade pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único da entidade.</param>
    /// <returns>
    /// <see cref="Result{T}"/> com <c>Success=true</c> e <c>Data</c> preenchido se encontrada;
    /// <c>Success=false</c> com <c>MessageErro</c> caso contrário.
    /// </returns>
    Task<Result<T>> GetByIdAsync(Guid id);

    /// <summary>
    /// Retorna todas as entidades da coleção.
    /// </summary>
    /// <returns>
    /// <see cref="Result{T}"/> com <c>Success=true</c> e <c>Data</c> contendo a coleção;
    /// <c>Success=false</c> com <c>MessageErro</c> em caso de erro.
    /// </returns>
    Task<Result<IEnumerable<T>>> GetAllAsync();

    /// <summary>
    /// Insere ou atualiza a entidade (upsert).
    /// </summary>
    /// <param name="entity">Entidade a ser inserida ou atualizada.</param>
    /// <returns>
    /// <see cref="Result"/> com <c>Success=true</c> se a operação foi bem-sucedida;
    /// <c>Success=false</c> com <c>MessageErro</c> em caso de erro.
    /// </returns>
    Task<Result> UpsertAsync(T entity);

    /// <summary>
    /// Remove a entidade identificada pelo <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Identificador único da entidade a ser removida.</param>
    /// <returns>
    /// <see cref="Result"/> com <c>Success=true</c> se removida com sucesso;
    /// <c>Success=false</c> com <c>MessageErro</c> em caso de erro.
    /// </returns>
    Task<Result> DeleteAsync(Guid id);
}
