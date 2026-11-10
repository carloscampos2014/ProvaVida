namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Resposta do endpoint de heartbeat após confirmar que o usuário está ativo.
/// </summary>
public record HeartbeatResponse(
    bool Sucesso,
    DateTime RecebidoEm
);