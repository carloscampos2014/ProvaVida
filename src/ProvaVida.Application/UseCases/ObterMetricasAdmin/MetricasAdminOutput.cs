namespace ProvaVida.Application.UseCases.ObterMetricasAdmin;

public class MetricasAdminOutput
{
    // Usuários
    public int TotalUsuariosAtivos { get; init; }
    public int NovoUsuariosUltimos7Dias { get; init; }

    // Check-in
    public int UsuariosComCheckInHoje { get; init; }
    public int UsuariosComCheckInAtrasado { get; init; }  // +2 dias sem check-in

    // Conectividade
    public int UsuariosPossivelmnteSemInternet { get; init; }  // heartbeat recente mas sem check-in há +1 dia

    // Notificações — hoje
    public int AvisosEnviadosAoUsuarioHoje { get; init; }      // status aguardando_resposta
    public int AlertasDisparadosAoContatoHoje { get; init; }   // status disparado
    public int AlertasCanceladosHoje { get; init; }            // status cancelado (falso positivo evitado)

    // Notificações — histórico total
    public int TotalAlertasDisparadosHistorico { get; init; }

    // Eventos paginados
    public IEnumerable<EventoNotificacaoDto> Eventos { get; init; } = [];
    public int TotalEventos { get; init; }
    public int PaginaAtual { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalPaginas => TamanhoPagina > 0 ? (int)Math.Ceiling((double)TotalEventos / TamanhoPagina) : 0;

    public DateTime GeradoEm { get; init; } = DateTime.UtcNow;
}
