using FluentAssertions;
using Moq;
using ProvaVida.Admin.Infrastructure.Queries;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Admin.Tests.Queries;

/// <summary>
/// Testes unitários para <see cref="AdminUsuarioQueryService"/>.
/// Verifica que o serviço delega corretamente para o repositório
/// e nunca expõe operações de mutação.
/// </summary>
public class AdminUsuarioQueryServiceTests
{
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly AdminUsuarioQueryService _service;

    public AdminUsuarioQueryServiceTests()
    {
        _repoMock = new Mock<IUsuarioRepository>();
        _service = new AdminUsuarioQueryService(_repoMock.Object);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_DeveRetornarListaDeUsuarios_QuandoRepositorioRetornaOk()
    {
        var usuarios = new List<Usuario>
        {
            CriarUsuario("Alice"),
            CriarUsuario("Bob")
        };
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok(usuarios));

        var result = await _service.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarFail_QuandoRepositorioFalha()
    {
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Fail("Erro de conexão"));

        var result = await _service.GetAllAsync();

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Erro de conexão");
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_DeveRetornarUsuario_QuandoEncontrado()
    {
        var usuario = CriarUsuario("Carlos");
        _repoMock
            .Setup(r => r.GetByIdAsync(usuario.Id))
            .ReturnsAsync(Result<Usuario>.Ok(usuario));

        var result = await _service.GetByIdAsync(usuario.Id);

        result.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(usuario.Id);
        result.Data.Nome.Should().Be("Carlos");
        _repoMock.Verify(r => r.GetByIdAsync(usuario.Id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarFail_QuandoNaoEncontrado()
    {
        var id = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(Result<Usuario>.Fail("Entidade não encontrada"));

        var result = await _service.GetByIdAsync(id);

        result.Success.Should().BeFalse();
        result.MessageErro.Should().Be("Entidade não encontrada");
    }

    // ── Garantia de isolamento — repositório nunca mutado pelo serviço ────

    [Fact]
    public async Task GetAllAsync_NuncaDeveInvocarUpsertOuDelete()
    {
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<Usuario>>.Ok(Enumerable.Empty<Usuario>()));

        await _service.GetAllAsync();

        _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Usuario>()), Times.Never);
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Usuario CriarUsuario(string nome) => new()
    {
        Id = Guid.NewGuid(),
        Nome = nome,
        Email = $"{nome.ToLower()}@teste.com",
        Whatsapp = "11999999999",
        SenhaHash = new string('a', 64),
        ContatoEmergenciaNome = "Contato",
        ContatoEmergenciaEmail = "contato@teste.com",
        ContatoEmergenciaWhatsapp = "11988888888",
        CriadoEm = DateTimeOffset.UtcNow,
        AtualizadoEm = DateTimeOffset.UtcNow
    };
}
