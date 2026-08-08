using FluentAssertions;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.Domain;

public class SessaoLoginTests
{
    [Fact]
    public void Criar_ComDadosValidos_RetornaSessaoAtiva()
    {
        var usuarioId = Guid.NewGuid();
        var expira = DateTime.UtcNow.AddHours(24);

        var sessao = SessaoLogin.Criar(usuarioId, "token-jwt", expira);

        sessao.UsuarioId.Should().Be(usuarioId);
        sessao.Token.Should().Be("token-jwt");
        sessao.Ativo.Should().BeTrue();
        sessao.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void EstaValida_QuandoAtivaENaoExpirada_RetornaTrue()
    {
        var sessao = SessaoLogin.Criar(Guid.NewGuid(), "token", DateTime.UtcNow.AddHours(1));

        sessao.EstaValida().Should().BeTrue();
    }

    [Fact]
    public void EstaValida_QuandoExpirada_RetornaFalse()
    {
        var sessao = SessaoLogin.Criar(Guid.NewGuid(), "token", DateTime.UtcNow.AddSeconds(-1));

        sessao.EstaValida().Should().BeFalse();
    }

    [Fact]
    public void Invalidar_MarcaSessaoComoInativa()
    {
        var sessao = SessaoLogin.Criar(Guid.NewGuid(), "token", DateTime.UtcNow.AddHours(1));

        sessao.Invalidar();

        sessao.Ativo.Should().BeFalse();
        sessao.EstaValida().Should().BeFalse();
    }
}
