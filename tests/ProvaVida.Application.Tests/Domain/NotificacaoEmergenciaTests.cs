using FluentAssertions;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.Domain;

public class NotificacaoEmergenciaTests
{
    [Fact]
    public void CriarHeartbeatAtivo_RetornaStatusCorreto()
    {
        var n = NotificacaoEmergencia.CriarHeartbeatAtivo(Guid.NewGuid());
        n.Status.Should().Be(NotificacaoEmergencia.Statuses.HeartbeatAtivo);
        n.JanelaExpiraEm.Should().BeNull();
    }

    [Fact]
    public void CriarAguardandoResposta_DefineFutura()
    {
        var n = NotificacaoEmergencia.CriarAguardandoResposta(Guid.NewGuid(), 6);
        n.Status.Should().Be(NotificacaoEmergencia.Statuses.AguardandoResposta);
        n.JanelaExpiraEm.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void CriarDisparado_RetornaStatusECanal()
    {
        var n = NotificacaoEmergencia.CriarDisparado(Guid.NewGuid(), "email+whatsapp");
        n.Status.Should().Be(NotificacaoEmergencia.Statuses.Disparado);
        n.Canal.Should().Be("email+whatsapp");
    }

    [Fact]
    public void Cancelar_AlteraStatusParaCancelado()
    {
        var n = NotificacaoEmergencia.CriarAguardandoResposta(Guid.NewGuid());
        n.Cancelar();
        n.Status.Should().Be(NotificacaoEmergencia.Statuses.Cancelado);
    }
}
