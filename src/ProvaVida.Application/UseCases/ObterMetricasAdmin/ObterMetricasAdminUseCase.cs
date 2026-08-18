using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.ObterMetricasAdmin;

public class ObterMetricasAdminUseCase
{
    private readonly IAdminMetricasRepository _repo;

    public ObterMetricasAdminUseCase(IAdminMetricasRepository repo)
    {
        _repo = repo;
    }

    public async Task<MetricasAdminOutput> ExecutarAsync(CancellationToken ct = default)
    {
        var tasks = await Task.WhenAll(
            _repo.ContarUsuariosAtivosAsync(ct),
            _repo.ContarNovosUsuariosAsync(7, ct),
            _repo.ContarUsuariosComCheckInHojeAsync(ct),
            _repo.ContarUsuariosComCheckInAtrasadoAsync(2, ct),
            _repo.ContarUsuariosPossivelmnteSemInternetAsync(ct),
            _repo.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.AguardandoResposta, ct),
            _repo.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Disparado, ct),
            _repo.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Cancelado, ct),
            _repo.ContarTotalNotificacoesPorStatusAsync(NotificacaoEmergencia.Statuses.Disparado, ct)
        );

        return new MetricasAdminOutput
        {
            TotalUsuariosAtivos                = tasks[0],
            NovoUsuariosUltimos7Dias           = tasks[1],
            UsuariosComCheckInHoje             = tasks[2],
            UsuariosComCheckInAtrasado         = tasks[3],
            UsuariosPossivelmnteSemInternet    = tasks[4],
            AvisosEnviadosAoUsuarioHoje        = tasks[5],
            AlertasDisparadosAoContatoHoje     = tasks[6],
            AlertasCanceladosHoje              = tasks[7],
            TotalAlertasDisparadosHistorico    = tasks[8],
            GeradoEm                           = DateTime.UtcNow
        };
    }
}
