using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.ObterMetricasAdmin;

public class ObterMetricasAdminUseCase
{
    private readonly IAdminMetricasRepository _repo;
    public const int TamanhoPaginaDefault = 20;

    public ObterMetricasAdminUseCase(IAdminMetricasRepository repo)
    {
        _repo = repo;
    }

    public async Task<MetricasAdminOutput> ExecutarAsync(int pagina = 1, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;

        var contadores = await Task.WhenAll(
            _repo.ContarUsuariosAtivosAsync(ct),
            _repo.ContarNovosUsuariosAsync(7, ct),
            _repo.ContarUsuariosComCheckInHojeAsync(ct),
            _repo.ContarUsuariosComCheckInAtrasadoAsync(2, ct),
            _repo.ContarUsuariosPossivelmnteSemInternetAsync(ct),
            _repo.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.AguardandoResposta, ct),
            _repo.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Disparado, ct),
            _repo.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Cancelado, ct),
            _repo.ContarTotalNotificacoesPorStatusAsync(NotificacaoEmergencia.Statuses.Disparado, ct),
            _repo.ContarTotalEventosAsync(ct)
        );

        var totalEventos = contadores[9];
        var totalPaginas = (int)Math.Ceiling((double)totalEventos / TamanhoPaginaDefault);
        pagina = pagina > totalPaginas && totalPaginas > 0 ? totalPaginas : pagina;

        var eventos = await _repo.ListarEventosAsync(pagina, TamanhoPaginaDefault, ct);

        return new MetricasAdminOutput
        {
            TotalUsuariosAtivos             = contadores[0],
            NovoUsuariosUltimos7Dias        = contadores[1],
            UsuariosComCheckInHoje          = contadores[2],
            UsuariosComCheckInAtrasado      = contadores[3],
            UsuariosPossivelmnteSemInternet = contadores[4],
            AvisosEnviadosAoUsuarioHoje     = contadores[5],
            AlertasDisparadosAoContatoHoje  = contadores[6],
            AlertasCanceladosHoje           = contadores[7],
            TotalAlertasDisparadosHistorico = contadores[8],
            TotalEventos                    = totalEventos,
            PaginaAtual                     = pagina,
            TamanhoPagina                   = TamanhoPaginaDefault,
            Eventos                         = eventos.Select(e => new EventoNotificacaoDto
            {
                Id            = e.Id,
                NomeUsuario   = e.NomeUsuario,
                Status        = e.Status,
                Canal         = e.Canal,
                DataDisparo   = e.DataDisparo,
                JanelaExpiraEm = e.JanelaExpiraEm
            }),
            GeradoEm = DateTime.UtcNow
        };
    }
}
