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
    Task<int> ContarTotalEventosAsync(CancellationToken ct = default);
    Task<IEnumerable<EventosNotificacaoRow>> ListarEventosAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
}

public record EventosNotificacaoRow(
    Guid Id,
    string NomeUsuario,
    string Status,
    string Canal,
    DateTime DataDisparo,
    DateTime? JanelaExpiraEm
);
