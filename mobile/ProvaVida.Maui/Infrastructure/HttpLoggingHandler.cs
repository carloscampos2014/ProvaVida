using Microsoft.Extensions.Logging;

namespace ProvaVida.Maui.Infrastructure;

/// <summary>
/// DelegatingHandler que loga todas as requisições e respostas HTTP.
/// Útil para diagnóstico de problemas de comunicação com a API.
/// </summary>
public sealed class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpLoggingHandler> _logger;

    public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("[HTTP] → {Method} {Uri} | Auth: {Auth}",
            request.Method,
            request.RequestUri,
            request.Headers.Authorization?.Scheme ?? "none");

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HTTP] ✗ {Method} {Uri} — Excecao: {Msg}",
                request.Method, request.RequestUri, ex.Message);
            throw;
        }

        _logger.LogWarning("[HTTP] ← {Status} {Uri}",
            (int)response.StatusCode,
            request.RequestUri);

        return response;
    }
}
