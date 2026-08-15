using ProvaVida.Maui.Models;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly ITokenStorage _tokenStorage;
    private readonly IAuthService _authService;
    private readonly IUsuarioStorage _usuarioStorage;

    public App(AppShell shell, ITokenStorage tokenStorage, IAuthService authService, IUsuarioStorage usuarioStorage)
    {
        InitializeComponent();
        _shell = shell;
        _tokenStorage = tokenStorage;
        _authService = authService;
        _usuarioStorage = usuarioStorage;
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
                await Shell.Current.GoToAsync("//login");
                return;
            }

            var expiraEm = await _tokenStorage.ObterExpiraEmAsync();
            var agora = DateTime.UtcNow;

            if (expiraEm.HasValue && expiraEm.Value > agora)
            {
                // Token ainda válido — garante nome salvo e vai para check-in
                PopularUsuarioDoToken(token);
                await Shell.Current.GoToAsync("//checkin");
                return;
            }

            // Token expirado — tenta renovar com refresh token
            var renovado = await _authService.TentarRenovarTokenAsync();
            if (renovado)
            {
                var novoToken = await _tokenStorage.ObterAsync();
                if (!string.IsNullOrEmpty(novoToken)) PopularUsuarioDoToken(novoToken);
                await Shell.Current.GoToAsync("//checkin");
            }
            else
            {
                await Shell.Current.GoToAsync("//login");
            }
        }
        catch
        {
            await Shell.Current.GoToAsync("//login");
        }
    }

    /// <summary>
    /// Extrai nome e email do JWT e salva no UsuarioStorage se ainda não estiver salvo.
    /// </summary>
    private void PopularUsuarioDoToken(string token)
    {
        try
        {
            var usuarioExistente = _usuarioStorage.Obter();
            if (usuarioExistente is not null) return; // já está salvo

            var nome  = ExtrairClaim(token, "nome");
            var email = ExtrairClaim(token, "email");

            if (!string.IsNullOrEmpty(nome))
            {
                _usuarioStorage.Salvar(new UsuarioLocal
                {
                    Nome  = nome,
                    Email = email ?? string.Empty
                });
            }
        }
        catch { /* falha silenciosa — não impede a navegação */ }
    }

    private static string? ExtrairClaim(string token, string claim)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            while (payload.Length % 4 != 0) payload += '=';

            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var doc  = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(claim, out var v) ? v.GetString() : null;
        }
        catch { return null; }
    }
}
