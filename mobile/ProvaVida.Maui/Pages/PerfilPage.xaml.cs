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
        _vm.AoExibir();
    }

    private void OnHamburgerTapped(object? sender, TappedEventArgs e)
        => Shell.Current.FlyoutIsPresented = true;
}
