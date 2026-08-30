using FluentAssertions;
using Moq;
using ProvaVida.Api.Infrastructure.Repositories;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Tests.Repositories;

/// <summary>
/// Testes unitários para <see cref="PostgresCheckinRepository"/>.
/// </summary>
public class PostgresCheckinRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        // Arrange
        var mockFactory = new Mock<IDbConnectionFactory>();
        var repo = new FakeNotFoundCheckinRepository(mockFactory.Object);

        // Act
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpsertAsync_DeveRetornarSuccess_QuandoConexaoOk()
    {
        // Arrange
        var mockFactory = new Mock<IDbConnectionFactory>();
        var repo = new FakeSuccessCheckinRepository(mockFactory.Object);
        var checkin = CriarCheckinValido();

        // Act
        var result = await repo.UpsertAsync(checkin);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarSuccess_QuandoConexaoOk()
    {
        // Arrange
        var mockFactory = new Mock<IDbConnectionFactory>();
        var repo = new FakeSuccessCheckinRepository(mockFactory.Object);

        // Act
        var result = await repo.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeTrue();
    }

    private static Checkin CriarCheckinValido() => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        Data = DateOnly.FromDateTime(DateTime.UtcNow),
        Latitude = -23.5505,
        Longitude = -46.6333,
        IdentificacaoAparelho = "device-abc-123",
        Sincronizado = false,
        CriadoEm = DateTimeOffset.UtcNow
    };

    // --- Fakes ---

    private class FakeNotFoundCheckinRepository : PostgresCheckinRepository
    {
        public FakeNotFoundCheckinRepository(IDbConnectionFactory factory) : base(factory) { }

        public override async Task<ProvaVida.Shared.Common.Result<Checkin>> GetByIdAsync(Guid id)
        {
            await Task.CompletedTask;
            return ProvaVida.Shared.Common.Result<Checkin>.Fail("Entidade não encontrada");
        }
    }

    private class FakeSuccessCheckinRepository : PostgresCheckinRepository
    {
        public FakeSuccessCheckinRepository(IDbConnectionFactory factory) : base(factory) { }

        public override async Task<ProvaVida.Shared.Common.Result> UpsertAsync(Checkin entity)
        {
            await Task.CompletedTask;
            return ProvaVida.Shared.Common.Result.Ok();
        }

        public override async Task<ProvaVida.Shared.Common.Result> DeleteAsync(Guid id)
        {
            await Task.CompletedTask;
            return ProvaVida.Shared.Common.Result.Ok();
        }
    }
}
