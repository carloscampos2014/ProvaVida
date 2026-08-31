using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Admin.Application.Queries;

/// <summary>
/// Serviço de consulta de usuários para o painel Admin.
/// </summary>
/// <remarks>
/// Expõe apenas operações de leitura, impedindo que o Admin acesse
/// mutações (<c>Upsert</c>, <c>Delete</c>) diretamente via repositório.
/// </remarks>
public interface IAdminUsuarioQueryService
{
    /// <summary>
    /// Retorna todos os usuários cadastrados, ordenados por data de criação decrescente.
    /// </summary>
    /// <returns>
    /// <see cref="Result{T}"/> com a lista de usuários em caso de sucesso,
    /// ou com <see cref="Result{T}.MessageErro"/> preenchido em caso de falha.
    /// </returns>
    Task<Result<IEnumerable<Usuario>>> GetAllAsync();

    /// <summary>
    /// Retorna um usuário pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único do usuário.</param>
    /// <returns>
    /// <see cref="Result{T}"/> com o usuário encontrado, ou falha se não existir.
    /// </returns>
    Task<Result<Usuario>> GetByIdAsync(Guid id);
}
