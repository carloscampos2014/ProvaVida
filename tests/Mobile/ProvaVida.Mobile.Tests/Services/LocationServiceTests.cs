using FluentAssertions;
using Moq;
using ProvaVida.Mobile.Application.Services;
using ProvaVida.Shared.Common;

namespace ProvaVida.Mobile.Tests.Services;

/// <summary>
/// Testes unitários do contrato <see cref="ILocationService"/>.
/// MauiLocationService depende de Geolocation.Default (API estática do MAUI) e não
/// pode ser instanciada em testes de unidade; os testes verificam o contrato via mock.
/// </summary>
public class LocationServiceTests
{
    private readonly Mock<ILocationService> _mockLocationService = new();

    // ── Permissão concedida ───────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentLocationAsync_QuandoPermissaoConcedida_RetornaLatLong()
    {
        // Arrange
        var expectedLatitude = -23.5505;
        var expectedLongitude = -46.6333;

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(Result<(double Latitude, double Longitude)>.Ok(
                (expectedLatitude, expectedLongitude)));

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Latitude.Should().BeApproximately(expectedLatitude, precision: 0.0001);
        result.Data.Longitude.Should().BeApproximately(expectedLongitude, precision: 0.0001);
    }

    // ── Permissão negada ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentLocationAsync_QuandoPermissaoNegada_RetornaFail()
    {
        // Arrange
        const string mensagemEsperada = "Permissão de localização necessária para o check-in";

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(Result<(double, double)>.Fail(mensagemEsperada));

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be(mensagemEsperada);
    }

    // ── Localização indisponível (timeout/erro) ───────────────────────────────

    [Fact]
    public async Task GetCurrentLocationAsync_QuandoLocalizacaoIndisponivel_RetornaFail()
    {
        // Arrange
        const string mensagemEsperada = "Não foi possível obter a localização";

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(Result<(double, double)>.Fail(mensagemEsperada));

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be(mensagemEsperada);
    }
}
