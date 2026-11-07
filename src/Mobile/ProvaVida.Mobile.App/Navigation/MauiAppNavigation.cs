using ProvaVida.Mobile.Application.Navigation;

namespace ProvaVida.Mobile.App.Navigation;

/// <summary>
/// Implementação de <see cref="IAppNavigation"/> que delega para <c>Shell.Current</c> do MAUI.
/// Permite que serviços de <c>Mobile.Application</c> (ex.: <c>TokenManager</c>) disparem
/// navegação para Login sem depender diretamente do framework.
/// </summary>
public class MauiAppNavigation : IAppNavigation
{
    /// <inheritdoc/>
    public Task NavigateToLoginAsync()
        => Shell.Current.GoToAsync("//LoginPage");
}
