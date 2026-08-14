using System.Net.Http.Json;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _tokenStorage;

    public AuthService(HttpClient http, ITokenStorage tokenStorage)
    {
        _http = http;
        _tokenStorage = tokenStorage;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("auth/login", request, ct);
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(ct)
            ?? throw new ApiException("Resposta inválida do servidor.", 500);

        return result;
    }

    public async Task CadastrarAsync(CadastroRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("auth/cadastro", request, ct);
        await EnsureSuccessAsync(response);
    }

    public async Task LogoffAsync(CancellationToken ct = default)
    {
        var token = await _tokenStorage.ObterAsync();
        if (string.IsNullOrEmpty(token)) return;

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Melhor esforço — se falhar, limpa localmente mesmo assim
        try { await _http.PostAsync("auth/logoff", null, ct); }
        catch { /* ignora falha de rede no logoff */ }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        var mensagem = TentarExtrairMensagem(body, (int)response.StatusCode);
        throw new ApiException(mensagem, (int)response.StatusCode);
    }

    private static string TentarExtrairMensagem(string body, int statusCode)
    {
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString() ?? MensagemPadrao(statusCode);
            if (doc.RootElement.TryGetProperty("errors", out var errs))
                return string.Join("; ", errs.EnumerateArray()
                    .Select(e => e.TryGetProperty("message", out var m) ? m.GetString() : null)
                    .Where(m => m != null));
        }
        catch { /* ignora JSON inválido */ }

        return MensagemPadrao(statusCode);
    }

    private static string MensagemPadrao(int statusCode) => statusCode switch
    {
        400 => "Dados inválidos. Verifique os campos.",
        401 => "E-mail ou senha incorretos.",
        409 => "E-mail já cadastrado.",
        _ => "Erro inesperado. Tente novamente."
    };
}
