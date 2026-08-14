using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Interfaces;

public interface ICheckInRepository
{
    /// <summary>
    /// Insere o check-in. Se já existir um com o mesmo id_local, ignora silenciosamente (idempotência).
    /// Retorna true se inserido, false se já existia.
    /// </summary>
    Task<bool> AdicionarSeNaoExistirAsync(CheckIn checkIn, CancellationToken ct = default);

    Task<IEnumerable<CheckIn>> ListarPorUsuarioAsync(
        Guid usuarioId, DateTime dataInicio, DateTime dataFim, CancellationToken ct = default);
}
