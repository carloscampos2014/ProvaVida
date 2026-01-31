namespace ProvaVida.Dominio.Repositorios;

/// <summary>
/// Interface para o repositório de contatos de emergência.
/// Define o contrato que a camada de Infraestrutura deve implementar.
/// </summary>
public interface IRepositorioContatoEmergencia
{
    /// <summary>
    /// Adiciona um novo contato de emergência ao repositório.
    /// </summary>
    /// <param name="contato">O contato a ser adicionado.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task AdicionarAsync(Entidades.ContatoEmergencia contato, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um contato de emergência pelo seu identificador único.
    /// </summary>
    /// <param name="id">O identificador do contato.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>O contato encontrado, ou null se não existir.</returns>
    Task<Entidades.ContatoEmergencia?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os contatos de emergência de um usuário específico.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>Lista de contatos do usuário, ordenados por prioridade.</returns>
    Task<IEnumerable<Entidades.ContatoEmergencia>> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um contato de emergência existente no repositório.
    /// </summary>
    /// <param name="contato">O contato com os dados atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task AtualizarAsync(Entidades.ContatoEmergencia contato, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um contato de emergência do repositório.
    /// </summary>
    /// <param name="id">O identificador do contato a ser removido.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
