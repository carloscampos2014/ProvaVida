using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Mobile.Application.Http;
using ProvaVida.Mobile.Application.Services;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Tests.Auth;

/// <summary>
/// Testes unitários para <see cref="CheckinMobileService"/> cobrindo os cenários
/// offline-first: internet, sem internet, duplicata SQLite e permissão negada.
/// </summary>
public class CheckinMobileServiceTests
{
    private readonly Mock<ISessionManager> _mockSession = new();
    private readonly Mock<ICheckinRepository> _mockCheckinRepo = new();
    private readonly Mock<ILocationService> _mockLocation = new();
    private readonly Mock<IConnectivityService> _mockConnectivity = new();
    private readonly Mock<IDeviceInfoService> _mockDeviceInfo = new();
    private readonly Mock<IApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<CheckinMobileService>> _mockLogger = new();

    private CheckinMobileService CriarSut() => new(
        _mockSession.Object,
        _mockCheckinRepo.Object,
        _mockLocation.Object,
        _mockConnectivity.Object,
        _mockDeviceInfo.Object,
        _mockApiClient.Object,
        _mockLogger.Object);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Usuario CriarUsuario() => new()
    {
        Id = Guid.NewGuid(),
        Nome = "João Silva",
        Email = "joao@teste.com",
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Maria",
        ContatoEmergenciaEmail = "maria@teste.com",
        ContatoEmergenciaWhatsapp = "11988888888",
        CriadoEm = DateTimeOffset.UtcNow,
        AtualizadoEm = DateTimeOffset.UtcNow
    };

    private static Checkin CriarCheckin(Guid usuarioId, bool sincronizado = false) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        Data = DateOnly.FromDateTime(DateTime.Today),
        Latitude = -23.5505,
        Longitude = -46.6333,
        IdentificacaoAparelho = "ModeloX/NomeY",
        Sincronizado = sincronizado,
        CriadoEm = DateTimeOffset.UtcNow
    };

    private static CheckinResponse CriarCheckinResponse(Checkin checkin) => new(
        checkin.Id,
        checkin.UsuarioId,
        checkin.Data,
        checkin.Latitude,
        checkin.Longitude,
        checkin.IdentificacaoAparelho,
        Sincronizado: true,
        CriadoEm: checkin.CriadoEm
    );

    private void ConfigurarLocalizacaoOk()
    {
        _mockLocation
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(Result<(double, double)>.Ok((-23.5505, -46.6333)));
    }

    private void ConfigurarSemCheckinHoje(Guid usuarioId)
    {
        _mockCheckinRepo
            .Setup(r => r.GetByUsuarioIdAndDataAsync(usuarioId, It.IsAny<DateOnly>()))
            .ReturnsAsync(Result<Checkin>.Fail("Check-in não encontrado para a data informada"));
    }

    private void ConfigurarUpsertOk()
    {
        _mockCheckinRepo
            .Setup(r => r.UpsertAsync(It.IsAny<Checkin>()))
            .ReturnsAsync(Result.Ok());
    }

    // ── RegistrarAsync: check-in com internet (sucesso) ───────────────────────

    [Fact]
    public async Task RegistrarAsync_ComInternet_DeveSalvarESincronizarComApi()
    {
        // Arrange
        var usuario = CriarUsuario();

        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(usuario);
        _mockDeviceInfo.Setup(d => d.GetIdentificacaoAparelho()).Returns("ModeloX/NomeY");
        _mockConnectivity.Setup(c => c.IsInternetAvailable).Returns(true);
        ConfigurarLocalizacaoOk();
        ConfigurarSemCheckinHoje(usuario.Id);
        ConfigurarUpsertOk();

        _mockApiClient
            .Setup(a => a.PostAsync<CheckinRequest, CheckinResponse>(
                "checkins", It.IsAny<CheckinRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckinResponse(
                Guid.NewGuid(), usuario.Id, DateOnly.FromDateTime(DateTime.Today),
                -23.5505, -46.6333, "ModeloX/NomeY", Sincronizado: true, DateTimeOffset.UtcNow));

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegistrarAsync();

        // Assert
        resultado.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Sincronizado.Should().BeTrue("deve ser marcado como sincronizado após POST bem-sucedido");

        // Deve chamar UpsertAsync duas vezes: salvar + atualizar Sincronizado=true
        _mockCheckinRepo.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Exactly(2));

        // API deve ter sido chamada
        _mockApiClient.Verify(
            a => a.PostAsync<CheckinRequest, CheckinResponse>(
                "checkins", It.IsAny<CheckinRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── RegistrarAsync: sem internet (salvo localmente) ───────────────────────

    [Fact]
    public async Task RegistrarAsync_SemInternet_DeveSalvarLocalmenteComSincronizadoFalse()
    {
        // Arrange
        var usuario = CriarUsuario();

        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(usuario);
        _mockDeviceInfo.Setup(d => d.GetIdentificacaoAparelho()).Returns("ModeloX/NomeY");
        _mockConnectivity.Setup(c => c.IsInternetAvailable).Returns(false);
        ConfigurarLocalizacaoOk();
        ConfigurarSemCheckinHoje(usuario.Id);
        ConfigurarUpsertOk();

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegistrarAsync();

        // Assert
        resultado.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Sincronizado.Should().BeFalse("sem internet, deve permanecer Sincronizado=false");

        // Deve chamar UpsertAsync apenas uma vez (salvar localmente)
        _mockCheckinRepo.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Once);

        // API NÃO deve ser chamada
        _mockApiClient.Verify(
            a => a.PostAsync<CheckinRequest, CheckinResponse>(
                It.IsAny<string>(), It.IsAny<CheckinRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── RegistrarAsync: duplicata SQLite ──────────────────────────────────────

    [Fact]
    public async Task RegistrarAsync_ComCheckinExistenteNoSqlite_DeveRetornarExistenteSemNovoRegistro()
    {
        // Arrange
        var usuario = CriarUsuario();
        var checkinExistente = CriarCheckin(usuario.Id, sincronizado: true);

        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(usuario);
        ConfigurarLocalizacaoOk();

        _mockCheckinRepo
            .Setup(r => r.GetByUsuarioIdAndDataAsync(usuario.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(Result<Checkin>.Ok(checkinExistente));

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegistrarAsync();

        // Assert
        resultado.Success.Should().BeTrue();
        resultado.Data.Should().Be(checkinExistente, "deve retornar o check-in já existente");

        // UpsertAsync e API NÃO devem ser chamados
        _mockCheckinRepo.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Never);
        _mockApiClient.Verify(
            a => a.PostAsync<CheckinRequest, CheckinResponse>(
                It.IsAny<string>(), It.IsAny<CheckinRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── RegistrarAsync: permissão de localização negada ───────────────────────

    [Fact]
    public async Task RegistrarAsync_PermissaoLocalizacaoNegada_DeveRetornarFalha()
    {
        // Arrange
        const string mensagemEsperada = "Permissão de localização necessária para o check-in";

        _mockLocation
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(Result<(double, double)>.Fail(mensagemEsperada));

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegistrarAsync();

        // Assert
        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().Be(mensagemEsperada);

        // Nada deve ser persistido nem chamado
        _mockSession.Verify(s => s.GetCurrentUserAsync(), Times.Never);
        _mockCheckinRepo.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Never);
        _mockApiClient.Verify(
            a => a.PostAsync<CheckinRequest, CheckinResponse>(
                It.IsAny<string>(), It.IsAny<CheckinRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── RegistrarAsync: API retorna 409 ───────────────────────────────────────

    [Fact]
    public async Task RegistrarAsync_QuandoApi409_DeveSincronizarLocalmenteSemFalhar()
    {
        // Arrange
        var usuario = CriarUsuario();

        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(usuario);
        _mockDeviceInfo.Setup(d => d.GetIdentificacaoAparelho()).Returns("ModeloX/NomeY");
        _mockConnectivity.Setup(c => c.IsInternetAvailable).Returns(true);
        ConfigurarLocalizacaoOk();
        ConfigurarSemCheckinHoje(usuario.Id);
        ConfigurarUpsertOk();

        _mockApiClient
            .Setup(a => a.PostAsync<CheckinRequest, CheckinResponse>(
                "checkins", It.IsAny<CheckinRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Conflict", null, HttpStatusCode.Conflict));

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegistrarAsync();

        // Assert
        resultado.Success.Should().BeTrue("409 não é falha — o check-in já está no servidor");
        resultado.Data!.Sincronizado.Should().BeTrue("deve marcar Sincronizado=true ao receber 409");

        // UpsertAsync chamado duas vezes: salvar + atualizar flag
        _mockCheckinRepo.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Exactly(2));
    }

    // ── RegistrarAsync: API retorna outro erro ────────────────────────────────

    [Fact]
    public async Task RegistrarAsync_QuandoApiFalhaComOutroErro_DeveManterSincronizadoFalse()
    {
        // Arrange
        var usuario = CriarUsuario();

        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(usuario);
        _mockDeviceInfo.Setup(d => d.GetIdentificacaoAparelho()).Returns("ModeloX/NomeY");
        _mockConnectivity.Setup(c => c.IsInternetAvailable).Returns(true);
        ConfigurarLocalizacaoOk();
        ConfigurarSemCheckinHoje(usuario.Id);
        ConfigurarUpsertOk();

        _mockApiClient
            .Setup(a => a.PostAsync<CheckinRequest, CheckinResponse>(
                "checkins", It.IsAny<CheckinRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Internal Server Error", null, HttpStatusCode.InternalServerError));

        var sut = CriarSut();

        // Act
        var resultado = await sut.RegistrarAsync();

        // Assert
        resultado.Success.Should().BeTrue("erro de API não impede o check-in local");
        resultado.Data!.Sincronizado.Should().BeFalse("deve manter Sincronizado=false após erro da API");

        // UpsertAsync chamado apenas uma vez (salvar localmente — sem atualização de flag)
        _mockCheckinRepo.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Once);
    }

    // ── GetCheckinHojeAsync: check-in existente ───────────────────────────────

    [Fact]
    public async Task GetCheckinHojeAsync_ComCheckinExistente_DeveRetornarCheckin()
    {
        // Arrange
        var usuario = CriarUsuario();
        var checkin = CriarCheckin(usuario.Id, sincronizado: true);

        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(usuario);
        _mockCheckinRepo
            .Setup(r => r.GetByUsuarioIdAndDataAsync(usuario.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(Result<Checkin>.Ok(checkin));

        var sut = CriarSut();

        // Act
        var resultado = await sut.GetCheckinHojeAsync();

        // Assert
        resultado.Success.Should().BeTrue();
        resultado.Data.Should().Be(checkin);
    }

    // ── GetCheckinHojeAsync: sem check-in hoje ────────────────────────────────

    [Fact]
    public async Task GetCheckinHojeAsync_SemCheckinHoje_DeveRetornarOkComDataNull()
    {
        // Arrange
        var usuario = CriarUsuario();

        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync(usuario);
        _mockCheckinRepo
            .Setup(r => r.GetByUsuarioIdAndDataAsync(usuario.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(Result<Checkin>.Fail("Check-in não encontrado para a data informada"));

        var sut = CriarSut();

        // Act
        var resultado = await sut.GetCheckinHojeAsync();

        // Assert
        resultado.Success.Should().BeTrue("ausência de check-in não é falha");
        resultado.Data.Should().BeNull("sem check-in hoje, Data deve ser null");
    }

    // ── GetCheckinHojeAsync: sem usuário logado ───────────────────────────────

    [Fact]
    public async Task GetCheckinHojeAsync_SemUsuarioLogado_DeveRetornarFalha()
    {
        // Arrange
        _mockSession.Setup(s => s.GetCurrentUserAsync()).ReturnsAsync((Usuario?)null);

        var sut = CriarSut();

        // Act
        var resultado = await sut.GetCheckinHojeAsync();

        // Assert
        resultado.Success.Should().BeFalse();
        resultado.MessageErro.Should().Be("Nenhum usuário logado.");
    }
}
