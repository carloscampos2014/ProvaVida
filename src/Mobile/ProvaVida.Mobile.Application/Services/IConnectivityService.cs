namespace ProvaVida.Mobile.Application.Services;

/// <summary>Abstração para verificar o estado de conectividade de rede do dispositivo.</summary>
/// <remarks>
/// A implementação concreta (<c>MauiConnectivityService</c>) usa <c>Connectivity.Current</c>
/// do MAUI Essentials e é registrada no DI em <c>MauiProgram.cs</c>.
/// Esta interface existe para permitir testes unitários sem dependência de APIs nativas.
/// </remarks>
public interface IConnectivityService
{
    /// <summary>
    /// Indica se o dispositivo tem acesso à internet no momento da chamada.
    /// </summary>
    bool IsInternetAvailable { get; }
}
