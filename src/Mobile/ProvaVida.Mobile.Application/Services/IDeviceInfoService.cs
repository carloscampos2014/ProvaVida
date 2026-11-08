namespace ProvaVida.Mobile.Application.Services;

/// <summary>Abstração para obter informações do dispositivo.</summary>
/// <remarks>
/// A implementação concreta (<c>MauiDeviceInfoService</c>) usa
/// <c>DeviceInfo.Current.Model + "/" + DeviceInfo.Current.Name</c> do MAUI Essentials
/// e é registrada no DI em <c>MauiProgram.cs</c>.
/// Esta interface existe para permitir testes unitários sem dependência de APIs nativas.
/// </remarks>
public interface IDeviceInfoService
{
    /// <summary>
    /// Retorna a identificação do aparelho no formato <c>"Modelo/Nome"</c>.
    /// Equivale a <c>DeviceInfo.Current.Model + "/" + DeviceInfo.Current.Name</c>.
    /// </summary>
    string GetIdentificacaoAparelho();
}
