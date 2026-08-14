using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Interfaces;

public interface IHeartbeatRepository
{
    Task AdicionarAsync(Heartbeat heartbeat, CancellationToken ct = default);

    /// <summary>
    /// Verifica se houve heartbeat do usuário nas últimas N horas.
    /// Usado pelo job de verificação de inatividade (Fase 6).
    /// </summary>
    Task<bool> ExisteHeartbeatRecenteAsync(Guid usuarioId, int horas, CancellationToken ct = default);
}
