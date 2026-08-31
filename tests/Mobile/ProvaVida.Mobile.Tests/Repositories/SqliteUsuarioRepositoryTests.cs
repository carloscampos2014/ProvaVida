using FluentAssertions;
using Moq;
using ProvaVida.Mobile.Infrastructure.Repositories;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Tests.Repositories;

/// <summary>
/// Testes unitários para <see cref="SqliteUsuarioRepository"/>.
/// </summary>
public class SqliteUsuarioRepositoryTests
{
    // ── TDD obrigatório ────────────────────────────────────────────────────

    [Fact]
    public async Task SqliteUsuarioRepository_GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var repo = new FakeNotFoundUsuarioRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().NotBeNullOrWhiteSpace();
    }

    // ── Caminhos de sucesso (via fakes) ────────────────────────────────────

    [Fact]
    public async Task UpsertAsync_DeveRetornarSuccess_QuandoConexaoOk()
    {
        var repo = new FakeSuccessUsuarioRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.UpsertAsync(CriarUsuarioValido());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarSuccess_QuandoConexaoOk()
    {
        var repo = new FakeSuccessUsuarioRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.DeleteAsync(Guid.NewGuid());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarSuccess_ComListaVazia_QuandoConexaoOk()
    {
        var repo = new FakeEmptyListUsuarioRepository(new Mock<IDbConnectionFactory>().Object);

        var result = await repo.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    // ── Caminhos de erro — exercita o bloco catch do DapperRepository ──────

    [Fact]
    public async Task GetByIdAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new SqliteUsuarioRepository(factory);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new SqliteUsuarioRepository(factory);

        var result = await repo.GetAllAsync();

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    [Fact]
    public async Task UpsertAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new SqliteUsuarioRepository(factory);

        var result = await repo.UpsertAsync(CriarUsuarioValido());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarFail_QuandoConexaoLancaExcecao()
    {
        var factory = CriarFactoryQueExplode();
        var repo = new SqliteUsuarioRepository(factory);

        var result = await repo.DeleteAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Contain("Conexão inválida");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Cria uma factory cujo <c>Create()</c> lança <see cref="InvalidOperationException"/>,
    /// forçando a execução dos blocos <c>catch</c> do <see cref="DapperRepository{T}"/>.
    /// </summary>
    private static IDbConnectionFactory CriarFactoryQueExplode()
    {
        var mock = new Mock<IDbConnectionFactory>();
        mock.Setup(f => f.Create())
            .Throws(new InvalidOperationException("Conexão inválida"));
        return mock.Object;
    }

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

    // ── Fakes de sucesso ───────────────────────────────────────────────────

    private class FakeNotFoundUsuarioRepository(IDbConnectionFactory factory)
        : SqliteUsuarioRepository(factory)
    {
        public override async Task<Result<Usuario>> GetByIdAsync(Guid id)
        {
            await Task.CompletedTask;
            return Result<Usuario>.Fail("Entidade não encontrada");
        }
    }

    private class FakeSuccessUsuarioRepository(IDbConnectionFactory factory)
        : SqliteUsuarioRepository(factory)
    {
        public override async Task<Result> UpsertAsync(Usuario entity)
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

    private class FakeEmptyListUsuarioRepository(IDbConnectionFactory factory)
        : SqliteUsuarioRepository(factory)
    {
        public override async Task<Result<IEnumerable<Usuario>>> GetAllAsync()
        {
            await Task.CompletedTask;
            return Result<IEnumerable<Usuario>>.Ok([]);
        }
    }
}
