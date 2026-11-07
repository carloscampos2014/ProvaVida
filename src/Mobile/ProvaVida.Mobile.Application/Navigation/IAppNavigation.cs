namespace ProvaVida.Mobile.Application.Navigation;

/// <summary>
/// Abstração de navegação para uso em camadas sem referência direta ao MAUI.
/// </summary>
/// <remarks>
/// A implementação concreta usa <c>Shell.Current.GoToAsync</c> no projeto <c>Mobile.App</c>.
/// Essa interface permite que <c>TokenManager</c> (e outros serviços de Application) disparem
/// navegação para Login sem criar dependência de framework na camada de aplicação.
/// </remarks>
public interface IAppNavigation
{
    /// <summary>
    /// Navega para a tela de Login, descartando toda a pilha de navegação anterior.
    /// </summary>
    Task NavigateToLoginAsync();
}
