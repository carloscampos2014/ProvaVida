using ProvaVida.Admin.Application.Queries;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Admin.Infrastructure.Queries;

/// <summary>
/// Implementação do serviço de consulta de usuários para o painel Admin.
/// </summary>
/// <remarks>
/// Delega para <see cref="IUsuarioRepository"/> e expõe apenas leitura,
/// sem permitir acesso a <c>UpsertAsync</c> ou <c>DeleteAsync</c> pelo painel.
/// </remarks>
public class AdminUsuarioQueryService : IAdminUsuarioQueryService
{
    private readonly IUsuarioRepository _repository;

    /// <summary>
    /// Inicializa o serviço com o repositório de usuários.
    /// </summary>
    /// <param name="repository">Repositório de usuários PostgreSQL.</param>
    public AdminUsuarioQueryService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public Task<Result<IEnumerable<Usuario>>> GetAllAsync() =>
        _repository.GetAllAsync();

    /// <inheritdoc/>
    public Task<Result<Usuario>> GetByIdAsync(Guid id) =>
        _repository.GetByIdAsync(id);
}
