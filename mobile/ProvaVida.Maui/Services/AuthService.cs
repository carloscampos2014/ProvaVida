using System.Net.Http.Json;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _tokenStorage;

    // Garante que apenas uma renovação de token ocorre por vez.
    // Chamadas simultâneas aguardam a primeira terminar e reutilizam o token já renovado.
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _renovacaoEmAndamento = false;

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
        if (!string.IsNullOrEmpty(token))
        {
            // Header por request — não polui o estado global do HttpClient
            using var request = new HttpRequestMessage(HttpMethod.Post, "auth/logoff");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try { await _http.SendAsync(request, ct); }
            catch { /* ignora falha de rede no logoff */ }
        }

        await _tokenStorage.LimparTudoAsync();
    }

    public async Task<bool> TentarRenovarTokenAsync(CancellationToken ct = default)
    {
        // Se uma renovação já está em andamento, aguarda e retorna — o token já foi renovado
        if (_renovacaoEmAndamento)
        {
            await _refreshLock.WaitAsync(ct);
            _refreshLock.Release();
            return true;
        }

        await _refreshLock.WaitAsync(ct);
        try
        {
            // Verifica novamente após adquirir o lock — outra thread pode ter renovado antes
            if (_renovacaoEmAndamento) return true;
            _renovacaoEmAndamento = true;

            var refreshToken = await _tokenStorage.ObterRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var response = await _http.PostAsJsonAsync(
                "auth/refresh",
                new RefreshTokenRequest(refreshToken),
                ct);

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(ct);
            if (result is null) return false;

            await _tokenStorage.SalvarAsync(result.Token);
            await _tokenStorage.SalvarExpiraEmAsync(result.ExpiraEm);
            await _tokenStorage.SalvarRefreshTokenAsync(result.RefreshToken);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _renovacaoEmAndamento = false;
            _refreshLock.Release();
        }
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
