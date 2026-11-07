using ProvaVida.Mobile.App.ViewModels;

namespace ProvaVida.Mobile.App.Pages;

/// <summary>
/// Code-behind da tela de Cadastro. A lógica de negócio fica em <see cref="CadastroViewModel"/>.
/// </summary>
public partial class CadastroPage : ContentPage
{
    /// <summary>
    /// Inicializa a página e define o <see cref="CadastroViewModel"/> como BindingContext.
    /// </summary>
    /// <param name="vm">ViewModel injetado via DI.</param>
    public CadastroPage(CadastroViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
