using FluentAssertions;
using Moq;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.ExcluirConta;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.ExcluirConta;

public class ExcluirContaUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly ExcluirContaUseCase _useCase;

    public ExcluirContaUseCaseTests()
    {
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
        _useCase = new ExcluirContaUseCase(_repoMock.Object, _hasherMock.Object, _uowMock.Object);
    }

    private static Usuario CriarUsuario() => Usuario.Criar(
        "João", "joao@email.com", "11999999999",
        "hash_bcrypt", "Maria", "maria@email.com", "11888888888");

    [Fact]
    public async Task ExecutarAsync_ComSenhaCorreta_AnonimizaECommita()
    {
        var usuario = CriarUsuario();
        _repoMock.Setup(r => r.ObterPorIdAsync(usuario.Id, default)).ReturnsAsync(usuario);
        _hasherMock.Setup(h => h.Verificar("senha123", "hash_bcrypt")).Returns(true);
        _repoMock.Setup(r => r.AnonimizarAsync(usuario.Id, It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.InvalidarSessoesAsync(usuario.Id, default)).Returns(Task.CompletedTask);

        await _useCase.ExecutarAsync(new ExcluirContaInput(usuario.Id, "senha123"));

        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
        _repoMock.Verify(r => r.AnonimizarAsync(usuario.Id, It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        _repoMock.Verify(r => r.InvalidarSessoesAsync(usuario.Id, default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_SenhaIncorreta_LancaAppException()
    {
        var usuario = CriarUsuario();
        _repoMock.Setup(r => r.ObterPorIdAsync(usuario.Id, default)).ReturnsAsync(usuario);
        _hasherMock.Setup(h => h.Verificar("senhaerrada", "hash_bcrypt")).Returns(false);

        var act = () => _useCase.ExecutarAsync(new ExcluirContaInput(usuario.Id, "senhaerrada"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task ExecutarAsync_UsuarioNaoEncontrado_LancaAppException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.ObterPorIdAsync(id, default)).ReturnsAsync((Usuario?)null);

        var act = () => _useCase.ExecutarAsync(new ExcluirContaInput(id, "senha"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 404);
    }
}
