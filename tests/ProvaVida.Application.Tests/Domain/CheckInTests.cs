using FluentAssertions;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.Domain;

public class CheckInTests
{
    [Fact]
    public void Criar_ComDadosValidos_RetornaCheckInCorreto()
    {
        var usuarioId = Guid.NewGuid();
        var idLocal = Guid.NewGuid();
        var dataHora = DateTime.UtcNow;

        var checkIn = CheckIn.Criar(usuarioId, idLocal, dataHora, -23.5, -46.6, "device-123");

        checkIn.Id.Should().NotBeEmpty();
        checkIn.UsuarioId.Should().Be(usuarioId);
        checkIn.IdLocal.Should().Be(idLocal);
        checkIn.DataHora.Should().Be(dataHora);
        checkIn.Latitude.Should().Be(-23.5);
        checkIn.Longitude.Should().Be(-46.6);
        checkIn.DeviceId.Should().Be("device-123");
    }

    [Fact]
    public void Criar_SemLocalizacao_RetornaCheckInComNullLatLong()
    {
        var checkIn = CheckIn.Criar(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, "device");

        checkIn.Latitude.Should().BeNull();
        checkIn.Longitude.Should().BeNull();
    }

    [Fact]
    public void Criar_DeviceIdComEspacos_TrimaEspacos()
    {
        var checkIn = CheckIn.Criar(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, "  device-abc  ");

        checkIn.DeviceId.Should().Be("device-abc");
    }
}
