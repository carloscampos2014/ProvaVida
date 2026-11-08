using ProvaVida.Shared.Common;

namespace ProvaVida.Mobile.Application.Services;

/// <summary>Serviço para obter a localização geográfica atual do dispositivo.</summary>
public interface ILocationService
{
    /// <summary>
    /// Obtém a localização atual do dispositivo.
    /// Retorna <see cref="Result{T}"/> com <c>Fail</c> se permissão negada ou timeout.
    /// </summary>
    Task<Result<(double Latitude, double Longitude)>> GetCurrentLocationAsync();
}
