namespace ProvaVida.Application.Interfaces;

public interface IAdminMetricasRepository
{
    Task<int> ContarUsuariosAtivosAsync(CancellationToken ct = default);
    Task<int> ContarNovosUsuariosAsync(int ultimosDias, CancellationToken ct = default);
    Task<int> ContarUsuariosComCheckInHojeAsync(CancellationToken ct = default);
    Task<int> ContarUsuariosComCheckInAtrasadoAsync(int diasSemCheckIn, CancellationToken ct = default);
    Task<int> ContarUsuariosPossivelmnteSemInternetAsync(CancellationToken ct = default);
    Task<int> ContarNotificacoesPorStatusHojeAsync(string status, CancellationToken ct = default);
    Task<int> ContarTotalNotificacoesPorStatusAsync(string status, CancellationToken ct = default);
}
