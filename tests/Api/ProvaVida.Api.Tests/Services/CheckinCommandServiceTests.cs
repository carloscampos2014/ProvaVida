using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Services;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Tests.Services;

/// <summary>
/// Testes unitários para <see cref="CheckinCommandService"/>.
/// </summary>
public class CheckinCommandServiceTests
{
    private readonly Mock<ICheckinRepository> _repositoryMock;
    private readonly Mock<ILogger<CheckinCommandService>> _loggerMock;
    private readonly CheckinCommandService _service;
    private readonly Faker _faker;

    public CheckinCommandServiceTests()
    {
        _repositoryMock = new Mock<ICheckinRepository>();
        _loggerMock = new Mock<ILogger<CheckinCommandService>>();
        _service = new CheckinCommandService(_repositoryMock.Object, _loggerMock.Object);
        _faker = new Faker("pt_BR");
    }

    // ── Cenário 1: Sucesso ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_QuandoNaoExisteDuplicata_DeveRetornarCheckinCriado()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: _faker.Random.Guid(),
            Data: DateOnly.FromDateTime(DateTime.Today),
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: _faker.Internet.UserAgent()
        );

        _repositoryMock
            .Setup(r => r.GetByUsuarioIdAndDataAsync(command.UsuarioId, command.Data))
            .ReturnsAsync(Result<Checkin>.Fail("não encontrado"));

        _repositoryMock
            .Setup(r => r.UpsertAsync(It.IsAny<Checkin>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UsuarioId.Should().Be(command.UsuarioId);
        result.Data.Data.Should().Be(command.Data);
        result.Data.Latitude.Should().Be(command.Latitude);
        result.Data.Longitude.Should().Be(command.Longitude);
        result.Data.IdentificacaoAparelho.Should().Be(command.IdentificacaoAparelho);
        result.Data.Sincronizado.Should().BeTrue();
        result.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_QuandoSucesso_DeveChamarUpsertUmaVez()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: _faker.Random.Guid(),
            Data: DateOnly.FromDateTime(DateTime.Today),
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: "Samsung Galaxy S24"
        );

        _repositoryMock
            .Setup(r => r.GetByUsuarioIdAndDataAsync(command.UsuarioId, command.Data))
            .ReturnsAsync(Result<Checkin>.Fail("não encontrado"));

        _repositoryMock
            .Setup(r => r.UpsertAsync(It.IsAny<Checkin>()))
            .ReturnsAsync(Result.Ok());

        // Act
        await _service.CreateAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Once);
    }

    // ── Cenário 2: Duplicata ───────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_QuandoJaExisteCheckinNoDia_DeveRetornarFail()
    {
        // Arrange
        var usuarioId = _faker.Random.Guid();
        var data = DateOnly.FromDateTime(DateTime.Today);

        var checkinExistente = new Checkin
        {
            Id = _faker.Random.Guid(),
            UsuarioId = usuarioId,
            Data = data,
            Latitude = -23.5505,
            Longitude = -46.6333,
            IdentificacaoAparelho = "iPhone 15",
            Sincronizado = true,
            CriadoEm = DateTimeOffset.UtcNow.AddHours(-2)
        };

        var command = new CriarCheckinCommand(
            UsuarioId: usuarioId,
            Data: data,
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: "Samsung Galaxy S24"
        );

        _repositoryMock
            .Setup(r => r.GetByUsuarioIdAndDataAsync(usuarioId, data))
            .ReturnsAsync(Result<Checkin>.Ok(checkinExistente));

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Já existe um check-in para hoje");
    }

    [Fact]
    public async Task CreateAsync_QuandoJaExisteCheckinNoDia_NaoDeveChamarUpsert()
    {
        // Arrange
        var usuarioId = _faker.Random.Guid();
        var data = DateOnly.FromDateTime(DateTime.Today);

        var checkinExistente = new Checkin
        {
            Id = _faker.Random.Guid(),
            UsuarioId = usuarioId,
            Data = data,
            Latitude = -23.5505,
            Longitude = -46.6333,
            IdentificacaoAparelho = "iPhone 15",
            Sincronizado = true,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByUsuarioIdAndDataAsync(usuarioId, data))
            .ReturnsAsync(Result<Checkin>.Ok(checkinExistente));

        var command = new CriarCheckinCommand(
            UsuarioId: usuarioId,
            Data: data,
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: "outro aparelho"
        );

        // Act
        await _service.CreateAsync(command);

        // Assert — UpsertAsync jamais deve ser chamado quando há duplicata
        _repositoryMock.Verify(r => r.UpsertAsync(It.IsAny<Checkin>()), Times.Never);
    }

    // ── Cenário 3: Validação inválida ──────────────────────────────────────

    [Fact]
    public async Task CreateAsync_QuandoIdentificacaoAparelhoVazia_DeveRetornarFail()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: _faker.Random.Guid(),
            Data: DateOnly.FromDateTime(DateTime.Today),
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: string.Empty
        );

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateAsync_QuandoUsuarioIdVazio_DeveRetornarFail()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: Guid.Empty,
            Data: DateOnly.FromDateTime(DateTime.Today),
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: "Samsung Galaxy S24"
        );

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("UsuarioId");
    }

    [Fact]
    public async Task CreateAsync_QuandoDataFutura_DeveRetornarFail()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: _faker.Random.Guid(),
            Data: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: "Samsung Galaxy S24"
        );

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("futura");
    }

    [Fact]
    public async Task CreateAsync_QuandoLatitudeForaDoLimite_DeveRetornarFail()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: _faker.Random.Guid(),
            Data: DateOnly.FromDateTime(DateTime.Today),
            Latitude: 91.0,  // acima de 90
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: "Samsung Galaxy S24"
        );

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Latitude");
    }

    [Fact]
    public async Task CreateAsync_QuandoLongitudeForaDoLimite_DeveRetornarFail()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: _faker.Random.Guid(),
            Data: DateOnly.FromDateTime(DateTime.Today),
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: 181.0,  // acima de 180
            IdentificacaoAparelho: "Samsung Galaxy S24"
        );

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Longitude");
    }

    // ── Cenário 4: Falha no repositório ───────────────────────────────────

    [Fact]
    public async Task CreateAsync_QuandoUpsertFalha_DeveRetornarFail()
    {
        // Arrange
        var command = new CriarCheckinCommand(
            UsuarioId: _faker.Random.Guid(),
            Data: DateOnly.FromDateTime(DateTime.Today),
            Latitude: _faker.Random.Double(-90, 90),
            Longitude: _faker.Random.Double(-180, 180),
            IdentificacaoAparelho: "Samsung Galaxy S24"
        );

        _repositoryMock
            .Setup(r => r.GetByUsuarioIdAndDataAsync(command.UsuarioId, command.Data))
            .ReturnsAsync(Result<Checkin>.Fail("não encontrado"));

        _repositoryMock
            .Setup(r => r.UpsertAsync(It.IsAny<Checkin>()))
            .ReturnsAsync(Result.Fail("erro de banco"));

        // Act
        var result = await _service.CreateAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("erro de banco");
    }
}
