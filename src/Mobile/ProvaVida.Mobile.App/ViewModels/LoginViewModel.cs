using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Auth;

namespace ProvaVida.Mobile.App.ViewModels;

/// <summary>
/// ViewModel da tela de Login. Coordena validação de conectividade, chamada ao
/// <see cref="IAuthMobileService"/> e navegação pós-autenticação.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthMobileService _authService;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>Inicializa o ViewModel com as dependências necessárias.</summary>
    public LoginViewModel(IAuthMobileService authService, ILogger<LoginViewModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>E-mail digitado pelo usuário.</summary>
    [ObservableProperty]
    private string email = string.Empty;

    /// <summary>Senha digitada pelo usuário (texto puro — hash gerado no serviço).</summary>
    [ObservableProperty]
    private string senha = string.Empty;

    /// <summary>Indica que uma operação está em andamento (exibe ActivityIndicator).</summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>Mensagem de erro exibida abaixo do formulário. Vazio = sem erro.</summary>
    [ObservableProperty]
    private string mensagemErro = string.Empty;

    /// <summary>
    /// Valida conectividade, chama o serviço de autenticação e navega para Check-in em caso de sucesso.
    /// RF-203: sem internet → exibir erro e encerrar; erro de credenciais → exibir mensagem e manter tela.
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        MensagemErro = string.Empty;

        // RF-203.2 — sem internet: exibir erro e encerrar o app
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            MensagemErro = "Sem conexão com a internet. O app será encerrado.";
            await Task.Delay(2000);
            Microsoft.Maui.Controls.Application.Current?.Quit();
            return;
        }

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Preencha e-mail e senha.";
            return;
        }

        IsLoading = true;
        try
        {
            // RF-203.3 — hash SHA-256 é gerado dentro do AuthMobileService
            var resultado = await _authService.LoginAsync(Email, Senha);

            if (resultado.Success)
            {
                // RF-203.4 — dados e tokens já foram persistidos pelo serviço; navegar para Check-in
                // A rota //CheckinPage será registrada na Task 8; por ora usa MainPage como placeholder
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                // RF-203.5 — credenciais inválidas
                MensagemErro = "Login ou senha inválidos.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante o login.");
            MensagemErro = "Ocorreu um erro inesperado. Tente novamente.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Navega para a tela de Cadastro.</summary>
    [RelayCommand]
    private async Task IrParaCadastroAsync()
    {
        await Shell.Current.GoToAsync("//CadastroPage");
    }
}
