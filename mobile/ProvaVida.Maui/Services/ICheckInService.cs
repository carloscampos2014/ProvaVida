namespace ProvaVida.Maui.Services;

public record RegistrarCheckInRequest(
    Guid IdLocal,
    DateTimeOffset DataHora,
    double? Latitude,
    double? Longitude,
    string? DeviceId);

public record CheckInHistoricoItem(
    Guid Id,
    Guid IdLocal,
    DateTimeOffset DataHora,
    double? Latitude,
    double? Longitude,
    string DeviceId);

public interface ICheckInService
{
    Task<bool> RegistrarAsync(RegistrarCheckInRequest request, CancellationToken ct = default);
    Task<List<CheckInHistoricoItem>> ObterHistoricoAsync(
        DateTimeOffset? dataInicio = null, DateTimeOffset? dataFim = null, CancellationToken ct = default);
}
