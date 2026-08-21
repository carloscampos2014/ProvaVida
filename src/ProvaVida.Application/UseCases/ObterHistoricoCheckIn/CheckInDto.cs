namespace ProvaVida.Application.UseCases.ObterHistoricoCheckIn;

public record CheckInDto(
    Guid Id,
    Guid IdLocal,
    DateTimeOffset DataHora,
    double? Latitude,
    double? Longitude,
    string DeviceId
);
