using FluentAssertions;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.Domain;

public class HeartbeatTests
{
    [Fact]
    public void Criar_ComDadosValidos_RetornaHeartbeatCorreto()
    {
        var usuarioId = Guid.NewGuid();
        var dataHora = DateTime.UtcNow;

        var heartbeat = Heartbeat.Criar(usuarioId, dataHora);

        heartbeat.Id.Should().NotBeEmpty();
        heartbeat.UsuarioId.Should().Be(usuarioId);
        heartbeat.DataHora.Should().Be(dataHora);
    }
}
