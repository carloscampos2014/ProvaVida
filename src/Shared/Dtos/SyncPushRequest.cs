namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Requisição para sincronização de dados do dispositivo móvel para o servidor.
/// </summary>
/// <remarks>
/// Usado no endpoint POST /sync/push para enviar check-ins pendentes.
/// </remarks>
public record SyncPushRequest(
    Guid UsuarioId,
    DateOnly Data,
    double Latitude,
    double Longitude,
    string IdentificacaoAparelho
);