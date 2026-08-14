using FluentAssertions;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.RegistrarCheckIn;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.RegistrarCheckIn;

public class RegistrarCheckInUseCaseTests
{
    private readonly Mock<ICheckInRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly RegistrarCheckInUseCase _useCase;

    public RegistrarCheckInUseCaseTests()
    {
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
        _useCase = new RegistrarCheckInUseCase(_repoMock.Object, _uowMock.Object);
    }

    private static RegistrarCheckInInput InputValido() => new(
        Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, -23.5, -46.6, "device-123");

    [Fact]
    public async Task ExecutarAsync_CheckInNovo_RetornaTrueECommita()
    {
        _repoMock.Setup(r => r.AdicionarSeNaoExistirAsync(It.IsAny<CheckIn>(), default))
            .ReturnsAsync(true);

        var resultado = await _useCase.ExecutarAsync(InputValido());

        resultado.Should().BeTrue();
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_CheckInDuplicado_RetornaFalseECommita()
    {
        _repoMock.Setup(r => r.AdicionarSeNaoExistirAsync(It.IsAny<CheckIn>(), default))
            .ReturnsAsync(false);

        var resultado = await _useCase.ExecutarAsync(InputValido());

        resultado.Should().BeFalse();
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoRepositorioFalha_FazRollback()
    {
        _repoMock.Setup(r => r.AdicionarSeNaoExistirAsync(It.IsAny<CheckIn>(), default))
            .ThrowsAsync(new Exception("DB error"));

        var act = () => _useCase.ExecutarAsync(InputValido());

        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackAsync(default), Times.Once);
    }
}
