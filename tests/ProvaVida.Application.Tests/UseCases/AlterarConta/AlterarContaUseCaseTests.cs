using FluentAssertions;
using FluentValidation;
using Moq;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.AlterarConta;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.AlterarConta;

public class AlterarContaUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly AlterarContaUseCase _useCase;

    public AlterarContaUseCaseTests()
    {
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
        _useCase = new AlterarContaUseCase(_repoMock.Object, _uowMock.Object, new AlterarContaValidator());
    }

    private static Usuario CriarUsuario() => Usuario.Criar(
        "João", "joao@email.com", "11999999999",
        "hash", "Maria", "maria@email.com", "11888888888");

    private static AlterarContaInput InputValido(Guid id) => new(
        id, "João Novo", "11777777777", "Pedro", "pedro@email.com", "11666666666");

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_AtualizaECommita()
    {
        var usuario = CriarUsuario();
        _repoMock.Setup(r => r.ObterPorIdAsync(usuario.Id, default)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.AtualizarAsync(usuario, default)).Returns(Task.CompletedTask);

        await _useCase.ExecutarAsync(InputValido(usuario.Id));

        usuario.Nome.Should().Be("João Novo");
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_UsuarioNaoEncontrado_LancaAppException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.ObterPorIdAsync(id, default)).ReturnsAsync((Usuario?)null);

        var act = () => _useCase.ExecutarAsync(InputValido(id));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 404);
    }

    [Fact]
    public async Task ExecutarAsync_NomeVazio_LancaValidationException()
    {
        var id = Guid.NewGuid();
        var input = new AlterarContaInput(id, "", "11777777777", "Pedro", "pedro@email.com", "11666666666");

        var act = () => _useCase.ExecutarAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
