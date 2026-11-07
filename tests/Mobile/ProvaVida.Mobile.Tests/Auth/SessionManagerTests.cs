using FluentAssertions;
using Moq;
using ProvaVida.Mobile.Application.Auth;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Tests.Auth;

/// <summary>
/// Testes unitários para <see cref="SessionManager"/>.
/// </summary>
public class SessionManagerTests
{
    [Fact]
    public async Task SessionManager_IsLoggedIn_DeveRetornarTrue_QuandoUsuarioExisteNoSqlite()
    {
        // Arrange
        var usuario = CriarUsuarioValido();

        var mockUsuarioRepo = new Mock<IUsuarioRepository>();
        mockUsuarioRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([usuario]));

        var mockCheckinRepo = new Mock<ICheckinRepository>();

        var sut = new SessionManager(mockUsuarioRepo.Object, mockCheckinRepo.Object);

        // Act
        var logado = await sut.IsLoggedInAsync();

        // Assert
        logado.Should().BeTrue();
    }

    [Fact]
    public async Task SessionManager_ClearSession_DeveRemoverUsuarioECheckins()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var checkin = CriarCheckinValido(usuario.Id);

        var mockUsuarioRepo = new Mock<IUsuarioRepository>();
        mockUsuarioRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok([usuario]));
        mockUsuarioRepo
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok());

        var mockCheckinRepo = new Mock<ICheckinRepository>();
        mockCheckinRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Checkin>>.Ok([checkin]));
        mockCheckinRepo
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok());

        var sut = new SessionManager(mockUsuarioRepo.Object, mockCheckinRepo.Object);

        // Act
        await sut.ClearSessionAsync();

        // Assert — check-ins excluídos antes do usuário (ordem obrigatória: Fluxo-Logoff.md)
        mockCheckinRepo.Verify(r => r.DeleteAsync(checkin.Id), Times.Once);
        mockUsuarioRepo.Verify(r => r.DeleteAsync(usuario.Id), Times.Once);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Usuario CriarUsuarioValido() => new()
    {
        Id = Guid.NewGuid(),
        Nome = "João Silva",
        Email = "joao@exemplo.com",
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Maria Silva",
        ContatoEmergenciaEmail = "maria@exemplo.com",
        ContatoEmergenciaWhatsapp = "11988888888",
        CriadoEm = DateTimeOffset.UtcNow,
        AtualizadoEm = DateTimeOffset.UtcNow
    };

    private static Checkin CriarCheckinValido(Guid usuarioId) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        Data = DateOnly.FromDateTime(DateTime.Today),
        Latitude = -23.5505,
        Longitude = -46.6333,
        IdentificacaoAparelho = "device-test-001",
        Sincronizado = false,
        CriadoEm = DateTimeOffset.UtcNow
    };
}
