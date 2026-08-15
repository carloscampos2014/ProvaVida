using ProvaVida.Maui.Models;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Pages;

public partial class LoadingPage : ContentPage
{
    private readonly ITokenStorage _tokenStorage;
    private readonly IAuthService _authService;
    private readonly IUsuarioStorage _usuarioStorage;

    public LoadingPage(ITokenStorage tokenStorage, IAuthService authService, IUsuarioStorage usuarioStorage)
    {
        InitializeComponent();
        _tokenStorage = tokenStorage;
        _authService = authService;
        _usuarioStorage = usuarioStorage;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
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
            if (expiraEm.HasValue && expiraEm.Value > DateTime.UtcNow)
            {
                PopularUsuarioDoToken(token);
                await Shell.Current.GoToAsync("//checkin");
                return;
            }

            // Token expirado — tenta renovar
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

    private void PopularUsuarioDoToken(string token)
    {
        try
        {
            if (_usuarioStorage.Obter() is not null) return;

            var nome  = ExtrairClaim(token, "nome");
            var email = ExtrairClaim(token, "email");
            if (!string.IsNullOrEmpty(nome))
                _usuarioStorage.Salvar(new UsuarioLocal { Nome = nome, Email = email ?? string.Empty });
        }
        catch { }
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
