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
        Guid usuarioId, DateTimeOffset dataInicio, DateTimeOffset dataFim, CancellationToken ct = default);

    /// <summary>
    /// Retorna IDs dos usuários cujo último check-in foi antes de dataCorte.
    /// Usado pelo job de verificação de inatividade.
    /// </summary>
    Task<IEnumerable<Guid>> ListarUsuariosInativosDesdeAsync(DateTimeOffset dataCorte, CancellationToken ct = default);

    /// <summary>
    /// Retorna true se o usuário fez check-in nas últimas <paramref name="horas"/> horas.
    /// Usado pelo job disparar-alerta para cancelar o ciclo apenas se houve check-in real.
    /// </summary>
    Task<bool> ExisteCheckInRecenteAsync(Guid usuarioId, int horas, CancellationToken ct = default);
}
