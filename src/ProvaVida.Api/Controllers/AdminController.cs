using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.UseCases.ObterMetricasAdmin;
using ProvaVida.Application.UseCases.TestarNotificacao;

namespace ProvaVida.Api.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    [HttpPost("testar-email")]
    [ProducesResponseType(typeof(TesteNotificacaoOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestarEmail(
        [FromBody] TesteDestinatarioRequest request,
        [FromServices] TestarNotificacaoUseCase useCase,
        CancellationToken ct)
    {
        var resultado = await useCase.TestarEmailAsync(request.Destinatario, ct);
        return Ok(resultado);
    }

    [HttpPost("testar-whatsapp")]
    [ProducesResponseType(typeof(TesteNotificacaoOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestarWhatsApp(
        [FromBody] TesteDestinatarioRequest request,
        [FromServices] TestarNotificacaoUseCase useCase,
        CancellationToken ct)
    {
        var resultado = await useCase.TestarWhatsAppAsync(request.Destinatario, ct);
        return Ok(resultado);
    }

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
        var geradoEm       = m.GeradoEm.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
        var paginaAnterior = m.PaginaAtual > 1 ? m.PaginaAtual - 1 : 1;
        var paginaProxima  = m.PaginaAtual < m.TotalPaginas ? m.PaginaAtual + 1 : m.TotalPaginas;

        static string IconeStatus(string status) => status switch
        {
            "disparado"           => "🔴",
            "aguardando_resposta" => "⏳",
            "cancelado"           => "✅",
            "heartbeat_ativo"     => "💚",
            _                     => "⚪"
        };

        static string IconeCanal(string canal) => canal switch
        {
            "email+whatsapp" => "✉️+📱",
            "email"          => "✉️",
            "whatsapp"       => "📱",
            "email_usuario"  => "✉️ usr",
            "falha"          => "❌",
            _                => "—"
        };

        var linhasTabela = string.Join("\n", m.Eventos.Select(e =>
            $"""
                    <tr>
                        <td>{System.Net.WebUtility.HtmlEncode(e.NomeUsuario)}</td>
                        <td>{IconeStatus(e.Status)} {e.Status.Replace("_", " ")}</td>
                        <td>{IconeCanal(e.Canal)}</td>
                        <td>{e.DataDisparo.ToString("dd/MM HH:mm")} UTC</td>
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
                        padding: 20px 24px;
                    }
                    header {
                        display: flex;
                        align-items: center;
                        gap: 12px;
                        margin-bottom: 20px;
                        flex-wrap: wrap;
                    }
                    header h1 { font-size: 20px; font-weight: 700; color: #774CCC; }
                    .header-right {
                        margin-left: auto;
                        display: flex;
                        align-items: center;
                        gap: 10px;
                        flex-wrap: wrap;
                    }
                    .gerado-em { font-size: 11px; color: #6E648B; }
                    .countdown {
                        font-size: 12px;
                        font-weight: 600;
                        color: #774CCC;
                        background: #EFE9FB;
                        padding: 5px 10px;
                        border-radius: 6px;
                        min-width: 100px;
                        text-align: center;
                    }
                    .countdown.urgente { color: #E73C3C; background: #FEE5E5; }
                    .btn {
                        display: inline-flex;
                        align-items: center;
                        gap: 5px;
                        padding: 6px 14px;
                        background: #774CCC;
                        color: #fff;
                        border-radius: 7px;
                        text-decoration: none;
                        font-size: 12px;
                        font-weight: 600;
                        cursor: pointer;
                        border: none;
                    }
                    .btn:hover { background: #57329F; }
                    .btn.sec { background: #EFE9FB; color: #774CCC; }
                    .btn.sec:hover { background: #DFD8F7; }
                    .btn[style*="opacity"] { pointer-events: none; }
                    h2 {
                        font-size: 11px;
                        font-weight: 700;
                        text-transform: uppercase;
                        letter-spacing: 0.08em;
                        color: #6E648B;
                        margin: 16px 0 8px;
                    }
                    .grid {
                        display: grid;
                        grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
                        gap: 10px;
                    }
                    .grid-diag {
                        display: grid;
                        grid-template-columns: 160px 240px 240px;
                        gap: 10px;
                    }
                    .card {
                        background: #fff;
                        border-radius: 12px;
                        padding: 14px 16px;
                        box-shadow: 0 2px 8px rgba(0,0,0,0.06);
                    }
                    .card .label { font-size: 11px; color: #6E648B; margin-bottom: 6px; }
                    .card .value { font-size: 28px; font-weight: 700; color: #774CCC; line-height: 1; }
                    .card.danger .value  { color: #E73C3C; }
                    .card.success .value { color: #2E9E6B; }
                    .card.warning .value { color: #D97706; }
                    .card.neutral .value { color: #1E1930; }
                    .card input {
                        width: 100%;
                        padding: 6px 10px;
                        border: 1px solid #E4E0F2;
                        border-radius: 6px;
                        font-size: 12px;
                        margin-bottom: 8px;
                        outline: none;
                    }
                    .card input:focus { border-color: #774CCC; }
                    .resultado { margin-top: 8px; font-size: 12px; min-height: 18px; }
                    .tabela-wrap {
                        background: #fff;
                        border-radius: 12px;
                        box-shadow: 0 2px 8px rgba(0,0,0,0.06);
                        overflow: hidden;
                        margin-top: 8px;
                    }
                    table { width: 100%; border-collapse: collapse; font-size: 12px; }
                    thead th {
                        background: #774CCC;
                        color: #fff;
                        padding: 8px 12px;
                        text-align: left;
                        font-weight: 600;
                        font-size: 11px;
                        text-transform: uppercase;
                        letter-spacing: 0.04em;
                    }
                    tbody tr:nth-child(even) { background: #F8F7FD; }
                    tbody tr:hover { background: #EFE9FB; }
                    tbody td { padding: 7px 12px; }
                    .paginacao {
                        display: flex;
                        align-items: center;
                        justify-content: space-between;
                        padding: 10px 14px;
                        border-top: 1px solid #E4E0F2;
                        font-size: 12px;
                        color: #6E648B;
                    }
                    .paginacao-nav { display: flex; gap: 6px; }
                </style>
            </head>
            <body>
                <header>
                    <h1>ProvaVida — Painel Admin</h1>
                    <div class="header-right">
                        <span class="gerado-em">{{geradoEm}}</span>
                        <span class="countdown" id="countdown">Atualizando em 60s</span>
                        <button class="btn" onclick="location.reload()">↻ Atualizar</button>
                    </div>
                </header>

                <h2>Usuários</h2>
                <div class="grid">
                    <div class="card neutral">
                        <div class="label">Total ativos</div>
                        <div class="value">{{m.TotalUsuariosAtivos}}</div>
                    </div>
                    <div class="card success">
                        <div class="label">Novos (7 dias)</div>
                        <div class="value">{{m.NovoUsuariosUltimos7Dias}}</div>
                    </div>
                    <div class="card success">
                        <div class="label">Check-in hoje</div>
                        <div class="value">{{m.UsuariosComCheckInHoje}}</div>
                    </div>
                    <div class="card danger">
                        <div class="label">Atrasado (+2d)</div>
                        <div class="value">{{m.UsuariosComCheckInAtrasado}}</div>
                    </div>
                    <div class="card warning">
                        <div class="label">Sem internet</div>
                        <div class="value">{{m.UsuariosPossivelmnteSemInternet}}</div>
                    </div>
                </div>

                <h2>Notificações — Hoje</h2>
                <div class="grid">
                    <div class="card warning">
                        <div class="label">Avisos ao usuário</div>
                        <div class="value">{{m.AvisosEnviadosAoUsuarioHoje}}</div>
                    </div>
                    <div class="card danger">
                        <div class="label">Alertas ao contato</div>
                        <div class="value">{{m.AlertasDisparadosAoContatoHoje}}</div>
                    </div>
                    <div class="card success">
                        <div class="label">Falsos positivos evitados</div>
                        <div class="value">{{m.AlertasCanceladosHoje}}</div>
                    </div>
                </div>

                <h2>Histórico Total e Diagnóstico</h2>
                <div class="grid-diag">
                    <div class="card neutral">
                        <div class="label">Alertas disparados (total)</div>
                        <div class="value">{{m.TotalAlertasDisparadosHistorico}}</div>
                    </div>
                    <div class="card">
                        <div class="label">✉️ Teste de E-mail</div>
                        <input id="email-dest" type="email" placeholder="email@exemplo.com"/>
                        <button class="btn" onclick="testar('email')">Enviar teste</button>
                        <div class="resultado" id="email-res"></div>
                    </div>
                    <div class="card">
                        <div class="label">📱 Teste de WhatsApp</div>
                        <input id="wapp-dest" type="tel" placeholder="5511999999999"/>
                        <button class="btn" onclick="testar('whatsapp')">Enviar teste</button>
                        <div class="resultado" id="wapp-res"></div>
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
                            <a href="/admin?pagina=1" class="btn sec" {{(m.PaginaAtual <= 1 ? "style=\"opacity:0.4\"" : "")}}>«</a>
                            <a href="/admin?pagina={{paginaAnterior}}" class="btn sec" {{(m.PaginaAtual <= 1 ? "style=\"opacity:0.4\"" : "")}}>‹ Anterior</a>
                            <a href="/admin?pagina={{paginaProxima}}" class="btn sec" {{(m.PaginaAtual >= m.TotalPaginas ? "style=\"opacity:0.4\"" : "")}}>Próxima ›</a>
                            <a href="/admin?pagina={{m.TotalPaginas}}" class="btn sec" {{(m.PaginaAtual >= m.TotalPaginas ? "style=\"opacity:0.4\"" : "")}}>»</a>
                        </div>
                    </div>
                </div>

                <script>
                    function testar(tipo) {
                        var dest = document.getElementById(tipo === 'email' ? 'email-dest' : 'wapp-dest').value.trim();
                        var res  = document.getElementById(tipo === 'email' ? 'email-res'  : 'wapp-res');
                        if (!dest) { res.innerHTML = '<span style="color:#E73C3C">⚠️ Informe o destinatário.</span>'; return; }
                        res.innerHTML = '<span style="color:#6E648B">⏳ Enviando...</span>';
                        fetch('/admin/testar-' + tipo, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ destinatario: dest })
                        })
                        .then(r => r.json())
                        .then(d => {
                            res.innerHTML = d.sucesso
                                ? '<span style="color:#2E9E6B">✅ ' + d.mensagem + ' (' + d.duracaoMs + 'ms)</span>'
                                : '<span style="color:#E73C3C">❌ ' + d.mensagem + ' (' + d.duracaoMs + 'ms)</span>';
                        })
                        .catch(e => { res.innerHTML = '<span style="color:#E73C3C">❌ ' + e.message + '</span>'; });
                    }

                    (function () {
                        var INTERVALO = 60, restante = INTERVALO, pagina = {{m.PaginaAtual}};
                        var el = document.getElementById('countdown');
                        function tick() {
                            el.textContent = 'Atualizando em ' + restante + 's';
                            restante <= 10 ? el.classList.add('urgente') : el.classList.remove('urgente');
                            if (restante-- <= 0) { window.location.href = '/admin?pagina=' + pagina; return; }
                            setTimeout(tick, 1000);
                        }
                        tick();
                    })();
                </script>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }
}

public record TesteDestinatarioRequest(string Destinatario);
