namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Requisição para sincronização de dados do servidor para o dispositivo móvel.
/// </summary>
/// <remarks>
/// Usado no endpoint GET /sync/pull para buscar check-ins não sincronizados.
/// </remarks>
public record SyncPullRequest(
    Guid UsuarioId,
    DateTime OffsetData
);