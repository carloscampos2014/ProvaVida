namespace ProvaVida.Shared.Dtos;

/// <summary>Payload do endpoint POST /checkins.</summary>
public record CheckinRequest(
    DateOnly Data,
    double Latitude,
    double Longitude,
    string IdentificacaoAparelho
);
