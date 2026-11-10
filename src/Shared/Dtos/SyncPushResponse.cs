namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Resposta da sincronização de dados do dispositivo móvel para o servidor.
/// </summary>
/// <remarks>
/// Indica se o push foi bem-sucedido e qual a data sincronizada.
/// </remarks>
public record SyncPushResponse(
    bool Sucesso,
    DateOnly DataSincronizada
);