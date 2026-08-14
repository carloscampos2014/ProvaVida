namespace ProvaVida.Maui.Services;

public record RegistrarCheckInRequest(
    Guid IdLocal,
    DateTime DataHora,
    double? Latitude,
    double? Longitude,
    string? DeviceId);

public record CheckInHistoricoItem(
    Guid Id,
    Guid IdLocal,
    DateTime DataHora,
    double? Latitude,
    double? Longitude,
    string DeviceId);

public interface ICheckInService
{
    Task<bool> RegistrarAsync(RegistrarCheckInRequest request, CancellationToken ct = default);
    Task<List<CheckInHistoricoItem>> ObterHistoricoAsync(
        DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default);
}
