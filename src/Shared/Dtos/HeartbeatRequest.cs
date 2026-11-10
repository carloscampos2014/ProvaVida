namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Requisição do endpoint de heartbeat para confirmar que o usuário está ativo.
/// </summary>
public record HeartbeatRequest(
    Guid UsuarioId,
    DateTime RecebidoEm
);