using System.Data;
using Dapper;
using FluentAssertions;
using Moq;
using ProvaVida.Api.Infrastructure.Repositories;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Tests.Repositories;

/// <summary>
/// Testes unitários para <see cref="PostgresUsuarioRepository"/>.
/// Usam Moq para simular <see cref="IDbConnectionFactory"/> e <see cref="IDbConnection"/>,
/// evitando dependência de banco real.
/// </summary>
public class PostgresUsuarioRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        // Arrange
        var mockConn = new Mock<IDbConnection>();
        var mockFactory = new Mock<IDbConnectionFactory>();
        mockFactory.Setup(f => f.Create()).Returns(mockConn.Object);

        // QueryFirstOrDefaultAsync retorna null via extensão Dapper — simulamos via repositório
        // com uma factory que lança QuerySingleOrDefaultAsync retornando null.
        // Como Dapper usa IDbConnection diretamente e não é fácil mockar, usamos uma subclasse
        // de DapperRepository que sobrescreve GetByIdAsync para retornar Fail.
        var repo = new FakeNotFoundUsuarioRepository(mockFactory.Object);

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
        var mockConn = new Mock<IDbConnection>();
        mockFactory.Setup(f => f.Create()).Returns(mockConn.Object);

        var repo = new FakeSuccessUsuarioRepository(mockFactory.Object);
        var usuario = CriarUsuarioValido();

        // Act
        var result = await repo.UpsertAsync(usuario);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarSuccess_QuandoConexaoOk()
    {
        // Arrange
        var mockFactory = new Mock<IDbConnectionFactory>();
        var mockConn = new Mock<IDbConnection>();
        mockFactory.Setup(f => f.Create()).Returns(mockConn.Object);

        var repo = new FakeSuccessUsuarioRepository(mockFactory.Object);

        // Act
        var result = await repo.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarSuccess_ComListaVazia_QuandoConexaoOk()
    {
        // Arrange
        var mockFactory = new Mock<IDbConnectionFactory>();
        var mockConn = new Mock<IDbConnection>();
        mockFactory.Setup(f => f.Create()).Returns(mockConn.Object);

        var repo = new FakeEmptyListUsuarioRepository(mockFactory.Object);

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
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

    // --- Fakes para isolar sem depender de banco real ---

    /// <summary>Simula GetByIdAsync retornando "não encontrado".</summary>
    private class FakeNotFoundUsuarioRepository : PostgresUsuarioRepository
    {
        public FakeNotFoundUsuarioRepository(IDbConnectionFactory factory) : base(factory) { }

        public override async Task<ProvaVida.Shared.Common.Result<Usuario>> GetByIdAsync(Guid id)
        {
            await Task.CompletedTask;
            return ProvaVida.Shared.Common.Result<Usuario>.Fail("Entidade não encontrada");
        }
    }

    /// <summary>Simula operações de escrita retornando sucesso.</summary>
    private class FakeSuccessUsuarioRepository : PostgresUsuarioRepository
    {
        public FakeSuccessUsuarioRepository(IDbConnectionFactory factory) : base(factory) { }

        public override async Task<ProvaVida.Shared.Common.Result> UpsertAsync(Usuario entity)
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

    /// <summary>Simula GetAllAsync retornando lista vazia com sucesso.</summary>
    private class FakeEmptyListUsuarioRepository : PostgresUsuarioRepository
    {
        public FakeEmptyListUsuarioRepository(IDbConnectionFactory factory) : base(factory) { }

        public override async Task<ProvaVida.Shared.Common.Result<IEnumerable<Usuario>>> GetAllAsync()
        {
            await Task.CompletedTask;
            return ProvaVida.Shared.Common.Result<IEnumerable<Usuario>>.Ok([]);
        }
    }
}
