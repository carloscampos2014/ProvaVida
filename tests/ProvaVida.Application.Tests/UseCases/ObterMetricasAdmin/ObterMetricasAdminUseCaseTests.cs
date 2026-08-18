using FluentAssertions;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.ObterMetricasAdmin;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.ObterMetricasAdmin;

public class ObterMetricasAdminUseCaseTests
{
    private readonly Mock<IAdminMetricasRepository> _repoMock = new();
    private readonly ObterMetricasAdminUseCase _useCase;

    public ObterMetricasAdminUseCaseTests()
    {
        _useCase = new ObterMetricasAdminUseCase(_repoMock.Object);
    }

    private void ConfigurarRespostasDefault(
        int usuariosAtivos = 10,
        int novos7d = 2,
        int checkInHoje = 7,
        int atrasados = 3,
        int semInternet = 1,
        int avisosHoje = 2,
        int alertasHoje = 1,
        int canceladosHoje = 1,
        int totalAlertas = 15,
        int totalEventos = 5)
    {
        _repoMock.Setup(r => r.ContarUsuariosAtivosAsync(default)).ReturnsAsync(usuariosAtivos);
        _repoMock.Setup(r => r.ContarNovosUsuariosAsync(7, default)).ReturnsAsync(novos7d);
        _repoMock.Setup(r => r.ContarUsuariosComCheckInHojeAsync(default)).ReturnsAsync(checkInHoje);
        _repoMock.Setup(r => r.ContarUsuariosComCheckInAtrasadoAsync(2, default)).ReturnsAsync(atrasados);
        _repoMock.Setup(r => r.ContarUsuariosPossivelmnteSemInternetAsync(default)).ReturnsAsync(semInternet);
        _repoMock.Setup(r => r.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.AguardandoResposta, default)).ReturnsAsync(avisosHoje);
        _repoMock.Setup(r => r.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Disparado, default)).ReturnsAsync(alertasHoje);
        _repoMock.Setup(r => r.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Cancelado, default)).ReturnsAsync(canceladosHoje);
        _repoMock.Setup(r => r.ContarTotalNotificacoesPorStatusAsync(NotificacaoEmergencia.Statuses.Disparado, default)).ReturnsAsync(totalAlertas);
        _repoMock.Setup(r => r.ContarTotalEventosAsync(default)).ReturnsAsync(totalEventos);
        _repoMock.Setup(r => r.ListarEventosAsync(It.IsAny<int>(), It.IsAny<int>(), default))
                 .ReturnsAsync([]);
    }

    [Fact]
    public async Task ExecutarAsync_RetornaMetricasCorretamente()
    {
        ConfigurarRespostasDefault();

        var resultado = await _useCase.ExecutarAsync();

        resultado.TotalUsuariosAtivos.Should().Be(10);
        resultado.NovoUsuariosUltimos7Dias.Should().Be(2);
        resultado.UsuariosComCheckInHoje.Should().Be(7);
        resultado.UsuariosComCheckInAtrasado.Should().Be(3);
        resultado.UsuariosPossivelmnteSemInternet.Should().Be(1);
        resultado.AvisosEnviadosAoUsuarioHoje.Should().Be(2);
        resultado.AlertasDisparadosAoContatoHoje.Should().Be(1);
        resultado.AlertasCanceladosHoje.Should().Be(1);
        resultado.TotalAlertasDisparadosHistorico.Should().Be(15);
        resultado.TotalEventos.Should().Be(5);
    }

    [Fact]
    public async Task ExecutarAsync_PaginacaoCorreta_CalculaTotalPaginas()
    {
        ConfigurarRespostasDefault(totalEventos: 45);

        var resultado = await _useCase.ExecutarAsync(pagina: 2);

        resultado.PaginaAtual.Should().Be(2);
        resultado.TamanhoPagina.Should().Be(ObterMetricasAdminUseCase.TamanhoPaginaDefault);
        resultado.TotalPaginas.Should().Be(3); // ceil(45/20) = 3
        _repoMock.Verify(r => r.ListarEventosAsync(2, ObterMetricasAdminUseCase.TamanhoPaginaDefault, default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_PaginaMenorQue1_UsaPagina1()
    {
        ConfigurarRespostasDefault();

        var resultado = await _useCase.ExecutarAsync(pagina: -5);

        resultado.PaginaAtual.Should().Be(1);
    }

    [Fact]
    public async Task ExecutarAsync_GeradoEmEstaProximoDeUtcNow()
    {
        ConfigurarRespostasDefault();
        var antes = DateTime.UtcNow;

        var resultado = await _useCase.ExecutarAsync();

        resultado.GeradoEm.Should().BeOnOrAfter(antes);
        resultado.GeradoEm.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(2));
    }

    [Fact]
    public async Task ExecutarAsync_InvocarTodasAsQueriesUmaVez()
    {
        ConfigurarRespostasDefault();

        await _useCase.ExecutarAsync();

        _repoMock.Verify(r => r.ContarUsuariosAtivosAsync(default), Times.Once);
        _repoMock.Verify(r => r.ContarNovosUsuariosAsync(7, default), Times.Once);
        _repoMock.Verify(r => r.ContarUsuariosComCheckInHojeAsync(default), Times.Once);
        _repoMock.Verify(r => r.ContarUsuariosComCheckInAtrasadoAsync(2, default), Times.Once);
        _repoMock.Verify(r => r.ContarUsuariosPossivelmnteSemInternetAsync(default), Times.Once);
        _repoMock.Verify(r => r.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.AguardandoResposta, default), Times.Once);
        _repoMock.Verify(r => r.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Disparado, default), Times.Once);
        _repoMock.Verify(r => r.ContarNotificacoesPorStatusHojeAsync(NotificacaoEmergencia.Statuses.Cancelado, default), Times.Once);
        _repoMock.Verify(r => r.ContarTotalNotificacoesPorStatusAsync(NotificacaoEmergencia.Statuses.Disparado, default), Times.Once);
        _repoMock.Verify(r => r.ContarTotalEventosAsync(default), Times.Once);
        _repoMock.Verify(r => r.ListarEventosAsync(It.IsAny<int>(), It.IsAny<int>(), default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoBancoZerado_RetornaZerosEmTudo()
    {
        ConfigurarRespostasDefault(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        var resultado = await _useCase.ExecutarAsync();

        resultado.TotalUsuariosAtivos.Should().Be(0);
        resultado.NovoUsuariosUltimos7Dias.Should().Be(0);
        resultado.UsuariosComCheckInHoje.Should().Be(0);
        resultado.UsuariosComCheckInAtrasado.Should().Be(0);
        resultado.UsuariosPossivelmnteSemInternet.Should().Be(0);
        resultado.AvisosEnviadosAoUsuarioHoje.Should().Be(0);
        resultado.AlertasDisparadosAoContatoHoje.Should().Be(0);
        resultado.AlertasCanceladosHoje.Should().Be(0);
        resultado.TotalAlertasDisparadosHistorico.Should().Be(0);
        resultado.TotalEventos.Should().Be(0);
        resultado.Eventos.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutarAsync_QuandoRepositorioFalha_PropagaExcecao()
    {
        _repoMock.Setup(r => r.ContarUsuariosAtivosAsync(default))
            .ThrowsAsync(new Exception("DB error"));

        var act = () => _useCase.ExecutarAsync();

        await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
    }
}
