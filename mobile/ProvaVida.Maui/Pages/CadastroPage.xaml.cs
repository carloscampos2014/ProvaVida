using ProvaVida.Maui.ViewModels;

namespace ProvaVida.Maui.Pages;

public partial class CadastroPage : ContentPage
{
    private readonly CadastroViewModel _vm;

    public CadastroPage(CadastroViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Limpa o formulário ao abrir a tela — evita dados residuais de sessão anterior
        _vm.Limpar();
    }
}
