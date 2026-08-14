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
public record LoginResponse(string Token, DateTime ExpiraEm);

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task CadastrarAsync(CadastroRequest request, CancellationToken ct = default);
    Task LogoffAsync(CancellationToken ct = default);
}
