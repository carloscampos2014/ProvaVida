using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Services;
using ProvaVida.Shared.Common;

namespace ProvaVida.Mobile.App.Services;

/// <summary>
/// Implementação de <see cref="ILocationService"/> que usa as APIs nativas do MAUI
/// (<see cref="Geolocation.Default"/>) para obter a localização do dispositivo.
/// </summary>
public class MauiLocationService : ILocationService
{
    private readonly ILogger<MauiLocationService> _logger;

    /// <summary>Inicializa uma nova instância de <see cref="MauiLocationService"/>.</summary>
    public MauiLocationService(ILogger<MauiLocationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<(double Latitude, double Longitude)>> GetCurrentLocationAsync()
    {
        try
        {
            // Tentativa 1: localização cacheada (rápida, sem prompt de permissão adicional)
            var location = await Geolocation.Default.GetLastKnownLocationAsync();

            if (location is null)
            {
                // Tentativa 2: localização nova via GPS/rede
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                location = await Geolocation.Default.GetLocationAsync(request);
            }

            if (location is null)
            {
                _logger.LogWarning("Localização indisponível após duas tentativas.");
                return Result<(double, double)>.Fail("Não foi possível obter a localização");
            }

            return Result<(double, double)>.Ok((location.Latitude, location.Longitude));
        }
        catch (PermissionException ex)
        {
            _logger.LogWarning(ex, "Permissão de localização negada.");
            return Result<(double, double)>.Fail("Permissão de localização necessária para o check-in");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter localização.");
            return Result<(double, double)>.Fail("Não foi possível obter a localização");
        }
    }
}
