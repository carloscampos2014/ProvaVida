namespace ProvaVida.Dominio.Repositorios;

/// <summary>
/// Interface para o repositório de notificações (histórico de alertas).
/// Define o contrato que a camada de Infraestrutura deve implementar.
/// </summary>
public interface IRepositorioNotificacao
{
    /// <summary>
    /// Adiciona uma nova notificação ao repositório.
    /// O histórico será automaticamente mantido com no máximo 5 registros por contato de emergência.
    /// </summary>
    /// <param name="notificacao">A notificação a ser adicionada.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task AdicionarAsync(Entidades.Notificacao notificacao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma notificação pelo seu identificador único.
    /// </summary>
    /// <param name="id">O identificador da notificação.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>A notificação encontrada, ou null se não existir.</returns>
    Task<Entidades.Notificacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as notificações de um usuário, ordenadas por data descendente.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>Lista de notificações do usuário.</returns>
    Task<IEnumerable<Entidades.Notificacao>> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém notificações de um contato de emergência específico.
    /// </summary>
    /// <param name="contatoEmergenciaId">O identificador do contato.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>Lista de notificações do contato.</returns>
    Task<IEnumerable<Entidades.Notificacao>> ObterPorContatoIdAsync(Guid contatoEmergenciaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta quantas notificações de emergência um contato possui.
    /// </summary>
    /// <param name="contatoEmergenciaId">O identificador do contato.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>Número de notificações de emergência.</returns>
    Task<int> ContarNotificacoesEmergenciaAsync(Guid contatoEmergenciaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpa o histórico de notificações de um contato, mantendo apenas os N registros mais recentes.
    /// Esta operação é executada automaticamente quando uma nova notificação é adicionada.
    /// </summary>
    /// <param name="contatoEmergenciaId">O identificador do contato.</param>
    /// <param name="limiteRegistros">Número máximo de registros a manter.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task LimparHistoricoExcedentesAsync(Guid contatoEmergenciaId, int limiteRegistros = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o status de uma notificação (ex: Pendente → Enviada).
    /// </summary>
    /// <param name="notificacao">A notificação com status atualizado.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task AtualizarAsync(Entidades.Notificacao notificacao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma notificação do repositório.
    /// </summary>
    /// <param name="id">O identificador da notificação a ser removida.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
