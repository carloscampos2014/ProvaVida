using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Services;

public class HeartbeatService : IHeartbeatService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _tokenStorage;

    public HeartbeatService(HttpClient http, ITokenStorage tokenStorage)
    {
        _http = http;
        _tokenStorage = tokenStorage;
    }

    public async Task EnviarAsync(CancellationToken ct = default)
    {
        var token = await _tokenStorage.ObterAsync();
        if (string.IsNullOrEmpty(token)) return;

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            await _http.PostAsync("heartbeat", null, ct);
        }
        catch
        {
            // Heartbeat é melhor esforço — falha silenciosa
        }
    }
}
