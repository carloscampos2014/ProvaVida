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

    public async Task AlterarAsync(AlterarContaRequest request, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync("conta", request, ct);
        await EnsureSuccessAsync(response);
    }

    public async Task ExcluirAsync(ExcluirContaRequest request, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "conta")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _http.SendAsync(httpRequest, ct);
        await EnsureSuccessAsync(response);
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenStorage.ObterAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        catch { /* ignora */ }

        throw new ApiException(mensagem, (int)response.StatusCode);
    }
}
