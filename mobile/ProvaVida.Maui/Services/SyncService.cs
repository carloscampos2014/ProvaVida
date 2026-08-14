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

    public SyncService(
        LocalDatabase db,
        ICheckInService checkInService,
        IHeartbeatService heartbeatService)
    {
        _db = db;
        _checkInService = checkInService;
        _heartbeatService = heartbeatService;
    }

    public async Task SincronizarAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

        await SincronizarCheckInsAsync();
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
