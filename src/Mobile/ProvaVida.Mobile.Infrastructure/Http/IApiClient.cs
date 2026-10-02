namespace ProvaVida.Mobile.Infrastructure.Http;

/// <summary>
/// Abstração do cliente HTTP utilizado para comunicação com a API ProvaVida.
/// A base URL é configurada externamente via <see cref="System.Net.Http.HttpClient.BaseAddress"/> no DI.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Envia uma requisição POST para o endpoint informado, serializando <paramref name="request"/> como JSON
    /// e desserializando a resposta para <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">Tipo do objeto de requisição.</typeparam>
    /// <typeparam name="TResponse">Tipo do objeto de resposta esperado.</typeparam>
    /// <param name="endpoint">Caminho relativo do endpoint (ex.: <c>auth/login</c>).</param>
    /// <param name="request">Objeto a ser enviado no corpo da requisição.</param>
    /// <param name="cancellationToken">Token de cancelamento opcional.</param>
    /// <returns>Objeto desserializado da resposta, ou <c>null</c> se o corpo estiver vazio.</returns>
    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default);
}
