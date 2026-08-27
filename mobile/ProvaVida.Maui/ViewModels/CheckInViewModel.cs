using System.Windows.Input;
using ProvaVida.Maui.Models;
using ProvaVida.Maui.Services;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.ViewModels;

public class CheckInViewModel : BaseViewModel
{
    private readonly LocalDatabase _db;
    private readonly ICheckInService _checkInService;
    private readonly LocationService _locationService;
    private readonly SyncService _syncService;
    private readonly IUsuarioStorage _usuarioStorage;

    private bool _fezCheckInHoje;
    private int _sequenciaDias;
    private List<bool> _semana = new(7);
    private string _nomeUsuario = string.Empty;

    public bool FezCheckInHoje { get => _fezCheckInHoje; set => SetProperty(ref _fezCheckInHoje, value); }
    public int SequenciaDias { get => _sequenciaDias; set => SetProperty(ref _sequenciaDias, value); }
    public List<bool> Semana { get => _semana; set => SetProperty(ref _semana, value); }
    public string NomeUsuario { get => _nomeUsuario; set => SetProperty(ref _nomeUsuario, value); }

    public ICommand CheckInCommand { get; }

    public CheckInViewModel(
        LocalDatabase db,
        ICheckInService checkInService,
        LocationService locationService,
        SyncService syncService,
        IUsuarioStorage usuarioStorage)
    {
        _db = db;
        _checkInService = checkInService;
        _locationService = locationService;
        _syncService = syncService;
        _usuarioStorage = usuarioStorage;

        CheckInCommand = new Command(async () => await ExecutarSeOcioso(FazerCheckInAsync), () => !IsLoading && !FezCheckInHoje);
    }

    public async Task InicializarAsync()
    {
        var usuario = _usuarioStorage.Obter();
        if (usuario is not null)
            NomeUsuario = usuario.Nome.Split(' ')[0]; // primeiro nome

        // Faz sync reversa ANTES de carregar o estado — garante dados atualizados após reinstalação
        await _syncService.SincronizarAsync();

        await CarregarEstadoAsync();

        // Sync dos pendentes em background — não bloqueia a UI
        _ = Task.Run(() => _syncService.SincronizarAsync());
    }

    private async Task CarregarEstadoAsync()
    {
        var usuario = _usuarioStorage.Obter();
        if (usuario is null) return;

        FezCheckInHoje = await _db.FezCheckInHojeAsync(usuario.Email);

        var checkInsSemana = await _db.ObterCheckInsDaSemanaAsync(usuario.Email);

        // Usa horário local do dispositivo — compara data local do check-in com data local atual
        var hojeLocal = DateTime.Now.Date;
        var semana = new List<bool>();
        for (int i = 6; i >= 0; i--)
        {
            var diaLocal = hojeLocal.AddDays(-i);
            // Converte cada check-in UTC para horário local antes de comparar a data
            semana.Add(checkInsSemana.Any(c =>
            {
                if (DateTime.TryParse(c.DataHora, out var dtUtc))
                    return dtUtc.ToLocalTime().Date == diaLocal;
                return false;
            }));
        }
        Semana = semana;

        // Sequência de dias consecutivos
        int seq = 0;
        for (int i = 0; i < semana.Count; i++)
        {
            if (semana[semana.Count - 1 - i]) seq++;
            else break;
        }
        SequenciaDias = seq;

        ((Command)CheckInCommand).ChangeCanExecute();
    }

    private async Task FazerCheckInAsync()
    {
        LimparErro();

        try
        {
            var usuario = _usuarioStorage.Obter();
            if (usuario is null) return;

            // 1. Captura localização (melhor esforço)
            var loc = await _locationService.ObterLocalizacaoAsync();

            // 2. Device ID
            var deviceId = DeviceInfo.Current.Name ?? "unknown";

            // 3. Grava localmente PRIMEIRO — offline-first
            var idLocal = Guid.NewGuid().ToString();
            var checkIn = new CheckInLocal
            {
                IdLocal   = idLocal,
                UsuarioId = usuario.Email,
                DataHora  = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Latitude  = loc.Latitude,
                Longitude = loc.Longitude,
                DeviceId  = deviceId,
                Sincronizado = false
            };

            await _db.SalvarCheckInAsync(checkIn);

            // Atualiza UI imediatamente — não espera sync
            FezCheckInHoje = true;
            await CarregarEstadoAsync();

            // 4. Tenta sincronizar com a API em background
            _ = Task.Run(async () =>
            {
                try
                {
                    var request = new RegistrarCheckInRequest(
                        Guid.Parse(idLocal),
                        DateTimeOffset.Parse(checkIn.DataHora),
                        checkIn.Latitude,
                        checkIn.Longitude,
                        checkIn.DeviceId);

                    var sucesso = await _checkInService.RegistrarAsync(request);
                    if (sucesso)
                        await _db.MarcarCheckInSincronizadoAsync(idLocal);
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[CheckIn] API rejeitou check-in {idLocal} — mantendo na fila");
                        await _db.IncrementarTentativaCheckInAsync(idLocal);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CheckInViewModel] Sync background falhou: {ex.Message}");
                    await _db.IncrementarTentativaCheckInAsync(idLocal);
                }
            });

            // 5. Cancela o lembrete push do dia
            CancelarLembrete();

            // 6. Atualiza widgets com novo estado
#if ANDROID
            try
            {
                var ctx = Android.App.Application.Context;
                CheckInWidgetSimples.AtualizarTodos(ctx);
                CheckInWidgetCompleto.AtualizarTodos(ctx);
            }
            catch { }
#endif
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao registrar check-in: {ex.Message}";
        }
    }

    private static void CancelarLembrete()
    {
        try
        {
            LocalNotificationService.CancelarLembrete();
        }
        catch { /* ignora se notificações não estiverem disponíveis */ }
    }
}
