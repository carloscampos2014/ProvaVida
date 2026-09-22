namespace ProvaVida.Api.Application.Commands;

/// <summary>
/// Comando para renovação de tokens de autenticação.
/// </summary>
public record RefreshTokenCommand(string RefreshToken);
