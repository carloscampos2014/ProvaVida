using FluentAssertions;
using Moq;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.Login;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.Login;

public class LoginUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _repoMock = new();
    private readonly Mock<ISessaoLoginRepository> _sessaoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRefreshTokenHasher> _refreshHasherMock = new();
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
        _refreshHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash-fake");

        _useCase = new LoginUseCase(
            _repoMock.Object, _sessaoMock.Object,
            _hasherMock.Object, _jwtMock.Object, _uowMock.Object, _refreshHasherMock.Object);
    }

    private static Usuario CriarUsuario() => Usuario.Criar(
        "João", "joao@email.com", "11999999999",
        "hash_bcrypt", "Maria", "maria@email.com", "11888888888");

    [Fact]
    public async Task ExecutarAsync_CredenciaisValidas_RetornaToken()
    {
        var usuario = CriarUsuario();
        var expira = DateTime.UtcNow.AddHours(24);

        _repoMock.Setup(r => r.ObterPorEmailAsync("joao@email.com", default)).ReturnsAsync(usuario);
        _hasherMock.Setup(h => h.Verificar("senha123", "hash_bcrypt")).Returns(true);
        _jwtMock.Setup(j => j.GerarToken(usuario, out expira)).Returns("jwt-token");
        _sessaoMock.Setup(s => s.AdicionarAsync(It.IsAny<SessaoLogin>(), default)).Returns(Task.CompletedTask);

        var resultado = await _useCase.ExecutarAsync(new LoginInput("joao@email.com", "senha123"));

        resultado.Token.Should().Be("jwt-token");
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_UsuarioNaoEncontrado_LancaAppException()
    {
        _repoMock.Setup(r => r.ObterPorEmailAsync("naoexiste@email.com", default))
            .ReturnsAsync((Usuario?)null);

        var act = () => _useCase.ExecutarAsync(new LoginInput("naoexiste@email.com", "senha"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task ExecutarAsync_SenhaIncorreta_LancaAppException()
    {
        var usuario = CriarUsuario();
        _repoMock.Setup(r => r.ObterPorEmailAsync("joao@email.com", default)).ReturnsAsync(usuario);
        _hasherMock.Setup(h => h.Verificar("senhaerrada", "hash_bcrypt")).Returns(false);

        var act = () => _useCase.ExecutarAsync(new LoginInput("joao@email.com", "senhaerrada"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }
}
