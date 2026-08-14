using System.Windows.Input;
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

    private string _nome = string.Empty;
    private string _whatsApp = string.Empty;
    private string _contatoNome = string.Empty;
    private string _contatoEmail = string.Empty;
    private string _contatoWhatsApp = string.Empty;
    private string _email = string.Empty;

    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string WhatsApp { get => _whatsApp; set => SetProperty(ref _whatsApp, value); }
    public string ContatoNome { get => _contatoNome; set => SetProperty(ref _contatoNome, value); }
    public string ContatoEmail { get => _contatoEmail; set => SetProperty(ref _contatoEmail, value); }
    public string ContatoWhatsApp { get => _contatoWhatsApp; set => SetProperty(ref _contatoWhatsApp, value); }

    public ICommand SalvarCommand { get; }
    public ICommand LogoffCommand { get; }
    public ICommand ExcluirContaCommand { get; }

    public PerfilViewModel(
        IContaService contaService,
        IAuthService authService,
        ITokenStorage tokenStorage,
        IUsuarioStorage usuarioStorage)
    {
        _contaService = contaService;
        _authService = authService;
        _tokenStorage = tokenStorage;
        _usuarioStorage = usuarioStorage;

        SalvarCommand = new Command(async () => await SalvarAsync(), () => !IsLoading);
        LogoffCommand = new Command(async () => await LogoffAsync());
        ExcluirContaCommand = new Command(async () => await ExcluirContaAsync());

        CarregarDadosLocais();
    }

    private void CarregarDadosLocais()
    {
        var usuario = _usuarioStorage.Obter();
        if (usuario is null) return;

        Nome = usuario.Nome;
        Email = usuario.Email;
        WhatsApp = usuario.WhatsApp;
        ContatoNome = usuario.ContatoEmergenciaNome;
        ContatoEmail = usuario.ContatoEmergenciaEmail;
        ContatoWhatsApp = usuario.ContatoEmergenciaWhatsApp;
    }

    private async Task SalvarAsync()
    {
        LimparErro();

        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(WhatsApp) ||
            string.IsNullOrWhiteSpace(ContatoNome) || string.IsNullOrWhiteSpace(ContatoEmail) ||
            string.IsNullOrWhiteSpace(ContatoWhatsApp))
        {
            ErrorMessage = "Preencha todos os campos.";
            return;
        }

        IsLoading = true;
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
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LogoffAsync()
    {
        await _authService.LogoffAsync();
        await _tokenStorage.RemoverAsync();
        _usuarioStorage.Remover();
        await Shell.Current.GoToAsync("//login");
    }

    private async Task ExcluirContaAsync()
    {
        var confirmar = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Excluir conta",
            "Seus dados serão removidos permanentemente. Esta ação não pode ser desfeita.",
            "Continuar", "Cancelar");

        if (!confirmar) return;

        var senha = await Application.Current.Windows[0].Page!.DisplayPromptAsync(
            "Confirmar senha",
            "Digite sua senha para confirmar a exclusão:",
            keyboard: Keyboard.Default,
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(senha)) return;

        IsLoading = true;
        try
        {
            await _contaService.ExcluirAsync(new ExcluirContaRequest(senha));
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
        finally
        {
            IsLoading = false;
        }
    }
}
