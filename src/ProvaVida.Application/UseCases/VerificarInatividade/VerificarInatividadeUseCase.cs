using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.VerificarInatividade;

/// <summary>
/// Fluxo 3 camadas anti-falso-positivo:
/// Camada 1 — Heartbeat recente → suspende alerta
/// Camada 2 — Push de aviso ao próprio usuário + janela de 6h
/// Camada 3 — Após janela expirar: dispara e-mail + WhatsApp ao contato de emergência
/// </summary>
public class VerificarInatividadeUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly IHeartbeatRepository _heartbeatRepository;
    private readonly INotificacaoEmergenciaRepository _notificacaoRepository;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IUnitOfWork _uow;

    private const int HorasInatividade    = 48;
    private const int HorasHeartbeat      = 24;
    private const int HorasJanelaGraca    = 6;

    public VerificarInatividadeUseCase(
        IUsuarioRepository usuarioRepository,
        ICheckInRepository checkInRepository,
        IHeartbeatRepository heartbeatRepository,
        INotificacaoEmergenciaRepository notificacaoRepository,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        IUnitOfWork uow)
    {
        _usuarioRepository = usuarioRepository;
        _checkInRepository = checkInRepository;
        _heartbeatRepository = heartbeatRepository;
        _notificacaoRepository = notificacaoRepository;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _uow = uow;
    }

    /// <summary>
    /// Detecta usuários inativos e executa as camadas 1 e 2.
    /// Chamado pelo job diário às 23h50.
    /// </summary>
    public async Task ExecutarDeteccaoAsync(CancellationToken ct = default)
    {
        var dataCorte = DateTime.UtcNow.AddHours(-HorasInatividade);
        var inativos = await _checkInRepository.ListarUsuariosInativosDesdeAsync(dataCorte, ct);

        foreach (var usuarioId in inativos)
        {
            // Evita reprocessar usuário que já está em ciclo ativo
            var jaEmCiclo = await _notificacaoRepository
                .ExisteNotificacaoAtivaNasUltimasHorasAsync(usuarioId, HorasInatividade, ct);
            if (jaEmCiclo) continue;

            // Camada 1 — Heartbeat recente?
            var temHeartbeat = await _heartbeatRepository
                .ExisteHeartbeatRecenteAsync(usuarioId, HorasHeartbeat, ct);

            if (temHeartbeat)
            {
                await GravarNotificacaoAsync(
                    NotificacaoEmergencia.CriarHeartbeatAtivo(usuarioId), ct);
                continue;
            }

            // Camada 2 — Envia aviso ao próprio usuário e registra janela de graça
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, ct);
            if (usuario is null || !usuario.Ativo) continue;

            await EnviarAvisoAoUsuarioAsync(usuario, ct);

            await GravarNotificacaoAsync(
                NotificacaoEmergencia.CriarAguardandoResposta(usuarioId, HorasJanelaGraca), ct);
        }
    }

    /// <summary>
    /// Verifica janelas de graça expiradas e dispara alerta ao contato de emergência.
    /// Chamado pelo job horário.
    /// </summary>
    public async Task ExecutarDisparoAsync(CancellationToken ct = default)
    {
        var janelasExpiradas = await _notificacaoRepository.ListarJanelasExpiradasAsync(ct);

        foreach (var notificacao in janelasExpiradas)
        {
            // Verifica se o usuário respondeu (novo heartbeat ou check-in desde o disparo)
            var respondeu = await _heartbeatRepository
                .ExisteHeartbeatRecenteAsync(notificacao.UsuarioId, HorasJanelaGraca, ct);

            if (respondeu)
            {
                notificacao.Cancelar();
                await GravarNotificacaoAsync(notificacao, ct);
                continue;
            }

            var usuario = await _usuarioRepository.ObterPorIdAsync(notificacao.UsuarioId, ct);
            if (usuario is null || !usuario.Ativo) continue;

            // Camada 3 — Dispara e-mail e WhatsApp ao contato de emergência (independentes)
            var emailEnviado = false;
            var whatsappEnviado = false;

            try
            {
                await _emailService.EnviarAsync(new EmailMensagem(
                    Para: usuario.ContatoEmergenciaEmail,
                    NomePara: usuario.ContatoEmergenciaNome,
                    Assunto: $"⚠️ {usuario.Nome} não fez check-in há mais de 48h",
                    CorpoHtml: MontarCorpoEmailEmergencia(usuario.Nome, usuario.ContatoEmergenciaNome)
                ), ct);
                emailEnviado = true;
            }
            catch { /* log seria registrado pelo Serilog via Hangfire */ }

            try
            {
                await _whatsAppService.EnviarAsync(
                    usuario.ContatoEmergenciaWhatsApp,
                    MontarMensagemWhatsApp(usuario.Nome, usuario.ContatoEmergenciaNome),
                    ct);
                whatsappEnviado = true;
            }
            catch { /* fallback — e-mail já foi tentado */ }

            var canal = (emailEnviado, whatsappEnviado) switch
            {
                (true, true)  => "email+whatsapp",
                (true, false) => "email",
                (false, true) => "whatsapp",
                _             => "falha"
            };

            await GravarNotificacaoAsync(
                NotificacaoEmergencia.CriarDisparado(notificacao.UsuarioId, canal), ct);
        }
    }

    private async Task GravarNotificacaoAsync(NotificacaoEmergencia notificacao, CancellationToken ct)
    {
        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            await _notificacaoRepository.AdicionarAsync(notificacao, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    private async Task EnviarAvisoAoUsuarioAsync(Usuario usuario, CancellationToken ct)
    {
        try
        {
            await _emailService.EnviarAsync(new EmailMensagem(
                Para: usuario.Email,
                NomePara: usuario.Nome,
                Assunto: "Está tudo bem com você?",
                CorpoHtml: MontarCorpoEmailAviso(usuario.Nome)
            ), ct);
        }
        catch { /* melhor esforço — continua mesmo se falhar */ }
    }

    private static string MontarCorpoEmailAviso(string nomeUsuario) => $"""
        <h2>Olá, {nomeUsuario}!</h2>
        <p>Não detectamos seu check-in no app <strong>ProvaVida</strong> nas últimas 48 horas.</p>
        <p>Se você está bem, abra o app e faça seu check-in para cancelar este aviso.</p>
        <p>Caso não responda em <strong>6 horas</strong>, notificaremos seu contato de emergência.</p>
        <br/>
        <p>Equipe ProvaVida</p>
        """;

    private static string MontarCorpoEmailEmergencia(string nomeUsuario, string nomeContato) => $"""
        <h2>Olá, {nomeContato}!</h2>
        <p>Você está cadastrado como contato de emergência de <strong>{nomeUsuario}</strong> no app ProvaVida.</p>
        <p><strong>{nomeUsuario}</strong> não realizou check-in nas últimas 48 horas e não respondeu ao aviso enviado.</p>
        <p>Recomendamos que você entre em contato com {nomeUsuario} para verificar se está bem.</p>
        <br/>
        <p>Equipe ProvaVida</p>
        """;

    private static string MontarMensagemWhatsApp(string nomeUsuario, string nomeContato) =>
        $"Olá {nomeContato}! Você é contato de emergência de *{nomeUsuario}* no ProvaVida. " +
        $"{nomeUsuario} não fez check-in há mais de 48h e não respondeu ao aviso. " +
        $"Por favor, verifique se está bem. - Equipe ProvaVida";
}
