namespace ProvaVida.Maui.Services;

public record CadastroRequest(
    string Nome,
    string Email,
    string WhatsApp,
    string Senha,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp);

public record LoginRequest(string Email, string Senha);

public record LoginResponse(
    string Token,
    DateTime ExpiraEm,
    string RefreshToken,
    DateTime RefreshTokenExpiraEm);

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(
    string Token,
    DateTime ExpiraEm,
    string RefreshToken,
    DateTime RefreshTokenExpiraEm);

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task CadastrarAsync(CadastroRequest request, CancellationToken ct = default);
    Task LogoffAsync(CancellationToken ct = default);

    /// <summary>
    /// Tenta renovar o access token usando o refresh token salvo.
    /// Retorna true se renovado com sucesso, false se o refresh token expirou ou é inválido.
    /// </summary>
    Task<bool> TentarRenovarTokenAsync(CancellationToken ct = default);
}
