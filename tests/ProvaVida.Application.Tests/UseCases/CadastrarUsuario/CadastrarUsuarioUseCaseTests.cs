using FluentAssertions;
using FluentValidation;
using Moq;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.CadastrarUsuario;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.CadastrarUsuario;

public class CadastrarUsuarioUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CadastrarUsuarioUseCase _useCase;

    public CadastrarUsuarioUseCaseTests()
    {
        var validator = new CadastrarUsuarioValidator();
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);

        _useCase = new CadastrarUsuarioUseCase(
            _repoMock.Object, _hasherMock.Object, _uowMock.Object, validator);
    }

    private static CadastrarUsuarioInput InputValido() => new(
        "João Silva", "joao@email.com", "11999999999", "senha@123",
        "Maria Silva", "maria@email.com", "11888888888");

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_RetornaGuid()
    {
        _repoMock.Setup(r => r.EmailExisteAsync("joao@email.com", default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("senha@123")).Returns("hash");
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>(), default)).Returns(Task.CompletedTask);

        var resultado = await _useCase.ExecutarAsync(InputValido());

        resultado.Should().NotBeEmpty();
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_EmailJaCadastrado_LancaAppException()
    {
        _repoMock.Setup(r => r.EmailExisteAsync("joao@email.com", default)).ReturnsAsync(true);

        var act = () => _useCase.ExecutarAsync(InputValido());

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 409);
    }

    [Fact]
    public async Task ExecutarAsync_SenhaInvalida_LancaValidationException()
    {
        var input = InputValido() with { Senha = "curta" };

        var act = () => _useCase.ExecutarAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecutarAsync_EmailInvalido_LancaValidationException()
    {
        var input = InputValido() with { Email = "nao-e-email" };

        var act = () => _useCase.ExecutarAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecutarAsync_QuandoRepositorioFalha_FazRollback()
    {
        _repoMock.Setup(r => r.EmailExisteAsync("joao@email.com", default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash");
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>(), default))
            .ThrowsAsync(new Exception("DB error"));

        var act = () => _useCase.ExecutarAsync(InputValido());

        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackAsync(default), Times.Once);
    }
}
