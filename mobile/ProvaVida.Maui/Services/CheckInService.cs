using System.Net.Http.Json;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Services;

public class CheckInService : ICheckInService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _tokenStorage;

    public CheckInService(HttpClient http, ITokenStorage tokenStorage)
    {
        _http = http;
        _tokenStorage = tokenStorage;
    }

    public async Task<bool> RegistrarAsync(
        RegistrarCheckInRequest body, CancellationToken ct = default)
    {
        using var request = await CriarRequestAsync(HttpMethod.Post, "checkin");
        request.Content = JsonContent.Create(body);
        var response = await _http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<CheckInHistoricoItem>> ObterHistoricoAsync(
        DateTimeOffset? dataInicio = null, DateTimeOffset? dataFim = null, CancellationToken ct = default)
    {
        var query = string.Empty;
        if (dataInicio.HasValue) query += $"?dataInicio={Uri.EscapeDataString(dataInicio.Value.ToString("O"))}";
        if (dataFim.HasValue) query += (query.Contains('?') ? "&" : "?") + $"dataFim={Uri.EscapeDataString(dataFim.Value.ToString("O"))}";

        using var request = await CriarRequestAsync(HttpMethod.Get, $"checkin/historico{query}");
        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return [];

        var result = await response.Content.ReadFromJsonAsync<List<CheckInHistoricoItem>>(ct);
        return result ?? [];
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
}
