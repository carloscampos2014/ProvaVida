namespace ProvaVida.Api.Application.Commands;

/// <summary>
/// Comando para encerramento da sessão do usuário.
/// </summary>
public record LogoutCommand(string RefreshToken);
