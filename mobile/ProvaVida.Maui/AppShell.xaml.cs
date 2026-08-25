using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui;

public partial class AppShell : Shell
{
    private readonly IUsuarioStorage _usuarioStorage;

    public AppShell(IUsuarioStorage usuarioStorage)
    {
        InitializeComponent();
        _usuarioStorage = usuarioStorage;
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
        // Obtém PerfilPage do DI apenas quando necessário — evita instanciar antes dos recursos XAML estarem prontos
        if (args.Current?.Location?.ToString()?.Contains("perfil") == true)
        {
            try
            {
                var perfilPage = IPlatformApplication.Current?.Services.GetService<Pages.PerfilPage>();
                perfilPage?.AoExibir();
            }
            catch { }
        }
    }
}
