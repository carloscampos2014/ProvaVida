using FluentAssertions;
using Moq;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.RegistrarHeartbeat;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.RegistrarHeartbeat;

public class RegistrarHeartbeatUseCaseTests
{
    private readonly Mock<IHeartbeatRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly RegistrarHeartbeatUseCase _useCase;

    public RegistrarHeartbeatUseCaseTests()
    {
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
        _useCase = new RegistrarHeartbeatUseCase(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_GravaECommita()
    {
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Heartbeat>(), default))
            .Returns(Task.CompletedTask);

        await _useCase.ExecutarAsync(new RegistrarHeartbeatInput(Guid.NewGuid(), DateTime.UtcNow));

        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Heartbeat>(), default), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoRepositorioFalha_FazRollback()
    {
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Heartbeat>(), default))
            .ThrowsAsync(new Exception("DB error"));

        var act = () => _useCase.ExecutarAsync(new RegistrarHeartbeatInput(Guid.NewGuid(), DateTime.UtcNow));

        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackAsync(default), Times.Once);
    }
}
