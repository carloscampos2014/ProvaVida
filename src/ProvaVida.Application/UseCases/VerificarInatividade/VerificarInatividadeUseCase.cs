using Microsoft.Extensions.Logging;
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
    private readonly ISmsService _smsService;
    private readonly IVoiceService _voiceService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<VerificarInatividadeUseCase> _logger;

    private const int HorasInatividade = 48;
    private const int HorasHeartbeat   = 24;
    private const int HorasJanelaGraca = 6;

    public VerificarInatividadeUseCase(
        IUsuarioRepository usuarioRepository,
        ICheckInRepository checkInRepository,
        IHeartbeatRepository heartbeatRepository,
        INotificacaoEmergenciaRepository notificacaoRepository,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        ISmsService smsService,
        IVoiceService voiceService,
        IUnitOfWork uow,
        ILogger<VerificarInatividadeUseCase> logger)
    {
        _usuarioRepository     = usuarioRepository;
        _checkInRepository     = checkInRepository;
        _heartbeatRepository   = heartbeatRepository;
        _notificacaoRepository = notificacaoRepository;
        _emailService          = emailService;
        _whatsAppService       = whatsAppService;
        _smsService            = smsService;
        _voiceService          = voiceService;
        _uow                   = uow;
        _logger                = logger;
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
            // Cancela APENAS se o usuário fez check-in dentro da janela de graça
            // Heartbeat não é suficiente — só check-in confirma que o usuário está bem
            var fezCheckIn = await _checkInRepository
                .ExisteCheckInRecenteAsync(notificacao.UsuarioId, HorasJanelaGraca, ct);

            if (fezCheckIn)
            {
                notificacao.Cancelar();
                await GravarNotificacaoAsync(notificacao, ct);
                continue;
            }

            var usuario = await _usuarioRepository.ObterPorIdAsync(notificacao.UsuarioId, ct);
            if (usuario is null || !usuario.Ativo) continue;

            // Camada 3 — Dispara e-mail, WhatsApp e SMS ao contato de emergência (independentes)
            var emailEnviado    = false;
            var whatsappEnviado = false;
            var smsEnviado      = false;

            try
            {
                await _emailService.EnviarAsync(new EmailMensagem(
                    Para: usuario.ContatoEmergenciaEmail,
                    NomePara: usuario.ContatoEmergenciaNome,
                    Assunto: $"⚠️ {usuario.Nome} não fez check-in há mais de 48h",
                    CorpoHtml: MontarCorpoEmailEmergencia(usuario.Nome, usuario.ContatoEmergenciaNome)
                ), ct);
                emailEnviado = true;
                _logger.LogInformation(
                    "E-mail de emergência enviado para {Contato} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaEmail, usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao enviar e-mail de emergência para {Contato} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaEmail, usuario.Id);
            }

            try
            {
                await _whatsAppService.EnviarAsync(
                    usuario.ContatoEmergenciaWhatsApp,
                    MontarMensagemWhatsApp(usuario.Nome, usuario.ContatoEmergenciaNome),
                    ct);
                whatsappEnviado = true;
                _logger.LogInformation(
                    "WhatsApp de emergência enviado para {Telefone} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaWhatsApp, usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao enviar WhatsApp de emergência para {Telefone} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaWhatsApp, usuario.Id);
            }

            try
            {
                await _smsService.EnviarAsync(
                    usuario.ContatoEmergenciaWhatsApp,
                    MontarMensagemSms(usuario.Nome, usuario.ContatoEmergenciaNome),
                    ct);
                smsEnviado = true;
                _logger.LogInformation(
                    "SMS de emergência enviado para {Telefone} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaWhatsApp, usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao enviar SMS de emergência para {Telefone} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaWhatsApp, usuario.Id);
            }

            var vozEnviada = false;
            try
            {
                await _voiceService.LigarAsync(
                    usuario.ContatoEmergenciaWhatsApp,
                    MontarMensagemVoz(usuario.Nome, usuario.ContatoEmergenciaNome),
                    ct);
                vozEnviada = true;
                _logger.LogInformation(
                    "Ligação de emergência iniciada para {Telefone} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaWhatsApp, usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao iniciar ligação de emergência para {Telefone} referente ao usuário {UsuarioId}",
                    usuario.ContatoEmergenciaWhatsApp, usuario.Id);
            }

            var canais = new List<string>();
            if (emailEnviado)    canais.Add("email");
            if (whatsappEnviado) canais.Add("whatsapp");
            if (smsEnviado)      canais.Add("sms");
            if (vozEnviada)      canais.Add("voz");
            var canal = canais.Count > 0 ? string.Join("+", canais) : "falha";

            _logger.LogInformation(
                "Resultado do disparo para usuário {UsuarioId}: canal={Canal}",
                usuario.Id, canal);

            // Atualiza o registro aguardando_resposta original para disparado
            // Sem isso o job horário continuaria reprocessando o mesmo registro
            notificacao.MarcarComoProcessado(canal);
            await GravarNotificacaoAsync(notificacao, ct);

            // Registra também um novo evento de disparo para histórico
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
            _logger.LogInformation(
                "Aviso de inatividade enviado ao usuário {UsuarioId} ({Email})",
                usuario.Id, usuario.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Falha ao enviar aviso de inatividade ao usuário {UsuarioId} ({Email})",
                usuario.Id, usuario.Email);
        }
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

    private static string MontarMensagemSms(string nomeUsuario, string nomeContato) =>
        $"ProvaVida: Ola {nomeContato}! {nomeUsuario} nao fez check-in ha mais de 48h e nao respondeu ao aviso. " +
        $"Por favor, verifique se esta bem.";

    private static string MontarMensagemVoz(string nomeUsuario, string nomeContato) =>
        $"Olá! Você é contato de emergência de {nomeUsuario} no ProvaVida. " +
        $"{nomeUsuario} não fez check-in há mais de 48 horas e não respondeu ao aviso enviado. " +
        $"Por favor, verifique se está bem. Esta mensagem foi enviada automaticamente pelo ProvaVida.";
}
