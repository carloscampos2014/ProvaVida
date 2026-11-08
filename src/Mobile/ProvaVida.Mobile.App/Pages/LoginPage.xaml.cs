using ProvaVida.Mobile.App.ViewModels;

namespace ProvaVida.Mobile.App.Pages;

/// <summary>
/// Tela de login do app Prova de Vida.
/// O ViewModel é injetado via DI e definido como <c>BindingContext</c> da página.
/// </summary>
public partial class LoginPage : ContentPage
{
    /// <summary>Inicializa a página com o ViewModel injetado.</summary>
    /// <param name="viewModel">ViewModel com lógica de autenticação e navegação.</param>
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
