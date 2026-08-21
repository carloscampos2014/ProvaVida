namespace ProvaVida.Application.UseCases.RegistrarCheckIn;

public record RegistrarCheckInInput(
    Guid UsuarioId,
    Guid IdLocal,
    DateTimeOffset DataHora,
    double? Latitude,
    double? Longitude,
    string DeviceId
);
