using System.Net.Http.Json;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Services;

public class ContaService : IContaService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _tokenStorage;

    public ContaService(HttpClient http, ITokenStorage tokenStorage)
    {
        _http = http;
        _tokenStorage = tokenStorage;
    }

    public async Task<ContaResponse?> ObterPerfilAsync(CancellationToken ct = default)
    {
        using var request = await CriarRequestAsync(HttpMethod.Get, "conta");
        try
        {
            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ContaResponse>(ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task AlterarAsync(AlterarContaRequest body, CancellationToken ct = default)
    {
        using var request = await CriarRequestAsync(HttpMethod.Put, "conta");
        request.Content = JsonContent.Create(body);
        var response = await _http.SendAsync(request, ct);
        await EnsureSuccessAsync(response);
    }

    public async Task AlterarSenhaAsync(AlterarSenhaRequest body, CancellationToken ct = default)
    {
        using var request = await CriarRequestAsync(HttpMethod.Put, "conta/senha");
        request.Content = JsonContent.Create(body);
        var response = await _http.SendAsync(request, ct);
        await EnsureSuccessAsync(response);
    }

    public async Task ExcluirAsync(ExcluirContaRequest body, CancellationToken ct = default)
    {
        using var request = await CriarRequestAsync(HttpMethod.Delete, "conta");
        request.Content = JsonContent.Create(body);
        var response = await _http.SendAsync(request, ct);
        await EnsureSuccessAsync(response);
    }

    /// <summary>
    /// Cria um HttpRequestMessage com o header Authorization já embutido.
    /// Cada request carrega seu próprio header — sem tocar em DefaultRequestHeaders.
    /// </summary>
    private async Task<HttpRequestMessage> CriarRequestAsync(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        var token = await _tokenStorage.ObterAsync();
        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        var mensagem = response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Sessão expirada. Faça login novamente.",
            System.Net.HttpStatusCode.NotFound => "Usuário não encontrado.",
            _ => "Erro inesperado. Tente novamente."
        };

        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err))
                mensagem = err.GetString() ?? mensagem;
        }
        catch { /* ignora JSON inválido */ }

        throw new ApiException(mensagem, (int)response.StatusCode);
    }
}
