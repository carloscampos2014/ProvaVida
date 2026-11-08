using ProvaVida.Mobile.App.ViewModels;

namespace ProvaVida.Mobile.App.Pages;

/// <summary>
/// Tela principal de check-in diário do app Prova de Vida.
/// Exibe o estado do check-in atual e permite registrar novo check-in.
/// O ViewModel é injetado via DI e definido como <c>BindingContext</c> da página.
/// </summary>
public partial class CheckinPage : ContentPage
{
    private readonly CheckinViewModel _viewModel;

    /// <summary>Inicializa a página com o ViewModel injetado.</summary>
    /// <param name="viewModel">ViewModel com lógica de check-in e estado da UI.</param>
    public CheckinPage(CheckinViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    /// <summary>
    /// Carrega o estado do check-in sempre que a tela se torna visível.
    /// Garante que a UI reflita o SQLite local sem necessitar de ação do usuário.
    /// RF-305.3: exibir dados do check-in existente ao abrir a tela.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarEstadoCommand.ExecuteAsync(null);
    }
}
