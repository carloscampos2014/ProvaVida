using FluentAssertions;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.ObterHistoricoCheckIn;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.ObterHistoricoCheckIn;

public class ObterHistoricoCheckInUseCaseTests
{
    private readonly Mock<ICheckInRepository> _repoMock = new();
    private readonly ObterHistoricoCheckInUseCase _useCase;

    public ObterHistoricoCheckInUseCaseTests()
    {
        _useCase = new ObterHistoricoCheckInUseCase(_repoMock.Object);
    }

    private static CheckIn CriarCheckIn(Guid usuarioId) =>
        CheckIn.Criar(usuarioId, Guid.NewGuid(), DateTimeOffset.UtcNow, -23.5, -46.6, "device");

    [Fact]
    public async Task ExecutarAsync_SemDatas_UsaUltimos7Dias()
    {
        var usuarioId = Guid.NewGuid();
        var checkIns = new[] { CriarCheckIn(usuarioId) };

        _repoMock.Setup(r => r.ListarPorUsuarioAsync(
                usuarioId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync(checkIns);

        var resultado = await _useCase.ExecutarAsync(new ObterHistoricoCheckInInput(usuarioId));

        resultado.Should().HaveCount(1);
        _repoMock.Verify(r => r.ListarPorUsuarioAsync(
            usuarioId,
            It.Is<DateTimeOffset>(d => d < DateTimeOffset.UtcNow.AddDays(-6)),
            It.Is<DateTimeOffset>(d => d >= DateTimeOffset.UtcNow.AddMinutes(-1)),
            default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_ComDatasEspecificas_PassaDatasCorretas()
    {
        var usuarioId = Guid.NewGuid();
        var inicio = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var fim    = new DateTimeOffset(2026, 8, 7, 0, 0, 0, TimeSpan.Zero);

        _repoMock.Setup(r => r.ListarPorUsuarioAsync(usuarioId, inicio, fim, default))
            .ReturnsAsync([]);

        var resultado = await _useCase.ExecutarAsync(
            new ObterHistoricoCheckInInput(usuarioId, inicio, fim));

        resultado.Should().BeEmpty();
        _repoMock.Verify(r => r.ListarPorUsuarioAsync(usuarioId, inicio, fim, default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_MapeiaParaDto()
    {
        var usuarioId = Guid.NewGuid();
        var checkIn = CriarCheckIn(usuarioId);

        _repoMock.Setup(r => r.ListarPorUsuarioAsync(
                usuarioId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync([checkIn]);

        var resultado = (await _useCase.ExecutarAsync(
            new ObterHistoricoCheckInInput(usuarioId))).ToList();

        resultado.Should().HaveCount(1);
        resultado[0].Id.Should().Be(checkIn.Id);
        resultado[0].IdLocal.Should().Be(checkIn.IdLocal);
        resultado[0].Latitude.Should().Be(checkIn.Latitude);
        resultado[0].DeviceId.Should().Be(checkIn.DeviceId);
    }
}
