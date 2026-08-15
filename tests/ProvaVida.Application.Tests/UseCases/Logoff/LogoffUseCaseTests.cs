using FluentAssertions;
using Moq;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.Logoff;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.Logoff;

public class LogoffUseCaseTests
{
    private readonly Mock<ISessaoLoginRepository> _sessaoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly LogoffUseCase _useCase;

    public LogoffUseCaseTests()
    {
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
        _useCase = new LogoffUseCase(_sessaoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task ExecutarAsync_SessaoValida_InvalidaECommita()
    {
        var sessao = SessaoLogin.Criar(Guid.NewGuid(), "token-valido", DateTime.UtcNow.AddHours(1), "refresh", DateTime.UtcNow.AddDays(365));
        _sessaoMock.Setup(s => s.ObterPorTokenAsync("token-valido", default)).ReturnsAsync(sessao);
        _sessaoMock.Setup(s => s.SalvarAlteracoesAsync(default)).Returns(Task.CompletedTask);

        await _useCase.ExecutarAsync(new LogoffInput("token-valido"));

        sessao.Ativo.Should().BeFalse();
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_TokenNaoEncontrado_LancaAppException()
    {
        _sessaoMock.Setup(s => s.ObterPorTokenAsync("token-invalido", default))
            .ReturnsAsync((SessaoLogin?)null);

        var act = () => _useCase.ExecutarAsync(new LogoffInput("token-invalido"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task ExecutarAsync_SessaoExpirada_LancaAppException()
    {
        var sessao = SessaoLogin.Criar(Guid.NewGuid(), "token-exp", DateTime.UtcNow.AddSeconds(-1), "refresh", DateTime.UtcNow.AddDays(365));
        _sessaoMock.Setup(s => s.ObterPorTokenAsync("token-exp", default)).ReturnsAsync(sessao);

        var act = () => _useCase.ExecutarAsync(new LogoffInput("token-exp"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }
}
