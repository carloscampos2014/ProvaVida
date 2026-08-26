using System.Windows.Input;
using Microsoft.Extensions.Logging;
using ProvaVida.Maui.Helpers;
using ProvaVida.Maui.Models;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.ViewModels;

public class PerfilViewModel : BaseViewModel
{
    private readonly IContaService _contaService;
    private readonly IAuthService _authService;
    private readonly ITokenStorage _tokenStorage;
    private readonly IUsuarioStorage _usuarioStorage;
    private readonly LocalDatabase _localDatabase;
    private readonly ILogger<PerfilViewModel> _logger;

    private string _nome = string.Empty;
    private string _whatsApp = string.Empty;
    private string _contatoNome = string.Empty;
    private string _contatoEmail = string.Empty;
    private string _contatoWhatsApp = string.Empty;
    private string _email = string.Empty;

    public string Nome
    {
        get => _nome;
        set
        {
            if (SetPropertyInternal(ref _nome, value))
                OnPropertyChanged(nameof(Inicial));
        }
    }
    public string Inicial => string.IsNullOrEmpty(_nome) ? "?" : _nome[0].ToString().ToUpper();
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string WhatsApp { get => _whatsApp; set => SetProperty(ref _whatsApp, value); }
    public string ContatoNome { get => _contatoNome; set => SetProperty(ref _contatoNome, value); }
    public string ContatoEmail { get => _contatoEmail; set => SetProperty(ref _contatoEmail, value); }
    public string ContatoWhatsApp { get => _contatoWhatsApp; set => SetProperty(ref _contatoWhatsApp, value); }

    public ICommand SalvarCommand { get; }
    public ICommand AlterarSenhaCommand { get; }
    public ICommand LogoffCommand { get; }
    public ICommand ExcluirContaCommand { get; }

    public PerfilViewModel(
        IContaService contaService,
        IAuthService authService,
        ITokenStorage tokenStorage,
        IUsuarioStorage usuarioStorage,
        LocalDatabase localDatabase,
        ILogger<PerfilViewModel> logger)
    {
        _contaService = contaService;
        _authService = authService;
        _tokenStorage = tokenStorage;
        _usuarioStorage = usuarioStorage;
        _localDatabase = localDatabase;
        _logger = logger;

        SalvarCommand = new Command(async () => await ExecutarSeOcioso(SalvarAsync), () => !IsLoading);
        AlterarSenhaCommand = new Command(async () => await AlterarSenhaAsync());
        LogoffCommand = new Command(async () => await LogoffAsync());
        ExcluirContaCommand = new Command(async () => await ExecutarSeOcioso(ExcluirContaAsync));

        // Não carrega no construtor — OnAppearing da PerfilPage garante o carregamento
        // correto em toda visita, inclusive após logoff/login (Shell reutiliza a instância).
    }

    /// <summary>
    /// Chamado pelo OnAppearing da PerfilPage. Garante recarga em toda visita à página.
    /// </summary>
    public void AoExibir()
    {
        // Tenta carregar do cache local primeiro (resposta imediata)
        CarregarDadosLocais();
        // Sempre busca da API — garante dados atualizados mesmo com cache vazio
        _ = CarregarDadosDaApiAsync();
    }

    private void CarregarDadosLocais()
    {
        var usuario = _usuarioStorage.Obter();
        if (usuario is null || string.IsNullOrEmpty(usuario.Nome)) return;

        Nome            = usuario.Nome;
        Email           = usuario.Email;
        WhatsApp        = usuario.WhatsApp;
        ContatoNome     = usuario.ContatoEmergenciaNome;
        ContatoEmail    = usuario.ContatoEmergenciaEmail;
        ContatoWhatsApp = usuario.ContatoEmergenciaWhatsApp;
    }

    private async Task CarregarDadosDaApiAsync()
    {
        try
        {
            // Diagnóstico: verificar se token existe antes de chamar a API
            var tokenDiag = await _tokenStorage.ObterAsync();
            var expiraDiag = await _tokenStorage.ObterExpiraEmAsync();
            _logger.LogWarning("[PerfilViewModel] Token presente: {Presente}, Expira: {Expira}",
                !string.IsNullOrEmpty(tokenDiag),
                expiraDiag?.ToString("dd/MM/yyyy HH:mm:ss") ?? "null");

            var perfil = await _contaService.ObterPerfilAsync();
            if (perfil is null)
            {
                ErrorMessage = "[DEBUG] API retornou null — verifique o log";
                _logger.LogError("[PerfilViewModel] ObterPerfilAsync retornou null");
                return;
            }

            Nome            = perfil.Nome;
            Email           = perfil.Email;
            WhatsApp        = perfil.WhatsApp;
            ContatoNome     = perfil.ContatoEmergenciaNome;
            ContatoEmail    = perfil.ContatoEmergenciaEmail;
            ContatoWhatsApp = perfil.ContatoEmergenciaWhatsApp;

            // Atualiza cache local
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
            ErrorMessage = $"[DEBUG] Erro ao carregar perfil: {ex.Message}";
            _logger.LogError(ex, "[PerfilViewModel] Falha ao carregar perfil da API");
            System.Diagnostics.Debug.WriteLine($"[PerfilViewModel] Falha ao carregar perfil da API: {ex.Message}");
        }
    }    private async Task SalvarAsync()
    {
        LimparErro();

        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(WhatsApp) ||
            string.IsNullOrWhiteSpace(ContatoNome) || string.IsNullOrWhiteSpace(ContatoEmail) ||
            string.IsNullOrWhiteSpace(ContatoWhatsApp))
        {
            ErrorMessage = "Preencha todos os campos.";
            return;
        }

        if (!ValidacaoHelper.WhatsAppValido(WhatsApp))
        {
            ErrorMessage = ValidacaoHelper.MsgWhatsAppInvalido;
            return;
        }

        if (!ValidacaoHelper.EmailValido(ContatoEmail))
        {
            ErrorMessage = $"E-mail do contato: {ValidacaoHelper.MsgEmailInvalido}";
            return;
        }

        if (!ValidacaoHelper.WhatsAppValido(ContatoWhatsApp))
        {
            ErrorMessage = $"WhatsApp do contato: {ValidacaoHelper.MsgWhatsAppInvalido}";
            return;
        }

        try
        {
            await _contaService.AlterarAsync(new AlterarContaRequest(
                Nome.Trim(), WhatsApp.Trim(),
                ContatoNome.Trim(), ContatoEmail.Trim(), ContatoWhatsApp.Trim()));

            // Atualiza cache local
            _usuarioStorage.Salvar(new UsuarioLocal
            {
                Nome = Nome.Trim(),
                Email = Email,
                WhatsApp = WhatsApp.Trim(),
                ContatoEmergenciaNome = ContatoNome.Trim(),
                ContatoEmergenciaEmail = ContatoEmail.Trim(),
                ContatoEmergenciaWhatsApp = ContatoWhatsApp.Trim()
            });

            await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
                "Salvo", "Dados atualizados com sucesso.", "OK");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = "Sem conexão. Tente novamente.";
        }
    }

    private async Task AlterarSenhaAsync()
    {
        var page = Application.Current!.Windows[0].Page!;

        var senhaAtual = await page.DisplayPromptAsync(
            "Alterar senha",
            "Digite sua senha atual:",
            keyboard: Keyboard.Default,
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(senhaAtual)) return;

        var novaSenha = await page.DisplayPromptAsync(
            "Alterar senha",
            "Digite a nova senha (mínimo 8 caracteres):",
            keyboard: Keyboard.Default,
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(novaSenha)) return;

        if (novaSenha.Length < 8)
        {
            await page.DisplayAlertAsync("Erro", "A nova senha deve ter no mínimo 8 caracteres.", "OK");
            return;
        }

        IsLoading = true;
        try
        {
            await _contaService.AlterarSenhaAsync(new AlterarSenhaRequest(senhaAtual, novaSenha));            await page.DisplayAlertAsync("Sucesso", "Senha alterada com sucesso.", "OK");
        }
        catch (ApiException ex)
        {
            await page.DisplayAlertAsync("Erro", ex.Message, "OK");
        }
        catch
        {
            await page.DisplayAlertAsync("Erro", "Sem conexão. Tente novamente.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LogoffAsync()
    {
        // Reseta campos em memória antes de navegar — Shell reutiliza esta instância
        Nome            = string.Empty;
        Email           = string.Empty;
        WhatsApp        = string.Empty;
        ContatoNome     = string.Empty;
        ContatoEmail    = string.Empty;
        ContatoWhatsApp = string.Empty;

        await _authService.LogoffAsync();
        await _tokenStorage.RemoverAsync();
        _usuarioStorage.Remover();

        // Atualiza widgets e shortcuts para refletir estado deslogado
#if ANDROID
        try
        {
            var ctx = Android.App.Application.Context;
            CheckInWidgetSimples.AtualizarTodos(ctx);
            CheckInWidgetCompleto.AtualizarTodos(ctx);
            if (OperatingSystem.IsAndroidVersionAtLeast(25))
                AppShortcutsService.Atualizar();
        }
        catch { }
#endif

        await Shell.Current.GoToAsync("//login");
    }

    private async Task ExcluirContaAsync()
    {
        var confirmar = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Excluir conta",
            "Todos os seus dados serão removidos permanentemente. Esta ação não pode ser desfeita.",
            "Continuar", "Cancelar");

        if (!confirmar) return;

        var senha = await Application.Current.Windows[0].Page!.DisplayPromptAsync(
            "Confirmar senha",
            "Digite sua senha para confirmar a exclusão:",
            keyboard: Keyboard.Default,
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(senha)) return;

        try
        {
            await _contaService.ExcluirAsync(new ExcluirContaRequest(senha));

            // Limpa dados locais do SQLite e armazenamento
            await _localDatabase.LimparDadosLocaisAsync();
            await _tokenStorage.RemoverAsync();
            _usuarioStorage.Remover();

            await Shell.Current.GoToAsync("//login");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = "Sem conexão. Tente novamente.";
        }
    }
}