using ProvaVida.Shared.Entities;

namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Resposta da sincronização de dados do servidor para o dispositivo móvel.
/// </summary>
/// <remarks>
/// Contém os dados de check-in que devem ser transferidos para o dispositivo.
/// </remarks>
public record SyncPullResponse(
    Guid UsuarioId,
    DateOnly Data,
    double Latitude,
    double Longitude,
    string IdentificacaoAparelho
);