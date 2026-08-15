using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly ITokenStorage _tokenStorage;
    private readonly IAuthService _authService;

    public App(AppShell shell, ITokenStorage tokenStorage, IAuthService authService)
    {
        InitializeComponent();
        _shell = shell;
        _tokenStorage = tokenStorage;
        _authService = authService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_shell);

    protected override async void OnStart()
    {
        base.OnStart();
        await VerificarSessaoAsync();
    }

    private async Task VerificarSessaoAsync()
    {
        try
        {
            var token = await _tokenStorage.ObterAsync();
            if (string.IsNullOrEmpty(token))
            {
                // Sem token — vai para login
                await Shell.Current.GoToAsync("//login");
                return;
            }

            var expiraEm = await _tokenStorage.ObterExpiraEmAsync();
            var agora = DateTime.UtcNow;

            if (expiraEm.HasValue && expiraEm.Value > agora)
            {
                // Token ainda válido — vai para check-in
                await Shell.Current.GoToAsync("//checkin");
                return;
            }

            // Token expirado — tenta renovar com refresh token
            var renovado = await _authService.TentarRenovarTokenAsync();
            if (renovado)
            {
                await Shell.Current.GoToAsync("//checkin");
            }
            else
            {
                // Refresh token inválido ou sem internet — vai para login
                await Shell.Current.GoToAsync("//login");
            }
        }
        catch
        {
            // Falha inesperada — vai para login por segurança
            await Shell.Current.GoToAsync("//login");
        }
    }
}
