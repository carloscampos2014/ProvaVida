using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Application.Utils;
using ProvaVida.Shared.Dtos;

namespace ProvaVida.Mobile.App.ViewModels;

/// <summary>
/// ViewModel da tela de Cadastro. Valida conectividade e dados, computa o hash SHA-256
/// da senha e chama <see cref="IAuthMobileService.RegisterAsync"/> antes de navegar para Login.
/// </summary>
public partial class CadastroViewModel : ObservableObject
{
    private readonly IAuthMobileService _authService;
    private readonly ILogger<CadastroViewModel> _logger;

    /// <summary>Inicializa o ViewModel com as dependências necessárias.</summary>
    public CadastroViewModel(IAuthMobileService authService, ILogger<CadastroViewModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // ── Dados da conta ────────────────────────────────────────────────────────

    /// <summary>Nome completo do usuário.</summary>
    [ObservableProperty]
    private string nome = string.Empty;

    /// <summary>E-mail do usuário.</summary>
    [ObservableProperty]
    private string email = string.Empty;

    /// <summary>Senha em texto puro — hash SHA-256 gerado antes de enviar à API.</summary>
    [ObservableProperty]
    private string senha = string.Empty;

    /// <summary>Confirmação da senha — usada apenas para validação local, nunca enviada.</summary>
    [ObservableProperty]
    private string confirmacaoSenha = string.Empty;

    /// <summary>WhatsApp do usuário (somente dígitos, 10 ou 11 caracteres).</summary>
    [ObservableProperty]
    private string whatsapp = string.Empty;

    // ── Contato de emergência ─────────────────────────────────────────────────

    /// <summary>Nome do contato de emergência.</summary>
    [ObservableProperty]
    private string contatoNome = string.Empty;

    /// <summary>E-mail do contato de emergência.</summary>
    [ObservableProperty]
    private string contatoEmail = string.Empty;

    /// <summary>WhatsApp do contato de emergência (somente dígitos, 10 ou 11 caracteres).</summary>
    [ObservableProperty]
    private string contatoWhatsapp = string.Empty;

    // ── Estado da UI ──────────────────────────────────────────────────────────

    /// <summary>Indica que uma operação está em andamento (exibe ActivityIndicator).</summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>Mensagem de erro exibida abaixo do formulário. Vazio = sem erro.</summary>
    [ObservableProperty]
    private string mensagemErro = string.Empty;

    /// <summary>
    /// Valida conectividade e campos, computa hash SHA-256, chama o serviço de cadastro
    /// e navega para Login em caso de sucesso.
    /// RF-202: sem internet → exibir erro; campos vazios → exibir erro; senhas divergentes → exibir erro;
    /// sucesso → exibir mensagem e navegar para Login; erro da API → exibir mensagem recebida.
    /// </summary>
    [RelayCommand]
    private async Task CadastrarAsync()
    {
        MensagemErro = string.Empty;

        // RF-202.2 — sem internet: exibir mensagem e não enviar
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            MensagemErro = "Não é possível efetuar cadastro sem acesso à internet.";
            return;
        }

        // RF-202.3 — campos obrigatórios em branco
        if (string.IsNullOrWhiteSpace(Nome) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Senha) ||
            string.IsNullOrWhiteSpace(ConfirmacaoSenha) ||
            string.IsNullOrWhiteSpace(Whatsapp) ||
            string.IsNullOrWhiteSpace(ContatoNome) ||
            string.IsNullOrWhiteSpace(ContatoEmail) ||
            string.IsNullOrWhiteSpace(ContatoWhatsapp))
        {
            MensagemErro = "Preencha todos os campos obrigatórios.";
            return;
        }

        if (Senha != ConfirmacaoSenha)
        {
            MensagemErro = "As senhas não conferem.";
            return;
        }

        IsLoading = true;
        try
        {
            // RF-202.4 — gerar hash SHA-256 da senha antes de chamar a API
            var senhaHash = Sha256Helper.ComputeHash(Senha);

            var request = new RegisterRequest(
                Nome: Nome.Trim(),
                Email: Email.Trim(),
                Whatsapp: Whatsapp.Trim(),
                SenhaHash: senhaHash,
                ContatoEmergenciaNome: ContatoNome.Trim(),
                ContatoEmergenciaEmail: ContatoEmail.Trim(),
                ContatoEmergenciaWhatsapp: ContatoWhatsapp.Trim()
            );

            var resultado = await _authService.RegisterAsync(request);

            if (resultado.Success)
            {
                // RF-202.5 — exibir mensagem de sucesso e navegar para Login
                await Shell.Current.DisplayAlertAsync("Sucesso", "Cadastro realizado com sucesso!", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                // RF-202.6 — exibir mensagem de erro recebida do servidor
                MensagemErro = resultado.MessageErro;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante o cadastro.");
            MensagemErro = "Ocorreu um erro inesperado. Tente novamente.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Volta para a tela de Login sem efetuar cadastro.</summary>
    [RelayCommand]
    private async Task VoltarParaLoginAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
