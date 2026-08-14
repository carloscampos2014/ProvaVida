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
        RegistrarCheckInRequest request, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("checkin", request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<CheckInHistoricoItem>> ObterHistoricoAsync(
        DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var query = string.Empty;
        if (dataInicio.HasValue) query += $"?dataInicio={dataInicio:O}";
        if (dataFim.HasValue) query += (query.Contains('?') ? "&" : "?") + $"dataFim={dataFim:O}";

        var result = await _http.GetFromJsonAsync<List<CheckInHistoricoItem>>(
            $"checkin/historico{query}", ct);
        return result ?? [];
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenStorage.ObterAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
