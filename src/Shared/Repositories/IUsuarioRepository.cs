using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Shared.Repositories;

/// <summary>Repositório de usuários — contrato especializado.</summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>
    /// Busca um usuário pelo e-mail (case-insensitive).
    /// Retorna <see cref="Result{T}"/> com Success=false se não encontrado.
    /// </summary>
    Task<Result<Usuario>> GetByEmailAsync(string email);
}
