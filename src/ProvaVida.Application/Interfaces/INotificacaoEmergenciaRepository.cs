using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Interfaces;

public interface INotificacaoEmergenciaRepository
{
    Task AdicionarAsync(NotificacaoEmergencia notificacao, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);

    /// <summary>
    /// Verifica se já existe notificação disparada ou aguardando no ciclo atual (últimas 48h).
    /// Evita reenvio duplicado.
    /// </summary>
    Task<bool> ExisteNotificacaoAtivaNasUltimasHorasAsync(Guid usuarioId, int horas, CancellationToken ct = default);

    /// <summary>
    /// Retorna notificações com status=aguardando_resposta cuja janela já expirou.
    /// </summary>
    Task<IEnumerable<NotificacaoEmergencia>> ListarJanelasExpiradasAsync(CancellationToken ct = default);
}
