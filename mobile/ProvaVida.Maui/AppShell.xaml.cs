using ProvaVida.Maui.Pages;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui;

public partial class AppShell : Shell
{
    private readonly IUsuarioStorage _usuarioStorage;
    private readonly PerfilPage _perfilPage;

    public AppShell(IUsuarioStorage usuarioStorage, PerfilPage perfilPage)
    {
        InitializeComponent();
        _usuarioStorage = usuarioStorage;
        _perfilPage = perfilPage;
    }

    public void AtualizarFlyoutHeader()
    {
        var usuario = _usuarioStorage.Obter();
        if (usuario is null) return;

        FlyoutNome.Text   = usuario.Nome;
        FlyoutEmail.Text  = usuario.Email;
        FlyoutAvatar.Text = string.IsNullOrEmpty(usuario.Nome)
            ? "?"
            : usuario.Nome[0].ToString().ToUpper();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AtualizarFlyoutHeader();
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        // Garante carregamento do perfil ao navegar via flyout
        // Shell com DataTemplate pode não disparar OnAppearing consistentemente
        if (args.Current?.Location?.ToString()?.Contains("perfil") == true)
            _perfilPage.AoExibir();
    }
}
