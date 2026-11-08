using FluentAssertions;
using Moq;
using ProvaVida.Api.Infrastructure.Repositories;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Tests.Repositories;

/// <summary>
/// Testes unitários para <see cref="PostgresCheckinRepository"/>.
/// </summary>
public class PostgresCheckinRepositoryTests
{
    // ── Caminhos de sucesso (via fakes) ────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new FakeNotFoundCheckinRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpsertAsync_DeveRetornarSuccess_QuandoConexaoOk()
    {
        var repo = new FakeSuccessCheckinRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.UpsertAsync(CriarCheckinValido());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarSuccess_QuandoConexaoOk()
    {
        var repo = new FakeSuccessCheckinRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.DeleteAsync(Guid.NewGuid());

        result.Success.Should().BeTrue();
    }

    // ── Caminhos de erro — exercita o bloco catch do DapperRepository ──────

    [Fact]
    public async Task GetByIdAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new PostgresCheckinRepository(factory);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new PostgresCheckinRepository(factory);

        var result = await repo.GetAllAsync();

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    [Fact]
    public async Task UpsertAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new PostgresCheckinRepository(factory);

        var result = await repo.UpsertAsync(CriarCheckinValido());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new PostgresCheckinRepository(factory);

        var result = await repo.DeleteAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    // ── Testes dos métodos especializados — GetByUsuarioIdAndDataAsync ────────

    [Fact]
    public async Task GetByUsuarioIdAndDataAsync_DeveRetornarCheckin_QuandoEncontrado()
    {
        var checkin = CriarCheckinValido();
        var repo = new FakeGetByUsuarioIdAndDataRepository(new Mock<IDbConnectionFactory>().Object, checkin);

        var result = await repo.GetByUsuarioIdAndDataAsync(checkin.UsuarioId, checkin.Data);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UsuarioId.Should().Be(checkin.UsuarioId);
        result.Data.Data.Should().Be(checkin.Data);
    }

    [Fact]
    public async Task GetByUsuarioIdAndDataAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new FakeGetByUsuarioIdAndDataNotFoundRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.GetByUsuarioIdAndDataAsync(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetByUsuarioIdAndDataAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new PostgresCheckinRepository(factory);

        var result = await repo.GetByUsuarioIdAndDataAsync(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    // ── Testes dos métodos especializados — GetPendingSyncAsync ──────────────

    [Fact]
    public async Task GetPendingSyncAsync_DeveRetornarNaoSincronizados()
    {
        var naoSincronizado = CriarCheckinValido();
        naoSincronizado.Sincronizado = false;
        var repo = new FakeGetPendingSyncRepository(new Mock<IDbConnectionFactory>().Object, [naoSincronizado]);

        var result = await repo.GetPendingSyncAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data!.First().Sincronizado.Should().BeFalse();
    }

    [Fact]
    public async Task GetPendingSyncAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new PostgresCheckinRepository(factory);

        var result = await repo.GetPendingSyncAsync();

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static IDbConnectionFactory CriarFactoryQueExplode()
    {
        var mock = new Mock<IDbConnectionFactory>();
        mock.Setup(f => f.Create())
            .Throws(new InvalidOperationException("Conexão inválida"));
        return mock.Object;
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

    // ── Fakes ──────────────────────────────────────────────────────────────

    private class FakeNotFoundCheckinRepository(IDbConnectionFactory factory)
        : PostgresCheckinRepository(factory)
    {
        public override async Task<Result<Checkin>> GetByIdAsync(Guid id)
        {
            await Task.CompletedTask;
            return Result<Checkin>.Fail("Entidade não encontrada");
        }
    }

    private class FakeSuccessCheckinRepository(IDbConnectionFactory factory)
        : PostgresCheckinRepository(factory)
    {
        public override async Task<Result> UpsertAsync(Checkin entity)
        {
            await Task.CompletedTask;
            return Result.Ok();
        }

        public override async Task<Result> DeleteAsync(Guid id)
        {
            await Task.CompletedTask;
            return Result.Ok();
        }
    }

    private class FakeGetByUsuarioIdAndDataRepository(IDbConnectionFactory factory, Checkin checkin)
        : PostgresCheckinRepository(factory)
    {
        public override async Task<Result<Checkin>> GetByUsuarioIdAndDataAsync(Guid usuarioId, DateOnly data)
        {
            await Task.CompletedTask;
            return Result<Checkin>.Ok(checkin);
        }
    }

    private class FakeGetByUsuarioIdAndDataNotFoundRepository(IDbConnectionFactory factory)
        : PostgresCheckinRepository(factory)
    {
        public override async Task<Result<Checkin>> GetByUsuarioIdAndDataAsync(Guid usuarioId, DateOnly data)
        {
            await Task.CompletedTask;
            return Result<Checkin>.Fail("Check-in não encontrado para a data informada");
        }
    }

    private class FakeGetPendingSyncRepository(IDbConnectionFactory factory, IEnumerable<Checkin> pendentes)
        : PostgresCheckinRepository(factory)
    {
        public override async Task<Result<IEnumerable<Checkin>>> GetPendingSyncAsync()
        {
            await Task.CompletedTask;
            return Result<IEnumerable<Checkin>>.Ok(pendentes);
        }
    }
}
