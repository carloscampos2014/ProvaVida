using FluentAssertions;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.Domain;

public class SessaoLoginTests
{
    private static SessaoLogin CriarSessao(DateTime? expiraEm = null, DateTime? refreshExpiraEm = null)
        => SessaoLogin.Criar(
            Guid.NewGuid(),
            "token-jwt",
            expiraEm ?? DateTime.UtcNow.AddHours(24),
            "refresh-token",
            refreshExpiraEm ?? DateTime.UtcNow.AddDays(365));

    [Fact]
    public void Criar_ComDadosValidos_RetornaSessaoAtiva()
    {
        var usuarioId = Guid.NewGuid();
        var expira = DateTime.UtcNow.AddHours(24);

        var sessao = SessaoLogin.Criar(usuarioId, "token-jwt", expira, "refresh", DateTime.UtcNow.AddDays(365));

        sessao.UsuarioId.Should().Be(usuarioId);
        sessao.Token.Should().Be("token-jwt");
        sessao.Ativo.Should().BeTrue();
        sessao.Id.Should().NotBeEmpty();
        sessao.RefreshTokenHash.Should().Be("refresh");
    }

    [Fact]
    public void EstaValida_QuandoAtivaENaoExpirada_RetornaTrue()
    {
        var sessao = CriarSessao(DateTime.UtcNow.AddHours(1));

        sessao.EstaValida().Should().BeTrue();
    }

    [Fact]
    public void EstaValida_QuandoExpirada_RetornaFalse()
    {
        var sessao = CriarSessao(DateTime.UtcNow.AddSeconds(-1));

        sessao.EstaValida().Should().BeFalse();
    }

    [Fact]
    public void Invalidar_MarcaSessaoComoInativa()
    {
        var sessao = CriarSessao(DateTime.UtcNow.AddHours(1));

        sessao.Invalidar();

        sessao.Ativo.Should().BeFalse();
        sessao.EstaValida().Should().BeFalse();
    }

    [Fact]
    public void RefreshTokenValido_QuandoAtivoENaoExpirado_RetornaTrue()
    {
        var sessao = CriarSessao();

        sessao.RefreshTokenValido().Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenValido_QuandoExpirado_RetornaFalse()
    {
        var sessao = CriarSessao(refreshExpiraEm: DateTime.UtcNow.AddSeconds(-1));

        sessao.RefreshTokenValido().Should().BeFalse();
    }

    [Fact]
    public void RefreshTokenValido_QuandoInvalidado_RetornaFalse()
    {
        var sessao = CriarSessao();
        sessao.Invalidar();

        sessao.RefreshTokenValido().Should().BeFalse();
    }
}
