using ProvaVida.Mobile.Application.Services;

namespace ProvaVida.Mobile.App.Services;

/// <summary>
/// Implementação de <see cref="IDeviceInfoService"/> que usa <c>DeviceInfo.Current</c>
/// do MAUI Essentials para obter informações de identificação do aparelho.
/// </summary>
public class MauiDeviceInfoService : IDeviceInfoService
{
    /// <inheritdoc/>
    public string GetIdentificacaoAparelho() =>
        $"{DeviceInfo.Current.Model}/{DeviceInfo.Current.Name}";
}
