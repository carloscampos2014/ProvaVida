using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Services;

public class HeartbeatService : IHeartbeatService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _tokenStorage;
    private readonly IAuthService _authService;

    // Renova o token quando restar menos de 7 horas para expirar
    private static readonly TimeSpan LimiarRenovacao = TimeSpan.FromHours(7);

    public HeartbeatService(HttpClient http, ITokenStorage tokenStorage, IAuthService authService)
    {
        _http = http;
        _tokenStorage = tokenStorage;
        _authService = authService;
    }

    public async Task EnviarAsync(CancellationToken ct = default)
    {
        // Renovação proativa: verifica se o token está prestes a expirar
        await RenovarTokenSeNecessarioAsync(ct);

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

    private async Task RenovarTokenSeNecessarioAsync(CancellationToken ct)
    {
        try
        {
            var expiraEm = await _tokenStorage.ObterExpiraEmAsync();
            if (!expiraEm.HasValue) return;

            var tempoRestante = expiraEm.Value - DateTime.UtcNow;
            if (tempoRestante > LimiarRenovacao) return;

            // Menos de 7 horas restantes — renova silenciosamente
            await _authService.TentarRenovarTokenAsync(ct);
        }
        catch
        {
            // Falha silenciosa — o token atual continua sendo usado se ainda válido
        }
    }
}
