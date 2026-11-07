namespace ProvaVida.Shared.Dtos;

/// <summary>
/// Resposta de login contendo os dados do usuário autenticado e os tokens de acesso.
/// </summary>
/// <remarks>
/// Retornada pelo endpoint <c>POST /auth/login</c>, permitindo ao app mobile
/// persistir dados do usuário e tokens no SQLite em uma única chamada.
/// As propriedades <c>AccessToken</c>, <c>RefreshToken</c>, <c>TokenType</c> e
/// <c>ExpiresInSeconds</c> espelham <see cref="TokenResponse"/> para compatibilidade.
/// </remarks>
public record LoginResponseDto(
    Guid UsuarioId,
    string Nome,
    string Email,
    string Whatsapp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsapp,
    DateTimeOffset CriadoEm,
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresInSeconds
);
