using ProvaVida.Mobile.Application.Services;

namespace ProvaVida.Mobile.App.Services;

/// <summary>
/// Implementação de <see cref="IConnectivityService"/> que usa <c>Connectivity.Current</c>
/// do MAUI Essentials para verificar o estado de conectividade do dispositivo.
/// </summary>
public class MauiConnectivityService : IConnectivityService
{
    /// <inheritdoc/>
    public bool IsInternetAvailable =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}
