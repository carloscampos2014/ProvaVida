using ProvaVida.Maui.Models;
using ProvaVida.Maui.Storage;

namespace ProvaVida.Maui.Services;

/// <summary>
/// Sincroniza check-ins e heartbeats pendentes no SQLite local com o backend.
/// Chamado ao abrir o app e ao recuperar conectividade.
/// </summary>
public class SyncService
{
    private readonly LocalDatabase _db;
    private readonly ICheckInService _checkInService;
    private readonly IHeartbeatService _heartbeatService;
    private readonly IUsuarioStorage _usuarioStorage;

    public SyncService(
        LocalDatabase db,
        ICheckInService checkInService,
        IHeartbeatService heartbeatService,
        IUsuarioStorage usuarioStorage)
    {
        _db = db;
        _checkInService = checkInService;
        _heartbeatService = heartbeatService;
        _usuarioStorage = usuarioStorage;
    }

    public async Task SincronizarAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

        await SincronizarCheckInsAsync();
        await SincronizarReversaAsync();
        await SincronizarHeartbeatsAsync();
    }

    private async Task SincronizarCheckInsAsync()
    {
        var pendentes = await _db.ObterCheckInsPendentesAsync();

        foreach (var checkIn in pendentes)
        {
            try
            {
                var request = new RegistrarCheckInRequest(
                    Guid.Parse(checkIn.IdLocal),
                    checkIn.DataHora,
                    checkIn.Latitude,
                    checkIn.Longitude,
                    checkIn.DeviceId);

                await _checkInService.RegistrarAsync(request);
                await _db.MarcarCheckInSincronizadoAsync(checkIn.IdLocal);
            }
            catch
            {
                await _db.IncrementarTentativaCheckInAsync(checkIn.IdLocal);
            }
        }
    }

    /// <summary>
    /// Sincronização reversa: busca check-ins dos últimos 30 dias na API
    /// e salva no SQLite local os que ainda não existem.
    /// Resolve o problema de reinstalação do app ou troca de dispositivo.
    /// </summary>
    private async Task SincronizarReversaAsync()
    {
        try
        {
            var usuario = _usuarioStorage.Obter();
            if (usuario is null) return;

            var historico = await _checkInService.ObterHistoricoAsync(
                dataInicio: DateTime.UtcNow.AddDays(-30));

            foreach (var item in historico)
            {
                var idLocal = item.IdLocal.ToString();
                if (await _db.ExisteCheckInAsync(idLocal)) continue;

                // Check-in existe na API mas não no SQLite local — importa
                await _db.SalvarCheckInAsync(new CheckInLocal
                {
                    IdLocal      = idLocal,
                    UsuarioId    = usuario.Email,
                    DataHora     = item.DataHora,
                    Latitude     = item.Latitude,
                    Longitude    = item.Longitude,
                    DeviceId     = item.DeviceId,
                    Sincronizado = true
                });
            }
        }
        catch
        {
            // Falha silenciosa — o app continua funcional com dados locais
        }
    }

    private async Task SincronizarHeartbeatsAsync()
    {
        var pendentes = await _db.ObterHeartbeatsPendentesAsync();

        foreach (var heartbeat in pendentes)
        {
            try
            {
                await _heartbeatService.EnviarAsync();
                await _db.MarcarHeartbeatSincronizadoAsync(heartbeat.IdLocal);
            }
            catch
            {
                // Ignora falha de heartbeat — próxima sync tentará novamente
            }
        }
    }
}
