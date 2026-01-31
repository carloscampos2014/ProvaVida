namespace ProvaVida.Dominio.Repositorios;

/// <summary>
/// Interface para o repositório de check-ins (histórico de provas de vida).
/// Define o contrato que a camada de Infraestrutura deve implementar.
/// </summary>
public interface IRepositorioCheckIn
{
    /// <summary>
    /// Adiciona um novo check-in ao repositório.
    /// O histórico será automaticamente mantido com no máximo 5 registros por usuário.
    /// </summary>
    /// <param name="checkIn">O check-in a ser adicionado.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task AdicionarAsync(Entidades.CheckIn checkIn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o check-in mais recente de um usuário.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>O check-in mais recente, ou null se nenhum foi registrado.</returns>
    Task<Entidades.CheckIn?> ObterMaisRecentePorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os últimos check-ins de um usuário (máximo 5).
    /// </summary>
    /// <param name="usuarioId">O identificador do usuário.</param>
    /// <param name="limite">Número máximo de registros a retornar (padrão: 5).</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    /// <returns>Lista ordenada descendentemente por data dos últimos check-ins.</returns>
    Task<IEnumerable<Entidades.CheckIn>> ObterHistoricoAsync(Guid usuarioId, int limite = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpa o histórico de check-ins, mantendo apenas os N registros mais recentes.
    /// Esta operação é executada automaticamente quando um novo check-in é adicionado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuário.</param>
    /// <param name="limiteRegistros">Número máximo de registros a manter.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task LimparHistoricoExcedentesAsync(Guid usuarioId, int limiteRegistros = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um check-in específico do repositório.
    /// </summary>
    /// <param name="id">O identificador do check-in a ser removido.</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas.</param>
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
