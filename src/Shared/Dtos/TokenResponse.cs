namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Resposta de autenticação contendo os tokens de acesso e renovação.
/// </summary>
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresInSeconds
);
