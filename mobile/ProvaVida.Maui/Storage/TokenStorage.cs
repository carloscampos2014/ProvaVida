namespace ProvaVida.Maui.Storage;

/// <summary>
/// Armazena tokens JWT e refresh token usando SecureStorage
/// (keychain no iOS, keystore no Android). Nunca em texto plano.
/// </summary>
public class TokenStorage : ITokenStorage
{
    private const string KeyToken       = "auth_token";
    private const string KeyRefresh     = "auth_refresh_token";
    private const string KeyExpiraEm    = "auth_token_expira_em";

    // ----- Access token -----

    public async Task SalvarAsync(string token)
        => await SecureStorage.Default.SetAsync(KeyToken, token);

    public async Task<string?> ObterAsync()
        => await SecureStorage.Default.GetAsync(KeyToken);

    public Task RemoverAsync()
    {
        SecureStorage.Default.Remove(KeyToken);
        return Task.CompletedTask;
    }

    public async Task<bool> ExisteAsync()
    {
        var token = await ObterAsync();
        return !string.IsNullOrEmpty(token);
    }

    // ----- Refresh token -----

    public async Task SalvarRefreshTokenAsync(string refreshToken)
        => await SecureStorage.Default.SetAsync(KeyRefresh, refreshToken);

    public async Task<string?> ObterRefreshTokenAsync()
        => await SecureStorage.Default.GetAsync(KeyRefresh);

    // ----- Expiração do access token -----

    public async Task SalvarExpiraEmAsync(DateTime expiraEm)
        => await SecureStorage.Default.SetAsync(KeyExpiraEm, expiraEm.ToString("O"));

    public async Task<DateTime?> ObterExpiraEmAsync()
    {
        var valor = await SecureStorage.Default.GetAsync(KeyExpiraEm);
        if (string.IsNullOrEmpty(valor)) return null;
        if (DateTime.TryParse(valor, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            return dt;
        return null;
    }

    // ----- Limpar tudo (logoff) -----

    public Task LimparTudoAsync()
    {
        SecureStorage.Default.Remove(KeyToken);
        SecureStorage.Default.Remove(KeyRefresh);
        SecureStorage.Default.Remove(KeyExpiraEm);
        return Task.CompletedTask;
    }
}
