using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.RegistrarCheckIn;

public class RegistrarCheckInUseCase
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IUnitOfWork _uow;

    public RegistrarCheckInUseCase(ICheckInRepository checkInRepository, IUnitOfWork uow)
    {
        _checkInRepository = checkInRepository;
        _uow = uow;
    }

    /// <summary>
    /// Registra um check-in. Se o mesmo id_local já foi enviado antes, retorna false (idempotente).
    /// </summary>
    public async Task<bool> ExecutarAsync(RegistrarCheckInInput input, CancellationToken ct = default)
    {
        var checkIn = CheckIn.Criar(
            input.UsuarioId,
            input.IdLocal,
            input.DataHora,
            input.Latitude,
            input.Longitude,
            input.DeviceId);

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            var inserido = await _checkInRepository.AdicionarSeNaoExistirAsync(checkIn, ct);
            await _uow.CommitAsync(ct);
            return inserido;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
