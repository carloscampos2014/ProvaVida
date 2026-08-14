namespace ProvaVida.Maui.Storage;

/// <summary>
/// Armazena o token JWT usando SecureStorage (keychain no iOS, keystore no Android).
/// Nunca em texto plano.
/// </summary>
public class TokenStorage : ITokenStorage
{
    private const string Key = "auth_token";

    public async Task SalvarAsync(string token)
        => await SecureStorage.Default.SetAsync(Key, token);

    public async Task<string?> ObterAsync()
        => await SecureStorage.Default.GetAsync(Key);

    public Task RemoverAsync()
    {
        SecureStorage.Default.Remove(Key);
        return Task.CompletedTask;
    }

    public async Task<bool> ExisteAsync()
    {
        var token = await ObterAsync();
        return !string.IsNullOrEmpty(token);
    }
}
