using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.UseCases.ObterMetricasAdmin;

namespace ProvaVida.Api.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    [HttpGet("metricas")]
    [ProducesResponseType(typeof(MetricasAdminOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> Metricas(
        [FromQuery] int pagina = 1,
        [FromServices] ObterMetricasAdminUseCase useCase = null!,
        CancellationToken ct = default)
    {
        var resultado = await useCase.ExecutarAsync(pagina, ct);
        return Ok(resultado);
    }

    [HttpGet]
    [Produces("text/html")]
    public async Task<ContentResult> Painel(
        [FromQuery] int pagina = 1,
        [FromServices] ObterMetricasAdminUseCase useCase = null!,
        CancellationToken ct = default)
    {
        var m = await useCase.ExecutarAsync(pagina, ct);
        var geradoEm = m.GeradoEm.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
        var paginaAnterior = m.PaginaAtual > 1 ? m.PaginaAtual - 1 : 1;
        var paginaProxima  = m.PaginaAtual < m.TotalPaginas ? m.PaginaAtual + 1 : m.TotalPaginas;

        static string IconeStatus(string status) => status switch
        {
            "disparado"          => "🔴",
            "aguardando_resposta" => "⏳",
            "cancelado"          => "✅",
            "heartbeat_ativo"    => "💚",
            _                    => "⚪"
        };

        static string IconeCanal(string canal) => canal switch
        {
            "email+whatsapp" => "✉️ + 📱",
            "email"          => "✉️",
            "whatsapp"       => "📱",
            "email_usuario"  => "✉️ usuário",
            "falha"          => "❌",
            _                => "—"
        };

        var linhasTabela = string.Join("\n", m.Eventos.Select(e =>
            $"""
                    <tr>
                        <td>{System.Net.WebUtility.HtmlEncode(e.NomeUsuario)}</td>
                        <td>{IconeStatus(e.Status)} {e.Status.Replace("_", " ")}</td>
                        <td>{IconeCanal(e.Canal)}</td>
                        <td>{e.DataDisparo.ToString("dd/MM/yyyy HH:mm")} UTC</td>
                        <td>{(e.JanelaExpiraEm.HasValue ? e.JanelaExpiraEm.Value.ToString("dd/MM HH:mm") : "—")}</td>
                    </tr>
            """));

        var html = $$"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
                <meta charset="UTF-8"/>
                <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
                <title>ProvaVida — Admin</title>
                <style>
                    * { box-sizing: border-box; margin: 0; padding: 0; }
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                        background: #F8F7FD;
                        color: #1E1930;
                        padding: 32px 24px;
                    }
                    header {
                        display: flex;
                        align-items: center;
                        gap: 16px;
                        margin-bottom: 32px;
                        flex-wrap: wrap;
                    }
                    header h1 { font-size: 24px; font-weight: 700; color: #774CCC; }
                    .header-right {
                        margin-left: auto;
                        display: flex;
                        align-items: center;
                        gap: 12px;
                        flex-wrap: wrap;
                    }
                    .gerado-em { font-size: 12px; color: #6E648B; }
                    .countdown {
                        font-size: 13px;
                        font-weight: 600;
                        color: #774CCC;
                        background: #EFE9FB;
                        padding: 6px 12px;
                        border-radius: 8px;
                        min-width: 110px;
                        text-align: center;
                    }
                    .countdown.urgente { color: #E73C3C; background: #FEE5E5; }
                    .btn {
                        display: inline-flex;
                        align-items: center;
                        gap: 6px;
                        padding: 8px 16px;
                        background: #774CCC;
                        color: #fff;
                        border-radius: 8px;
                        text-decoration: none;
                        font-size: 13px;
                        font-weight: 600;
                        cursor: pointer;
                        border: none;
                    }
                    .btn:hover { background: #57329F; }
                    .btn.secundario {
                        background: #EFE9FB;
                        color: #774CCC;
                    }
                    .btn.secundario:hover { background: #DFD8F7; }
                    .btn:disabled, .btn[disabled] {
                        opacity: 0.4;
                        pointer-events: none;
                    }
                    h2 {
                        font-size: 13px;
                        font-weight: 600;
                        text-transform: uppercase;
                        letter-spacing: 0.08em;
                        color: #6E648B;
                        margin: 28px 0 12px;
                    }
                    .grid {
                        display: grid;
                        grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
                        gap: 16px;
                    }
                    .card {
                        background: #fff;
                        border-radius: 16px;
                        padding: 20px;
                        box-shadow: 0 4px 16px rgba(0,0,0,0.06);
                    }
                    .card .label { font-size: 12px; color: #6E648B; margin-bottom: 8px; }
                    .card .value { font-size: 36px; font-weight: 700; color: #774CCC; line-height: 1; }
                    .card.danger .value  { color: #E73C3C; }
                    .card.success .value { color: #2E9E6B; }
                    .card.warning .value { color: #D97706; }
                    .card.neutral .value { color: #1E1930; }
                    .tabela-wrap {
                        background: #fff;
                        border-radius: 16px;
                        box-shadow: 0 4px 16px rgba(0,0,0,0.06);
                        overflow: hidden;
                    }
                    table {
                        width: 100%;
                        border-collapse: collapse;
                        font-size: 13px;
                    }
                    thead th {
                        background: #774CCC;
                        color: #fff;
                        padding: 12px 16px;
                        text-align: left;
                        font-weight: 600;
                        font-size: 12px;
                        text-transform: uppercase;
                        letter-spacing: 0.05em;
                    }
                    tbody tr:nth-child(even) { background: #F8F7FD; }
                    tbody tr:hover { background: #EFE9FB; }
                    tbody td { padding: 11px 16px; color: #1E1930; }
                    .paginacao {
                        display: flex;
                        align-items: center;
                        justify-content: space-between;
                        padding: 16px 20px;
                        border-top: 1px solid #E4E0F2;
                        background: #fff;
                        border-radius: 0 0 16px 16px;
                        font-size: 13px;
                        color: #6E648B;
                    }
                    .paginacao-nav { display: flex; gap: 8px; align-items: center; }
                </style>
            </head>
            <body>
                <header>
                    <h1>ProvaVida — Painel Admin</h1>
                    <div class="header-right">
                        <span class="gerado-em">Gerado em {{geradoEm}}</span>
                        <span class="countdown" id="countdown">Atualizando em 60s</span>
                        <button class="btn" onclick="location.reload()">↻ Atualizar agora</button>
                    </div>
                </header>

                <h2>Usuários</h2>
                <div class="grid">
                    <div class="card neutral">
                        <div class="label">Total ativos</div>
                        <div class="value">{{m.TotalUsuariosAtivos}}</div>
                    </div>
                    <div class="card success">
                        <div class="label">Novos (últimos 7 dias)</div>
                        <div class="value">{{m.NovoUsuariosUltimos7Dias}}</div>
                    </div>
                    <div class="card success">
                        <div class="label">Check-in hoje</div>
                        <div class="value">{{m.UsuariosComCheckInHoje}}</div>
                    </div>
                    <div class="card danger">
                        <div class="label">Check-in atrasado (+2 dias)</div>
                        <div class="value">{{m.UsuariosComCheckInAtrasado}}</div>
                    </div>
                    <div class="card warning">
                        <div class="label">Possivelmente sem internet</div>
                        <div class="value">{{m.UsuariosPossivelmnteSemInternet}}</div>
                    </div>
                </div>

                <h2>Notificações — Hoje</h2>
                <div class="grid">
                    <div class="card warning">
                        <div class="label">Avisos enviados ao usuário</div>
                        <div class="value">{{m.AvisosEnviadosAoUsuarioHoje}}</div>
                    </div>
                    <div class="card danger">
                        <div class="label">Alertas disparados ao contato</div>
                        <div class="value">{{m.AlertasDisparadosAoContatoHoje}}</div>
                    </div>
                    <div class="card success">
                        <div class="label">Falsos positivos evitados</div>
                        <div class="value">{{m.AlertasCanceladosHoje}}</div>
                    </div>
                </div>

                <h2>Histórico Total</h2>
                <div class="grid">
                    <div class="card neutral">
                        <div class="label">Alertas disparados (total)</div>
                        <div class="value">{{m.TotalAlertasDisparadosHistorico}}</div>
                    </div>
                </div>

                <h2>Eventos de Notificação — {{m.TotalEventos}} registros</h2>
                <div class="tabela-wrap">
                    <table>
                        <thead>
                            <tr>
                                <th>Usuário</th>
                                <th>Status</th>
                                <th>Canal</th>
                                <th>Data/Hora</th>
                                <th>Janela expira</th>
                            </tr>
                        </thead>
                        <tbody>
                            {{linhasTabela}}
                        </tbody>
                    </table>
                    <div class="paginacao">
                        <span>Página <strong>{{m.PaginaAtual}}</strong> de <strong>{{m.TotalPaginas}}</strong></span>
                        <div class="paginacao-nav">
                            <a href="/admin?pagina=1" class="btn secundario" {{(m.PaginaAtual <= 1 ? "style=\"opacity:0.4;pointer-events:none\"" : "")}}>« Primeira</a>
                            <a href="/admin?pagina={{paginaAnterior}}" class="btn secundario" {{(m.PaginaAtual <= 1 ? "style=\"opacity:0.4;pointer-events:none\"" : "")}}>‹ Anterior</a>
                            <a href="/admin?pagina={{paginaProxima}}" class="btn secundario" {{(m.PaginaAtual >= m.TotalPaginas ? "style=\"opacity:0.4;pointer-events:none\"" : "")}}>Próxima ›</a>
                            <a href="/admin?pagina={{m.TotalPaginas}}" class="btn secundario" {{(m.PaginaAtual >= m.TotalPaginas ? "style=\"opacity:0.4;pointer-events:none\"" : "")}}>Última »</a>
                        </div>
                    </div>
                </div>

                <script>
                    (function () {
                        var INTERVALO = 60;
                        var restante = INTERVALO;
                        var paginaAtual = {{m.PaginaAtual}};
                        var el = document.getElementById('countdown');

                        function atualizar() {
                            el.textContent = 'Atualizando em ' + restante + 's';
                            if (restante <= 10) {
                                el.classList.add('urgente');
                            } else {
                                el.classList.remove('urgente');
                            }
                            if (restante <= 0) {
                                window.location.href = '/admin?pagina=' + paginaAtual;
                                return;
                            }
                            restante--;
                            setTimeout(atualizar, 1000);
                        }

                        atualizar();
                    })();
                </script>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }
}
