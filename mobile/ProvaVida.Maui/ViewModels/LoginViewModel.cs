using System.Windows.Input;
using ProvaVida.Maui.Models;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ITokenStorage _tokenStorage;
    private readonly IUsuarioStorage _usuarioStorage;

    private string _email = string.Empty;
    private string _senha = string.Empty;

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

    public ICommand EntrarCommand { get; }
    public ICommand IrParaCadastroCommand { get; }

    public LoginViewModel(
        IAuthService authService,
        ITokenStorage tokenStorage,
        IUsuarioStorage usuarioStorage)
    {
        _authService = authService;
        _tokenStorage = tokenStorage;
        _usuarioStorage = usuarioStorage;

        EntrarCommand = new Command(async () => await EntrarAsync(), () => !IsLoading);
        IrParaCadastroCommand = new Command(async () =>
            await Shell.Current.GoToAsync("//cadastro"));
    }

    private async Task EntrarAsync()
    {
        LimparErro();

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            ErrorMessage = "Preencha e-mail e senha.";
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _authService.LoginAsync(new LoginRequest(Email.Trim(), Senha));
            await _tokenStorage.SalvarAsync(result.Token);
            await Shell.Current.GoToAsync("//checkin");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = "Sem conexão com o servidor. Verifique sua internet.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
