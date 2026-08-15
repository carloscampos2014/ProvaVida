using System.Windows.Input;
using ProvaVida.Maui.Services;

namespace ProvaVida.Maui.ViewModels;

public class CadastroViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    // Passo 1 — Dados pessoais
    private string _nome = string.Empty;
    private string _email = string.Empty;
    private string _whatsApp = string.Empty;
    private string _senha = string.Empty;
    private bool _senhaOculta = true;

    // Passo 2 — Contato de emergência
    private string _contatoNome = string.Empty;
    private string _contatoEmail = string.Empty;
    private string _contatoWhatsApp = string.Empty;
    private bool _aceitouTermos = false;

    private int _passo = 1;

    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string WhatsApp { get => _whatsApp; set => SetProperty(ref _whatsApp, value); }
    public string Senha { get => _senha; set => SetProperty(ref _senha, value); }
    public bool SenhaOculta { get => _senhaOculta; set => SetProperty(ref _senhaOculta, value); }
    public string ContatoNome { get => _contatoNome; set => SetProperty(ref _contatoNome, value); }
    public string ContatoEmail { get => _contatoEmail; set => SetProperty(ref _contatoEmail, value); }
    public string ContatoWhatsApp { get => _contatoWhatsApp; set => SetProperty(ref _contatoWhatsApp, value); }
    public bool AceitouTermos { get => _aceitouTermos; set => SetProperty(ref _aceitouTermos, value); }
    public int Passo
    {
        get => _passo;
        set
        {
            if (SetPropertyInternal(ref _passo, value))
            {
                OnPropertyChanged(nameof(MostrarPasso1));
                OnPropertyChanged(nameof(MostrarPasso2));
            }
        }
    }
    public bool MostrarPasso1 => Passo == 1;
    public bool MostrarPasso2 => Passo == 2;

    public ICommand ProximoCommand { get; }
    public ICommand VoltarCommand { get; }
    public ICommand CadastrarCommand { get; }
    public ICommand IrParaLoginCommand { get; }
    public ICommand AlternarSenhaCommand { get; }

    public CadastroViewModel(IAuthService authService)
    {
        _authService = authService;

        ProximoCommand = new Command(() =>
        {
            LimparErro();
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(WhatsApp) || string.IsNullOrWhiteSpace(Senha))
            {
                ErrorMessage = "Preencha todos os campos.";
                return;
            }
            if (Senha.Length < 8)
            {
                ErrorMessage = "Senha deve ter no mínimo 8 caracteres.";
                return;
            }
            Passo = 2;
        });

        VoltarCommand = new Command(() =>
        {
            Passo = 1;
        });

        CadastrarCommand = new Command(async () => await CadastrarAsync(), () => !IsLoading);

        IrParaLoginCommand = new Command(async () =>
            await Shell.Current.GoToAsync("//login"));

        AlternarSenhaCommand = new Command(() => SenhaOculta = !SenhaOculta);
    }

    private async Task CadastrarAsync()
    {
        LimparErro();

        if (string.IsNullOrWhiteSpace(ContatoNome) || string.IsNullOrWhiteSpace(ContatoEmail) ||
            string.IsNullOrWhiteSpace(ContatoWhatsApp))
        {
            ErrorMessage = "Preencha todos os dados do contato de emergência.";
            return;
        }

        if (!AceitouTermos)
        {
            ErrorMessage = "Você precisa aceitar os Termos de Uso e a Política de Privacidade.";
            return;
        }

        IsLoading = true;
        try
        {
            await _authService.CadastrarAsync(new CadastroRequest(
                Nome.Trim(), Email.Trim(), WhatsApp.Trim(), Senha,
                ContatoNome.Trim(), ContatoEmail.Trim(), ContatoWhatsApp.Trim()));

            await Shell.Current.GoToAsync("//login");
            await Shell.Current.DisplayAlertAsync(
                "Conta criada", "Seu cadastro foi realizado com sucesso!", "OK");
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
