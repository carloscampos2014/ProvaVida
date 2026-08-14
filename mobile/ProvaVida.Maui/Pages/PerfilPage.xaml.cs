using ProvaVida.Maui.ViewModels;

namespace ProvaVida.Maui.Pages;

public partial class PerfilPage : ContentPage
{
    public PerfilPage(PerfilViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
