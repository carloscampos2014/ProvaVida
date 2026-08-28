using System.Windows.Input;
using ProvaVida.Maui.Models;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;
namespace ProvaVida.Maui.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IContaService _contaService;
    private readonly ITokenStorage _tokenStorage;
    private readonly IUsuarioStorage _usuarioStorage;

    private string _email = string.Empty;
    private string _senha = string.Empty;
    private bool _senhaOculta = true;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Senha
    {
        get => _senha;
        set => SetProperty(ref _senha, value);
    }

    public bool SenhaOculta
    {
        get => _senhaOculta;
        set => SetProperty(ref _senhaOculta, value);
    }

    public ICommand EntrarCommand { get; }
    public ICommand IrParaCadastroCommand { get; }
    public ICommand AlternarSenhaCommand { get; }

    public LoginViewModel(
        IAuthService authService,
        IContaService contaService,
        ITokenStorage tokenStorage,
        IUsuarioStorage usuarioStorage)
    {
        _authService = authService;
        _contaService = contaService;
        _tokenStorage = tokenStorage;
        _usuarioStorage = usuarioStorage;

        EntrarCommand = new Command(async () => await ExecutarSeOcioso(EntrarAsync), () => !IsLoading);
        IrParaCadastroCommand = new Command(async () =>
            await Shell.Current.GoToAsync("//cadastro"));
        AlternarSenhaCommand = new Command(() => SenhaOculta = !SenhaOculta);
    }

    /// <summary>
    /// Extrai um claim do payload do JWT sem validar a assinatura.
    /// Usado apenas para leitura de dados não-sensíveis (nome, email).
    /// </summary>
    private static string? ExtrairClaimDoToken(string token, string claim)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            while (payload.Length % 4 != 0) payload += '=';

            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(claim, out var v) ? v.GetString() : null;
        }
        catch { return null; }
    }

    private async Task EntrarAsync()
    {
        LimparErro();

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            ErrorMessage = "Preencha e-mail e senha.";
            return;
        }

        try
        {
            var result = await _authService.LoginAsync(new LoginRequest(Email.Trim(), Senha));
            await _tokenStorage.SalvarAsync(result.Token);
            await _tokenStorage.SalvarExpiraEmAsync(result.ExpiraEm);
            await _tokenStorage.SalvarRefreshTokenAsync(result.RefreshToken);

            // Salva nome e email do JWT imediatamente (disponível offline)
            var nome  = ExtrairClaimDoToken(result.Token, "nome");
            var email = ExtrairClaimDoToken(result.Token, "email");
            _usuarioStorage.Salvar(new UsuarioLocal
            {
                Nome  = nome  ?? string.Empty,
                Email = email ?? Email.Trim()
            });

            // Agenda alarmes diários — uma vez no login é suficiente.
            // RepeatType.Daily + AlarmManager garantem disparo mesmo sem o app aberto.
            // BootReceiver reagenda após reboot.
            await LocalNotificationService.AgendarLembreteAsync();
            await LocalNotificationService.AgendarAvisoInatividadeAsync();

            // Busca perfil completo da API em background para popular WhatsApp e contatos
            _ = Task.Run(async () =>
            {
                try
                {
                    var perfil = await _contaService.ObterPerfilAsync();
                    if (perfil is null) return;
                    _usuarioStorage.Salvar(new UsuarioLocal
                    {
                        Nome                      = perfil.Nome,
                        Email                     = perfil.Email,
                        WhatsApp                  = perfil.WhatsApp,
                        ContatoEmergenciaNome     = perfil.ContatoEmergenciaNome,
                        ContatoEmergenciaEmail    = perfil.ContatoEmergenciaEmail,
                        ContatoEmergenciaWhatsApp = perfil.ContatoEmergenciaWhatsApp
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Falha ao buscar perfil completo: {ex.Message}");
                }
            });

            await Shell.Current.GoToAsync("//checkin");

#if ANDROID
            // Atualiza widgets e shortcuts para refletir estado logado
            try
            {
                var ctx = Android.App.Application.Context;
                CheckInWidgetSimples.AtualizarTodos(ctx);
                if (OperatingSystem.IsAndroidVersionAtLeast(25))
                    AppShortcutsService.Atualizar();
            }
            catch { }
#endif
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = "Sem conexão com o servidor. Verifique sua internet.";
        }
    }
}
