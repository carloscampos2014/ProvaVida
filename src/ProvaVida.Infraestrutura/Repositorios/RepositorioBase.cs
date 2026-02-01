using Microsoft.EntityFrameworkCore;
using ProvaVida.Infraestrutura.Contexto;

namespace ProvaVida.Infraestrutura.Repositorios;

/// <summary>
/// Classe base genérica para repositórios.
/// Implementa operações CRUD comuns, permitindo trocar o BD sem modificar cada repositório concreto.
/// 
/// Princípio: Uma única implementação de persistência para todas as entidades.
/// </summary>
/// <typeparam name="T">Tipo da entidade gerenciada pelo repositório.</typeparam>
public abstract class RepositorioBase<T> where T : class
{
    protected readonly ProvaVidaDbContext Contexto;
    protected readonly DbSet<T> DbSet;

    protected RepositorioBase(ProvaVidaDbContext contexto)
    {
        Contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        DbSet = contexto.Set<T>();
    }

    /// <summary>
    /// Adiciona uma nova entidade ao banco de dados.
    /// </summary>
    /// <param name="entidade">Entidade a ser adicionada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task assíncrona.</returns>
    public virtual async Task AdicionarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        if (entidade == null)
            throw new ArgumentNullException(nameof(entidade));

        await DbSet.AddAsync(entidade, cancellationToken);
        await Contexto.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém uma entidade pelo ID.
    /// </summary>
    /// <param name="id">ID da entidade.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Entidade encontrada ou null.</returns>
    public virtual async Task<T?> ObterPorIdAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        return await DbSet.FindAsync(new[] { id }, cancellationToken);
    }

    /// <summary>
    /// Obtém todas as entidades.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de entidades.</returns>
    public virtual async Task<List<T>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza uma entidade existente.
    /// </summary>
    /// <param name="entidade">Entidade com dados atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task assíncrona.</returns>
    public virtual async Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        if (entidade == null)
            throw new ArgumentNullException(nameof(entidade));

        DbSet.Update(entidade);
        await Contexto.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Remove uma entidade do banco de dados.
    /// </summary>
    /// <param name="id">ID da entidade a remover.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se removido, false se não encontrado.</returns>
    public virtual async Task<bool> RemoverAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        var entidade = await ObterPorIdAsync(id, cancellationToken);
        if (entidade == null)
            return false;

        DbSet.Remove(entidade);
        await Contexto.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Remove uma entidade específica.
    /// </summary>
    /// <param name="entidade">Entidade a remover.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task assíncrona.</returns>
    public virtual async Task RemoverAsync(T entidade, CancellationToken cancellationToken = default)
    {
        if (entidade == null)
            throw new ArgumentNullException(nameof(entidade));

        DbSet.Remove(entidade);
        await Contexto.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se uma entidade existe.
    /// </summary>
    /// <param name="id">ID da entidade.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se existe, false caso contrário.</returns>
    public virtual async Task<bool> ExisteAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id == null)
            return false;

        var entidade = await ObterPorIdAsync(id, cancellationToken);
        return entidade != null;
    }

    /// <summary>
    /// Conta o total de entidades.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Número de entidades.</returns>
    public virtual async Task<int> ContarAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }
}
