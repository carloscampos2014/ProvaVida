using System.Net.Http.Json;
using System.Text.Json;

namespace ProvaVida.Mobile.Infrastructure.Http;

/// <summary>
/// Implementação de <see cref="IApiClient"/> usando <see cref="HttpClient"/> com <c>System.Text.Json</c>.
/// A base URL deve ser configurada em <see cref="HttpClient.BaseAddress"/> antes do registro no DI.
/// </summary>
public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ApiClient"/>.
    /// </summary>
    /// <param name="httpClient">Instância de <see cref="HttpClient"/> com <c>BaseAddress</c> configurada.</param>
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength == 0)
            return default;

        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
    }
}
