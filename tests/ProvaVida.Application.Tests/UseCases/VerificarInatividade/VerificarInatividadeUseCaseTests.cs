using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.VerificarInatividade;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.VerificarInatividade;

public class VerificarInatividadeUseCaseTests
{
    private readonly Mock<IUsuarioRepository>               _usuarioRepo  = new();
    private readonly Mock<ICheckInRepository>               _checkInRepo  = new();
    private readonly Mock<IHeartbeatRepository>             _heartbeatRepo = new();
    private readonly Mock<INotificacaoEmergenciaRepository> _notifRepo    = new();
    private readonly Mock<IEmailService>                    _emailSvc     = new();
    private readonly Mock<IWhatsAppService>                 _wappSvc      = new();
    private readonly Mock<ISmsService>                      _smsSvc       = new();
    private readonly Mock<IVoiceService>                    _voiceSvc     = new();
    private readonly Mock<IUnitOfWork>                      _uow          = new();
    private readonly VerificarInatividadeUseCase            _useCase;

    public VerificarInatividadeUseCaseTests()
    {
        _uow.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uow.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uow.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);

        _useCase = new VerificarInatividadeUseCase(
            _usuarioRepo.Object, _checkInRepo.Object, _heartbeatRepo.Object,
            _notifRepo.Object, _emailSvc.Object, _wappSvc.Object, _smsSvc.Object, _voiceSvc.Object, _uow.Object,
            NullLogger<VerificarInatividadeUseCase>.Instance);
    }

    private static Usuario CriarUsuario() => Usuario.Criar(
        "João", "joao@email.com", "11999999999", "hash",
        "Maria", "maria@email.com", "11888888888");

    [Fact]
    public async Task ExecutarDeteccaoAsync_SemInativos_NaoGravaNotificacao()
    {
        _checkInRepo.Setup(r => r.ListarUsuariosInativosDesdeAsync(It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync([]);

        await _useCase.ExecutarDeteccaoAsync();

        _notifRepo.Verify(r => r.AdicionarAsync(It.IsAny<NotificacaoEmergencia>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecutarDeteccaoAsync_ComHeartbeat_GravaHeartbeatAtivo()
    {
        var usuarioId = Guid.NewGuid();
        _checkInRepo.Setup(r => r.ListarUsuariosInativosDesdeAsync(It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync([usuarioId]);
        _notifRepo.Setup(r => r.ExisteNotificacaoAtivaNasUltimasHorasAsync(usuarioId, It.IsAny<int>(), default))
            .ReturnsAsync(false);
        _heartbeatRepo.Setup(r => r.ExisteHeartbeatRecenteAsync(usuarioId, It.IsAny<int>(), default))
            .ReturnsAsync(true);

        await _useCase.ExecutarDeteccaoAsync();

        _notifRepo.Verify(r => r.AdicionarAsync(
            It.Is<NotificacaoEmergencia>(n => n.Status == NotificacaoEmergencia.Statuses.HeartbeatAtivo),
            default), Times.Once);
    }

    [Fact]
    public async Task ExecutarDeteccaoAsync_SemHeartbeat_GravaAguardandoResposta()
    {
        var usuario = CriarUsuario();
        _checkInRepo.Setup(r => r.ListarUsuariosInativosDesdeAsync(It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync([usuario.Id]);
        _notifRepo.Setup(r => r.ExisteNotificacaoAtivaNasUltimasHorasAsync(usuario.Id, It.IsAny<int>(), default))
            .ReturnsAsync(false);
        _heartbeatRepo.Setup(r => r.ExisteHeartbeatRecenteAsync(usuario.Id, It.IsAny<int>(), default))
            .ReturnsAsync(false);
        _usuarioRepo.Setup(r => r.ObterPorIdAsync(usuario.Id, default)).ReturnsAsync(usuario);
        _emailSvc.Setup(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default)).Returns(Task.CompletedTask);

        await _useCase.ExecutarDeteccaoAsync();

        _notifRepo.Verify(r => r.AdicionarAsync(
            It.Is<NotificacaoEmergencia>(n => n.Status == NotificacaoEmergencia.Statuses.AguardandoResposta),
            default), Times.Once);
    }

    [Fact]
    public async Task ExecutarDeteccaoAsync_UsuarioJaEmCiclo_NaoReprocessa()
    {
        var usuarioId = Guid.NewGuid();
        _checkInRepo.Setup(r => r.ListarUsuariosInativosDesdeAsync(It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync([usuarioId]);
        _notifRepo.Setup(r => r.ExisteNotificacaoAtivaNasUltimasHorasAsync(usuarioId, It.IsAny<int>(), default))
            .ReturnsAsync(true);

        await _useCase.ExecutarDeteccaoAsync();

        _notifRepo.Verify(r => r.AdicionarAsync(It.IsAny<NotificacaoEmergencia>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecutarDisparoAsync_UsuarioFezCheckIn_CancelaNotificacao()
    {
        var usuario = CriarUsuario();
        var notif = NotificacaoEmergencia.CriarAguardandoResposta(usuario.Id);

        _notifRepo.Setup(r => r.ListarJanelasExpiradasAsync(default)).ReturnsAsync([notif]);
        _checkInRepo.Setup(r => r.ExisteCheckInRecenteAsync(usuario.Id, It.IsAny<int>(), default))
            .ReturnsAsync(true);

        await _useCase.ExecutarDisparoAsync();

        notif.Status.Should().Be(NotificacaoEmergencia.Statuses.Cancelado);
    }

    [Fact]
    public async Task ExecutarDisparoAsync_HeartbeatSemCheckIn_NaoCancela_DispararAlerta()
    {
        var usuario = CriarUsuario();
        var notif = NotificacaoEmergencia.CriarAguardandoResposta(usuario.Id);

        _notifRepo.Setup(r => r.ListarJanelasExpiradasAsync(default)).ReturnsAsync([notif]);
        _checkInRepo.Setup(r => r.ExisteCheckInRecenteAsync(usuario.Id, It.IsAny<int>(), default))
            .ReturnsAsync(false);
        _usuarioRepo.Setup(r => r.ObterPorIdAsync(usuario.Id, default)).ReturnsAsync(usuario);
        _emailSvc.Setup(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default)).Returns(Task.CompletedTask);
        _wappSvc.Setup(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);
        _smsSvc.Setup(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);
        _voiceSvc.Setup(v => v.LigarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        await _useCase.ExecutarDisparoAsync();

        _emailSvc.Verify(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default), Times.Once);
        _notifRepo.Verify(r => r.AdicionarAsync(
            It.Is<NotificacaoEmergencia>(n => n.Status == NotificacaoEmergencia.Statuses.Disparado),
            default), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecutarDisparoAsync_JanelaExpirada_DispararEmailWhatsappESms()
    {
        var usuario = CriarUsuario();
        var notif = NotificacaoEmergencia.CriarAguardandoResposta(usuario.Id);

        _notifRepo.Setup(r => r.ListarJanelasExpiradasAsync(default)).ReturnsAsync([notif]);
        _checkInRepo.Setup(r => r.ExisteCheckInRecenteAsync(usuario.Id, It.IsAny<int>(), default))
            .ReturnsAsync(false);
        _usuarioRepo.Setup(r => r.ObterPorIdAsync(usuario.Id, default)).ReturnsAsync(usuario);
        _emailSvc.Setup(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default)).Returns(Task.CompletedTask);
        _wappSvc.Setup(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);
        _smsSvc.Setup(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);
        _voiceSvc.Setup(v => v.LigarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        await _useCase.ExecutarDisparoAsync();

        _emailSvc.Verify(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default), Times.Once);
        _wappSvc.Verify(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        _smsSvc.Verify(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        _voiceSvc.Verify(v => v.LigarAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        _notifRepo.Verify(r => r.AdicionarAsync(
            It.Is<NotificacaoEmergencia>(n => n.Status == NotificacaoEmergencia.Statuses.Disparado),
            default), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecutarDisparoAsync_WhatsAppFalha_EmailESmsDisparadosMesmoAssim()
    {
        var usuario = CriarUsuario();
        var notif = NotificacaoEmergencia.CriarAguardandoResposta(usuario.Id);

        _notifRepo.Setup(r => r.ListarJanelasExpiradasAsync(default)).ReturnsAsync([notif]);
        _checkInRepo.Setup(r => r.ExisteCheckInRecenteAsync(usuario.Id, It.IsAny<int>(), default))
            .ReturnsAsync(false);
        _usuarioRepo.Setup(r => r.ObterPorIdAsync(usuario.Id, default)).ReturnsAsync(usuario);
        _emailSvc.Setup(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default)).Returns(Task.CompletedTask);
        _wappSvc.Setup(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("WhatsApp API timeout"));
        _smsSvc.Setup(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);
        _voiceSvc.Setup(v => v.LigarAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        await _useCase.ExecutarDisparoAsync();

        _emailSvc.Verify(s => s.EnviarAsync(It.IsAny<EmailMensagem>(), default), Times.Once);
        _smsSvc.Verify(s => s.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        _notifRepo.Verify(r => r.AdicionarAsync(
            It.Is<NotificacaoEmergencia>(n =>
                n.Status == NotificacaoEmergencia.Statuses.Disparado && n.Canal == "email+sms+voz"),
            default), Times.Exactly(2));
    }
}
