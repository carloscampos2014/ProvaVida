namespace ProvaVida.Dominio.Repositorios;

/// <summary>
/// Interface para o repositório de usuários.
/// Define o contrato que a camada de Infraestrutura deve implementar.
/// </summary>
public interface IRepositorioUsuario
{
    /// <summary>
    /// Adiciona um novo usuário ao repositório.
    /// </summary>
    /// <param name="usuario">O usuário a ser adicionado.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task AdicionarAsync(Entidades.Usuario usuario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um usuário pelo seu identificador único.
    /// </summary>
    /// <param name="id">O identificador do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>O usuário encontrado, ou null se não existir.</returns>
    Task<Entidades.Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um usuário pelo seu email.
    /// </summary>
    /// <param name="email">O email do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>O usuário encontrado, ou null se não existir.</returns>
    Task<Entidades.Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um usuário existente no repositório.
    /// </summary>
    /// <param name="usuario">O usuário com os dados atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task AtualizarAsync(Entidades.Usuario usuario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um usuário do repositório.
    /// </summary>
    /// <param name="id">O identificador do usuário a ser removido.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
