using FluentAssertions;
using Moq;
using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.RefreshToken;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.UseCases.RefreshToken;

public class RefreshTokenUseCaseTests
{
    private readonly Mock<ISessaoLoginRepository> _sessaoMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRefreshTokenHasher> _refreshHasherMock = new();
    private readonly RefreshTokenUseCase _useCase;

    public RefreshTokenUseCaseTests()
    {
        _uowMock.Setup(x => x.BeginAsync(default, default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
        _sessaoMock.Setup(s => s.SalvarAlteracoesAsync(default)).Returns(Task.CompletedTask);
        _sessaoMock.Setup(s => s.AdicionarAsync(It.IsAny<SessaoLogin>(), default)).Returns(Task.CompletedTask);
        _refreshHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash-fake");

        _useCase = new RefreshTokenUseCase(
            _sessaoMock.Object,
            _usuarioMock.Object,
            _jwtMock.Object,
            _uowMock.Object,
            _refreshHasherMock.Object);
    }

    private static Usuario CriarUsuario()
    {
        var u = (Usuario)System.Runtime.CompilerServices
            .RuntimeHelpers.GetUninitializedObject(typeof(Usuario));
        typeof(Usuario).GetProperty("Id")!
            .SetValue(u, Guid.NewGuid());
        typeof(Usuario).GetProperty("Nome")!
            .SetValue(u, "Carlos");
        typeof(Usuario).GetProperty("Email")!
            .SetValue(u, "carlos@teste.com");
        typeof(Usuario).GetProperty("Ativo")!
            .SetValue(u, true);
        return u;
    }

    [Fact]
    public async Task ExecutarAsync_RefreshTokenValido_RetornaNovosPares()
    {
        var usuario = CriarUsuario();
        var sessao = SessaoLogin.Criar(
            usuario.Id, "token-antigo", DateTime.UtcNow.AddHours(-1),
            "refresh-valido", DateTime.UtcNow.AddDays(365));

        _sessaoMock.Setup(s => s.ObterPorRefreshTokenAsync("refresh-valido", default))
            .ReturnsAsync(sessao);
        _usuarioMock.Setup(u => u.ObterPorIdAsync(usuario.Id, default))
            .ReturnsAsync(usuario);

        var expiraEm = DateTime.UtcNow.AddHours(24);
        _jwtMock.Setup(j => j.GerarToken(usuario, out expiraEm)).Returns("novo-token");
        _jwtMock.Setup(j => j.GerarRefreshToken()).Returns("novo-refresh");

        var result = await _useCase.ExecutarAsync(new RefreshTokenInput("refresh-valido"));

        result.Token.Should().Be("novo-token");
        result.RefreshToken.Should().Be("novo-refresh");
        sessao.Ativo.Should().BeFalse(); // sessão antiga invalidada
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_RefreshTokenInexistente_LancaAppException()
    {
        _sessaoMock.Setup(s => s.ObterPorRefreshTokenAsync("invalido", default))
            .ReturnsAsync((SessaoLogin?)null);

        var act = () => _useCase.ExecutarAsync(new RefreshTokenInput("invalido"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task ExecutarAsync_RefreshTokenExpirado_LancaAppException()
    {
        var sessao = SessaoLogin.Criar(
            Guid.NewGuid(), "token", DateTime.UtcNow.AddHours(1),
            "refresh-exp", DateTime.UtcNow.AddSeconds(-1)); // refresh expirado

        _sessaoMock.Setup(s => s.ObterPorRefreshTokenAsync("refresh-exp", default))
            .ReturnsAsync(sessao);

        var act = () => _useCase.ExecutarAsync(new RefreshTokenInput("refresh-exp"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task ExecutarAsync_UsuarioInativo_LancaAppException()
    {
        var usuario = CriarUsuario();
        typeof(Usuario).GetProperty("Ativo")!.SetValue(usuario, false);

        var sessao = SessaoLogin.Criar(
            usuario.Id, "token", DateTime.UtcNow.AddHours(1),
            "refresh-valido", DateTime.UtcNow.AddDays(365));

        _sessaoMock.Setup(s => s.ObterPorRefreshTokenAsync("refresh-valido", default))
            .ReturnsAsync(sessao);
        _usuarioMock.Setup(u => u.ObterPorIdAsync(usuario.Id, default))
            .ReturnsAsync(usuario);

        var act = () => _useCase.ExecutarAsync(new RefreshTokenInput("refresh-valido"));

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.StatusCode == 401);
    }
}
