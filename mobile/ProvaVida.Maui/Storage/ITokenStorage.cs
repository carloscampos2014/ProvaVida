namespace ProvaVida.Maui.Storage;

public interface ITokenStorage
{
    // Access token
    Task SalvarAsync(string token);
    Task<string?> ObterAsync();
    Task RemoverAsync();
    Task<bool> ExisteAsync();

    // Refresh token
    Task SalvarRefreshTokenAsync(string refreshToken);
    Task<string?> ObterRefreshTokenAsync();

    // Expiração do access token
    Task SalvarExpiraEmAsync(DateTime expiraEm);
    Task<DateTime?> ObterExpiraEmAsync();

    // Remove tudo (logoff)
    Task LimparTudoAsync();
}
