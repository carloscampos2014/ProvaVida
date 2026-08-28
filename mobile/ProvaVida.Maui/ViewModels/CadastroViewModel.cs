using System.Windows.Input;
using ProvaVida.Maui.Helpers;
using ProvaVida.Maui.Models;
using ProvaVida.Maui.Services;

namespace ProvaVida.Maui.ViewModels;

public class CadastroViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    // Passo 1 — Dados pessoais
    private string _nome = string.Empty;
    private string _email = string.Empty;
    private string _whatsAppNumero = string.Empty;
    private PaisDdi _paisWhatsApp = PaisDdi.Padrao;
    private string _senha = string.Empty;
    private bool _senhaOculta = true;

    // Passo 2 — Contato de emergência
    private string _contatoNome = string.Empty;
    private string _contatoEmail = string.Empty;
    private string _contatoWhatsAppNumero = string.Empty;
    private PaisDdi _paisContatoWhatsApp = PaisDdi.Padrao;
    private bool _aceitouTermos = false;

    private int _passo = 1;

    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string WhatsAppNumero { get => _whatsAppNumero; set => SetProperty(ref _whatsAppNumero, value); }
    public PaisDdi PaisWhatsApp { get => _paisWhatsApp; set => SetProperty(ref _paisWhatsApp, value); }
    public string WhatsApp => $"{PaisWhatsApp.Codigo}{WhatsAppNumero}";
    public string Senha { get => _senha; set => SetProperty(ref _senha, value); }
    public bool SenhaOculta { get => _senhaOculta; set => SetProperty(ref _senhaOculta, value); }
    public string ContatoNome { get => _contatoNome; set => SetProperty(ref _contatoNome, value); }
    public string ContatoEmail { get => _contatoEmail; set => SetProperty(ref _contatoEmail, value); }
    public string ContatoWhatsAppNumero { get => _contatoWhatsAppNumero; set => SetProperty(ref _contatoWhatsAppNumero, value); }
    public PaisDdi PaisContatoWhatsApp { get => _paisContatoWhatsApp; set => SetProperty(ref _paisContatoWhatsApp, value); }
    public string ContatoWhatsApp => $"{PaisContatoWhatsApp.Codigo}{ContatoWhatsAppNumero}";
    public bool AceitouTermos { get => _aceitouTermos; set => SetProperty(ref _aceitouTermos, value); }
    public IReadOnlyList<PaisDdi> PaisesDdi => PaisDdi.Todos;
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
                string.IsNullOrWhiteSpace(WhatsAppNumero) || string.IsNullOrWhiteSpace(Senha))
            {
                ErrorMessage = "Preencha todos os campos.";
                return;
            }
            if (!ValidacaoHelper.EmailValido(Email))
            {
                ErrorMessage = ValidacaoHelper.MsgEmailInvalido;
                return;
            }
            if (!ValidacaoHelper.WhatsAppValido(WhatsApp))
            {
                ErrorMessage = ValidacaoHelper.MsgWhatsAppInvalido;
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

        CadastrarCommand = new Command(async () => await ExecutarSeOcioso(CadastrarAsync), () => !IsLoading);

        IrParaLoginCommand = new Command(async () =>
            await Shell.Current.GoToAsync("//login"));

        AlternarSenhaCommand = new Command(() => SenhaOculta = !SenhaOculta);
    }

    private async Task CadastrarAsync()
    {
        LimparErro();

        if (string.IsNullOrWhiteSpace(ContatoNome) || string.IsNullOrWhiteSpace(ContatoEmail) ||
            string.IsNullOrWhiteSpace(ContatoWhatsAppNumero))
        {
            ErrorMessage = "Preencha todos os dados do contato de emergência.";
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

        if (!AceitouTermos)
        {
            ErrorMessage = "Você precisa aceitar os Termos de Uso e a Política de Privacidade.";
            return;
        }

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
    }
}
