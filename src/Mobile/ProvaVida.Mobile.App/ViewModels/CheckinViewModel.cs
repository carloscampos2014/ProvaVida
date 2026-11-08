using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.App.ViewModels;

/// <summary>
/// ViewModel da tela de Check-in. Coordena a leitura do estado atual do check-in
/// e o registro de novos check-ins via <see cref="ICheckinMobileService"/>.
/// </summary>
public partial class CheckinViewModel : ObservableObject
{
    private readonly ICheckinMobileService _checkinService;
    private readonly ILogger<CheckinViewModel> _logger;

    /// <summary>Inicializa o ViewModel com as dependências necessárias.</summary>
    public CheckinViewModel(ICheckinMobileService checkinService, ILogger<CheckinViewModel> logger)
    {
        _checkinService = checkinService;
        _logger = logger;
    }

    /// <summary>Check-in do dia atual, ou null se nenhum registrado.</summary>
    [ObservableProperty]
    public partial Checkin? CheckinHoje { get; set; }

    /// <summary>Indica que uma operação está em andamento (exibe ActivityIndicator).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FazerCheckinCommand))]
    public partial bool IsLoading { get; set; }

    /// <summary>Mensagem de erro exibida abaixo do botão. Vazio = sem mensagem.</summary>
    [ObservableProperty]
    public partial string MensagemErro { get; set; } = string.Empty;

    /// <summary>True quando já existe check-in do dia (desabilita o botão de registro).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FazerCheckinCommand))]
    public partial bool JaFezCheckin { get; set; }

    /// <summary>
    /// Registra o check-in do dia: captura localização, salva no SQLite
    /// e sincroniza com a API se houver internet.
    /// RF-302, RF-303, RF-304: offline-first; permissão de localização obrigatória.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanFazerCheckin))]
    private async Task FazerCheckinAsync()
    {
        MensagemErro = string.Empty;
        IsLoading = true;
        try
        {
            var resultado = await _checkinService.RegistrarAsync();
            if (resultado.Success)
            {
                CheckinHoje = resultado.Data;
                JaFezCheckin = true;
            }
            else
            {
                MensagemErro = resultado.MessageErro;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao registrar check-in.");
            MensagemErro = "Ocorreu um erro inesperado. Tente novamente.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Predicado de habilitação do botão: habilitado apenas quando não está carregando
    /// e ainda não foi feito check-in hoje.
    /// </summary>
    private bool CanFazerCheckin() => !IsLoading && !JaFezCheckin;

    /// <summary>
    /// Carrega o estado do check-in do dia a partir do SQLite local.
    /// Chamado automaticamente pela <c>CheckinPage</c> no OnAppearing.
    /// </summary>
    [RelayCommand]
    private async Task CarregarEstadoAsync()
    {
        MensagemErro = string.Empty;
        IsLoading = true;
        try
        {
            var resultado = await _checkinService.GetCheckinHojeAsync();
            if (resultado.Success)
            {
                CheckinHoje = resultado.Data;
                JaFezCheckin = resultado.Data is not null;
            }
            else
            {
                MensagemErro = resultado.MessageErro;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao carregar estado do check-in.");
            MensagemErro = "Ocorreu um erro inesperado ao carregar os dados.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
