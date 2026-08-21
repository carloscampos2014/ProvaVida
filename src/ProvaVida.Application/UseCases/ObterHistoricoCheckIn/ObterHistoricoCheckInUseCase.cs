using ProvaVida.Application.Interfaces;

namespace ProvaVida.Application.UseCases.ObterHistoricoCheckIn;

public class ObterHistoricoCheckInUseCase
{
    private readonly ICheckInRepository _checkInRepository;

    public ObterHistoricoCheckInUseCase(ICheckInRepository checkInRepository)
    {
        _checkInRepository = checkInRepository;
    }

    public async Task<IEnumerable<CheckInDto>> ExecutarAsync(
        ObterHistoricoCheckInInput input, CancellationToken ct = default)
    {
        var fim    = input.DataFim    ?? DateTimeOffset.UtcNow;
        var inicio = input.DataInicio ?? fim.AddDays(-7);

        var checkIns = await _checkInRepository.ListarPorUsuarioAsync(
            input.UsuarioId, inicio, fim, ct);

        return checkIns.Select(c => new CheckInDto(
            c.Id,
            c.IdLocal,
            c.DataHora,
            c.Latitude,
            c.Longitude,
            c.DeviceId));
    }
}
