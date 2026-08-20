using ProvaVida.Maui.ViewModels;

namespace ProvaVida.Maui.Pages;

public partial class PerfilPage : ContentPage
{
    private readonly PerfilViewModel _vm;

    public PerfilPage(PerfilViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Shell reutiliza a instância da página — recarrega dados em toda visita
        _vm.AoExibir();
    }
}
