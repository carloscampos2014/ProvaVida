using ProvaVida.Maui.ViewModels;

namespace ProvaVida.Maui.Pages;

public partial class CadastroPage : ContentPage
{
    public CadastroPage(CadastroViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
