using ProvaVida.Shared.Entities;

namespace ProvaVida.Shared.Dtos;

/// <summary>Resposta do endpoint POST /checkins com dados do check-in criado.</summary>
public record CheckinResponse(
    Guid Id,
    Guid UsuarioId,
    DateOnly Data,
    double Latitude,
    double Longitude,
    string IdentificacaoAparelho,
    bool Sincronizado,
    DateTimeOffset CriadoEm
);

/// <summary>Extensões de mapeamento para <see cref="Checkin"/>.</summary>
public static class CheckinExtensions
{
    /// <summary>Converte uma entidade <see cref="Checkin"/> para <see cref="CheckinResponse"/>.</summary>
    public static CheckinResponse ToDto(this Checkin checkin) => new(
        checkin.Id,
        checkin.UsuarioId,
        checkin.Data,
        checkin.Latitude,
        checkin.Longitude,
        checkin.IdentificacaoAparelho,
        checkin.Sincronizado,
        checkin.CriadoEm
    );
}
