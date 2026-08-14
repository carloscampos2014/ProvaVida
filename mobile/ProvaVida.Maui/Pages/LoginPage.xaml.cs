using ProvaVida.Maui.ViewModels;

namespace ProvaVida.Maui.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
