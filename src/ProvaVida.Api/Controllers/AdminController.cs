using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.ObterMetricasAdmin;
using ProvaVida.Application.UseCases.TestarNotificacao;
using ProvaVida.Infrastructure.Jobs;
namespace ProvaVida.Api.Controllers;

[ApiController]
[Route("admin")]
[Authorize(AuthenticationSchemes = "BasicAuth")]
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

    [HttpPost("testar-sms")]
    [ProducesResponseType(typeof(TesteNotificacaoOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestarSms(
        [FromBody] TesteDestinatarioRequest request,
        [FromServices] TestarNotificacaoUseCase useCase,
        CancellationToken ct)
    {
        var resultado = await useCase.TestarSmsAsync(request.Destinatario, ct);
        return Ok(resultado);
    }

    [HttpPost("testar-voz")]
    [ProducesResponseType(typeof(TesteNotificacaoOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestarVoz(
        [FromBody] TesteDestinatarioRequest request,
        [FromServices] TestarNotificacaoUseCase useCase,
        CancellationToken ct)
    {
        var resultado = await useCase.TestarVozAsync(request.Destinatario, ct);
        return Ok(resultado);
    }

    [HttpGet("checkins")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarCheckIns(
        [FromQuery] int pagina = 1,
        [FromServices] ICheckInRepository checkInRepo = null!,
        CancellationToken ct = default)
    {
        // Busca os últimos 50 check-ins de todos os usuários
        var checkIns = await checkInRepo.ListarTodosAsync(pagina, 50, ct);
        return Ok(checkIns);
    }

    [HttpGet("ips-bloqueados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> IpsBloqueados(
        [FromServices] IBruteForceService bruteForce,
        CancellationToken ct)
    {
        var lista = await bruteForce.ListarBloqueadosAsync(ct);
        return Ok(lista);
    }

    [HttpPost("ips-bloqueados/{ip}/liberar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LiberarIp(
        string ip,
        [FromServices] IBruteForceService bruteForce,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return BadRequest(new { error = "IP inválido." });

        var liberadoPor = User.Identity?.Name ?? "admin";
        await bruteForce.LiberarAsync(ip, liberadoPor, ct);
        return NoContent();
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

    [HttpPost("backup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> FazerBackup(
        [FromServices] IConfiguration configuration,
        CancellationToken ct)
    {
        var cs = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        var (host, port, database, username, password) = BackupDatabaseJob.ParseConnectionString(cs);

        var nomeArquivo = $"provavida-{DateTime.UtcNow:yyyyMMdd-HHmmss}.sql";
        var tempPath    = Path.Combine(Path.GetTempPath(), nomeArquivo);

        try
        {
            await BackupDatabaseJob.ExecutarPgDumpAsync(host, port, database, username, password, tempPath);
            var bytes = await System.IO.File.ReadAllBytesAsync(tempPath, ct);
            return File(bytes, "application/octet-stream", nomeArquivo);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
        }
    }

    [HttpPost("restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestaurarBackup(
        IFormFile arquivo,
        [FromServices] IConfiguration configuration,
        CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new { error = "Arquivo SQL inválido ou vazio." });

        if (!arquivo.FileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Apenas arquivos .sql são aceitos." });

        var cs = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        var (host, port, database, username, password) = BackupDatabaseJob.ParseConnectionString(cs);

        var tempPath = Path.Combine(Path.GetTempPath(), $"restore-{Guid.NewGuid()}.sql");
        try
        {
            await using (var stream = System.IO.File.Create(tempPath))
                await arquivo.CopyToAsync(stream, ct);

            await BackupDatabaseJob.ExecutarPsqlRestoreAsync(host, port, database, username, password, tempPath);
            return Ok(new { message = "Restore concluído com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        finally
        {
            if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
        }
    }

    [HttpGet("backups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ListarBackups()
    {
        var arquivos = BackupDatabaseJob.ListarBackups()
            .Select(f => new
            {
                nome        = f.Name,
                tamanho     = f.Length,
                criadoEm    = f.LastWriteTimeUtc.ToString("dd/MM/yyyy HH:mm") + " UTC"
            });
        return Ok(arquivos);
    }

    [HttpGet]
    [Produces("text/html")]
    public async Task<ContentResult> Painel(
        [FromQuery] int pagina = 1,
        [FromServices] ObterMetricasAdminUseCase useCase = null!,
        [FromServices] IBruteForceService bruteForce = null!,
        [FromServices] ICheckInRepository checkInRepo = null!,
        CancellationToken ct = default)
    {
        var m = await useCase.ExecutarAsync(pagina, ct);
        var ipsBloqueados = await bruteForce.ListarBloqueadosAsync(ct);
        var checkIns = await checkInRepo.ListarTodosAsync(1, 50, ct);
        var geradoEm       = m.GeradoEm.ToString("dd/MM/yyyy HH:mm:ss") + " UTC";
        var paginaAnterior = m.PaginaAtual > 1 ? m.PaginaAtual - 1 : 1;
        var paginaProxima  = m.PaginaAtual < m.TotalPaginas ? m.PaginaAtual + 1 : m.TotalPaginas;
        var urlBase        = "/admin?pagina=";

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

        var linhasIpsBloqueados = string.Join("\n", ipsBloqueados.Select(b =>
            $"""
                    <tr>
                        <td><code>{System.Net.WebUtility.HtmlEncode(b.Ip)}</code></td>
                        <td>{System.Net.WebUtility.HtmlEncode(b.Motivo)}</td>
                        <td>{b.TotalTentativas}</td>
                        <td>{b.BloqueadoEm:dd/MM HH:mm} UTC</td>
                        <td>{b.ExpiraEm:dd/MM HH:mm} UTC</td>
                        <td>
                            <button class="btn" style="background:#E73C3C;font-size:11px;padding:4px 10px"
                                onclick="liberarIp('{System.Net.WebUtility.HtmlEncode(b.Ip)}', this)">
                                🔓 Liberar
                            </button>
                        </td>
                    </tr>
            """));

        var ipsBloqueadosCount = ipsBloqueados.Count();

        var checkInsRows = checkIns.ToList();
        var linhasCheckIns = string.Join("\n", checkInsRows.Select(c =>
        {
            var row = (IDictionary<string, object>)c;
            var email = System.Net.WebUtility.HtmlEncode(row["usuario_email"]?.ToString() ?? "");
            var nome  = System.Net.WebUtility.HtmlEncode(row["usuario_nome"]?.ToString() ?? "");
            var dataHora = row["data_hora"]?.ToString() ?? "";
            if (DateTimeOffset.TryParse(dataHora, out var dt))
                dataHora = dt.ToString("dd/MM/yyyy HH:mm") + " UTC";
            var device = System.Net.WebUtility.HtmlEncode(row["device_id"]?.ToString() ?? "");
            return $"""
                    <tr>
                        <td>{nome}</td>
                        <td>{email}</td>
                        <td>{dataHora}</td>
                        <td>{device}</td>
                    </tr>
            """;
        }));
        var checkInsCount = checkInsRows.Count;

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
                <div style="display:grid;grid-template-columns:160px 1fr 1fr 1fr 1fr;gap:10px;">
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
                    <div class="card">
                        <div class="label">💬 Teste de SMS</div>
                        <input id="sms-dest" type="tel" placeholder="5511999999999"/>
                        <button class="btn" onclick="testar('sms')">Enviar teste</button>
                        <div class="resultado" id="sms-res"></div>
                    </div>
                    <div class="card">
                        <div class="label">📞 Teste de Voz</div>
                        <input id="voz-dest" type="tel" placeholder="5511999999999"/>
                        <button class="btn" onclick="testar('voz')">Ligar</button>
                        <div class="resultado" id="voz-res"></div>
                    </div>
                </div>

                <h2>IPs Bloqueados — {{ipsBloqueadosCount}} ativo(s)</h2>
                <div class="tabela-wrap">
                    <table>
                        <thead>
                            <tr>
                                <th>IP</th>
                                <th>Motivo</th>
                                <th>Tentativas (24h)</th>
                                <th>Bloqueado em</th>
                                <th>Expira em</th>
                                <th>Ação</th>
                            </tr>
                        </thead>
                        <tbody id="tabela-ips">
                            {{(string.IsNullOrEmpty(linhasIpsBloqueados) ? "<tr><td colspan=\"6\" style=\"text-align:center;color:#6E648B;padding:14px\">✅ Nenhum IP bloqueado no momento</td></tr>" : linhasIpsBloqueados)}}
                        </tbody>
                    </table>
                </div>

                <h2>Check-ins Registrados — {{checkInsCount}} mais recentes</h2>
                <div class="tabela-wrap">
                    <table>
                        <thead>
                            <tr>
                                <th>Usuário</th>
                                <th>E-mail</th>
                                <th>Data/Hora</th>
                                <th>Dispositivo</th>
                            </tr>
                        </thead>
                        <tbody>
                            {{(string.IsNullOrEmpty(linhasCheckIns) ? "<tr><td colspan=\"4\" style=\"text-align:center;color:#6E648B;padding:14px\">Nenhum check-in registrado</td></tr>" : linhasCheckIns)}}
                        </tbody>
                    </table>
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
                            <a href="{{urlBase}}1" class="btn sec" {{(m.PaginaAtual <= 1 ? "style=\"opacity:0.4\"" : "")}}>«</a>
                            <a href="{{urlBase}}{{paginaAnterior}}" class="btn sec" {{(m.PaginaAtual <= 1 ? "style=\"opacity:0.4\"" : "")}}>‹ Anterior</a>
                            <a href="{{urlBase}}{{paginaProxima}}" class="btn sec" {{(m.PaginaAtual >= m.TotalPaginas ? "style=\"opacity:0.4\"" : "")}}>Próxima ›</a>
                            <a href="{{urlBase}}{{m.TotalPaginas}}" class="btn sec" {{(m.PaginaAtual >= m.TotalPaginas ? "style=\"opacity:0.4\"" : "")}}>»</a>
                        </div>
                    </div>
                </div>

                <script>
                    function testar(tipo) {
                        var idMap = { email: 'email-dest', whatsapp: 'wapp-dest', sms: 'sms-dest', voz: 'voz-dest' };
                        var resMap = { email: 'email-res', whatsapp: 'wapp-res', sms: 'sms-res', voz: 'voz-res' };
                        var dest = document.getElementById(idMap[tipo]).value.trim();
                        var res  = document.getElementById(resMap[tipo]);
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

                    function liberarIp(ip, btn) {
                        if (!confirm('Liberar o IP ' + ip + '?')) return;
                        btn.disabled = true;
                        btn.textContent = '⏳';
                        fetch('/admin/ips-bloqueados/' + encodeURIComponent(ip) + '/liberar', { method: 'POST' })
                            .then(r => {
                                if (r.ok || r.status === 204) {
                                    var row = btn.closest('tr');
                                    row.style.opacity = '0.4';
                                    btn.textContent = '✅ Liberado';
                                } else {
                                    btn.disabled = false;
                                    btn.textContent = '🔓 Liberar';
                                    alert('Falha ao liberar IP. Status: ' + r.status);
                                }
                            })
                            .catch(e => { btn.disabled = false; btn.textContent = '🔓 Liberar'; alert(e.message); });
                    }

                    (function () {
                        var INTERVALO = 60, restante = INTERVALO, pagina = {{m.PaginaAtual}};
                        var el = document.getElementById('countdown');
                        function tick() {
                            el.textContent = 'Atualizando em ' + restante + 's';
                            restante <= 10 ? el.classList.add('urgente') : el.classList.remove('urgente');
                            if (restante-- <= 0) { window.location.href = '{{urlBase}}' + pagina; return; }
                            setTimeout(tick, 1000);
                        }
                        tick();

                        function fazerBackup() {
                            var btn = document.getElementById('btn-backup');
                            var res = document.getElementById('backup-res');
                            btn.disabled = true;
                            res.innerHTML = '<span style="color:#6E648B">⏳ Gerando backup...</span>';
                            fetch('/admin/backup', { method: 'POST' })
                                .then(r => {
                                    if (!r.ok) throw new Error('Status ' + r.status);
                                    return r.blob().then(blob => ({ blob, disposition: r.headers.get('content-disposition') }));
                                })
                                .then(({ blob, disposition }) => {
                                    var nome = 'backup.sql';
                                    if (disposition) { var m = disposition.match(/filename="?([^"]+)"?/); if (m) nome = m[1]; }
                                    var url = URL.createObjectURL(blob);
                                    var a = document.createElement('a'); a.href = url; a.download = nome; a.click();
                                    URL.revokeObjectURL(url);
                                    res.innerHTML = '<span style="color:#2E9E6B">✅ Backup gerado: ' + nome + '</span>';
                                })
                                .catch(e => { res.innerHTML = '<span style="color:#E73C3C">❌ ' + e.message + '</span>'; })
                                .finally(() => { btn.disabled = false; });
                        }

                        function restaurarBackup() {
                            var input = document.getElementById('restore-file');
                            var res   = document.getElementById('restore-res');
                            if (!input.files.length) { res.innerHTML = '<span style="color:#E73C3C">⚠️ Selecione um arquivo .sql</span>'; return; }
                            var form = new FormData();
                            form.append('arquivo', input.files[0]);
                            res.innerHTML = '<span style="color:#6E648B">⏳ Restaurando... isso pode levar alguns segundos.</span>';
                            fetch('/admin/restore', { method: 'POST', body: form })
                                .then(r => r.json().then(d => ({ ok: r.ok, d })))
                                .then(({ ok, d }) => {
                                    res.innerHTML = ok
                                        ? '<span style="color:#2E9E6B">✅ ' + d.message + '</span>'
                                        : '<span style="color:#E73C3C">❌ ' + d.error + '</span>';
                                })
                                .catch(e => { res.innerHTML = '<span style="color:#E73C3C">❌ ' + e.message + '</span>'; });
                        }

                        window.fazerBackup    = fazerBackup;
                        window.restaurarBackup = restaurarBackup;
                    })();
                <h2>Backup do Banco de Dados</h2>
                <div style="background:white;border-radius:16px;padding:20px;margin-bottom:24px;box-shadow:0 4px 16px rgba(0,0,0,.06)">
                    <div style="display:flex;gap:16px;flex-wrap:wrap;align-items:flex-start">
                        <div style="flex:1;min-width:200px">
                            <p style="font-size:13px;color:#6E648B;margin-bottom:12px">Gera um dump completo do banco provavida e faz o download como arquivo .sql.</p>
                            <button id="btn-backup" class="btn" onclick="fazerBackup()">📥 Fazer Backup</button>
                            <div id="backup-res" style="margin-top:8px;font-size:13px"></div>
                        </div>
                        <div style="flex:1;min-width:200px">
                            <p style="font-size:13px;color:#6E648B;margin-bottom:12px">Restaura o banco a partir de um arquivo .sql gerado anteriormente. ⚠️ Dados existentes podem ser afetados.</p>
                            <input type="file" id="restore-file" accept=".sql" style="font-size:13px;margin-bottom:8px;display:block"/>
                            <button class="btn" style="background:#E73C3C" onclick="restaurarBackup()">📤 Restaurar Backup</button>
                            <div id="restore-res" style="margin-top:8px;font-size:13px"></div>
                        </div>
                    </div>
                </div>

                <script>
                </script>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }
}

public record TesteDestinatarioRequest(string Destinatario);
