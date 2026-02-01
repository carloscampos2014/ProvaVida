using ProvaVida.Aplicacao.Dtos.CheckIns;

namespace ProvaVida.Aplicacao.Servicos;

/// <summary>
/// Interface para o serviço de check-in.
/// Responsável por registrar provas de vida (48h) e gerenciar histórico.
/// </summary>
public interface ICheckInService
{
    /// <summary>
    /// Registra um novo check-in para o usuário.
    /// Reseta o prazo de 48 horas e limpa alertas pendentes.
    /// </summary>
    /// <param name="dto">ID do usuário e localização opcional.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Confirmação do check-in com próximo prazo.</returns>
    /// <exception cref="UsuarioNaoEncontradoException">Se usuário não existe.</exception>
    /// <exception cref="UsuarioInativoException">Se usuário está inativo.</exception>
    Task<CheckInResumoDto> RegistrarCheckInAsync(
        CheckInRegistroDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os últimos 5 check-ins do usuário (histórico FIFO).
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de últimos 5 check-ins.</returns>
    Task<List<CheckInResumoDto>> ObterHistoricoAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
