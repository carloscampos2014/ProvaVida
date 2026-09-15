namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Dados para renovação ou revogação de sessão via refresh token.
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken
);
