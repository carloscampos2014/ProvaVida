using FluentAssertions;
using Moq;
using ProvaVida.Aplicacao.Dtos.CheckIns;
using ProvaVida.Aplicacao.Exceções;
using ProvaVida.Aplicacao.Servicos;
using ProvaVida.Aplicacao.Tests.Helpers;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Enums;
using ProvaVida.Dominio.Repositorios;
using Xunit;

namespace ProvaVida.Aplicacao.Tests.Servicos;

/// <summary>
/// Testes unitários para CheckInService.
/// Valida: registro de check-in, histórico, limpeza de notificações.
/// </summary>
public class CheckInServiceTests
{
    private readonly Mock<IRepositorioUsuario> _repositorioUsuarioMock;
    private readonly Mock<IRepositorioCheckIn> _repositorioCheckInMock;
    private readonly Mock<IRepositorioNotificacao> _repositorioNotificacaoMock;
    private readonly ICheckInService _servico;

    public CheckInServiceTests()
    {
        _repositorioUsuarioMock = RepositorioMocks.CriarRepositorioUsuarioMock();
        _repositorioCheckInMock = RepositorioMocks.CriarRepositorioCheckInMock();
        _repositorioNotificacaoMock = RepositorioMocks.CriarRepositorioNotificacaoMock();
        
        _servico = new CheckInService(
            _repositorioUsuarioMock.Object,
            _repositorioCheckInMock.Object,
            _repositorioNotificacaoMock.Object
        );
    }

    #region RegistrarCheckInAsync

    /// <summary>
    /// Deve registrar um novo check-in com dados válidos.
    /// </summary>
    [Fact]
    public async Task RegistrarCheckInAsync_ComDadosValidos_DeveRegistrar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = RepositorioMocks.CriarUsuarioValido();
        
        var dto = new CheckInRegistroDto
        {
            UsuarioId = usuarioId,
            Localizacao = "-23.5505,-46.6333"
        };

        _repositorioUsuarioMock
            .Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _repositorioNotificacaoMock
            .Setup(r => r.ObterPorUsuarioIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notificacao>());

        // CheckInService chama AtualizarAsync do usuario, não AdicionarAsync
        _repositorioUsuarioMock
            .Setup(r => r.AtualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _servico.RegistrarCheckInAsync(dto, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.UsuarioId.Should().NotBeEmpty();

        // Verifica que usuario foi atualizado
        _repositorioUsuarioMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Deve lançar exceção ao tentar registrar check-in para usuário inexistente.
    /// </summary>
    [Fact]
    public async Task RegistrarCheckInAsync_UsuarioNaoExistenteOuInativo_DeveLancarExcecao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        
        var dto = new CheckInRegistroDto
        {
            UsuarioId = usuarioId,
            Localizacao = "-23.5505,-46.6333"
        };

        // Simular usuário inativo retornando null (como seria em um caso de exclusão/inativação)
        _repositorioUsuarioMock
            .Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(
            () => _servico.RegistrarCheckInAsync(dto, CancellationToken.None));
    }

    #endregion

    #region ObterHistoricoAsync

    /// <summary>
    /// Deve retornar os últimos 5 check-ins do usuário.
    /// </summary>
    [Fact]
    public async Task ObterHistoricoAsync_DeveRetornarUltimos5CheckIns()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var checkIns = new List<CheckIn>
        {
            RepositorioMocks.CriarCheckInValido(usuarioId, DateTime.UtcNow.AddDays(-4)),
            RepositorioMocks.CriarCheckInValido(usuarioId, DateTime.UtcNow.AddDays(-3)),
            RepositorioMocks.CriarCheckInValido(usuarioId, DateTime.UtcNow.AddDays(-2)),
            RepositorioMocks.CriarCheckInValido(usuarioId, DateTime.UtcNow.AddDays(-1)),
            RepositorioMocks.CriarCheckInValido(usuarioId, DateTime.UtcNow)
        };

        _repositorioCheckInMock
            .Setup(r => r.ObterHistoricoAsync(usuarioId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkIns);

        // Act
        var resultado = await _servico.ObterHistoricoAsync(usuarioId, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(5);
        
        _repositorioCheckInMock.Verify(
            r => r.ObterHistoricoAsync(usuarioId, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Deve retornar lista vazia quando usuário não tem histórico.
    /// </summary>
    [Fact]
    public async Task ObterHistoricoAsync_SemHistorico_DeveRetornarListaVazia()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        _repositorioCheckInMock
            .Setup(r => r.ObterHistoricoAsync(usuarioId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheckIn>());

        // Act
        var resultado = await _servico.ObterHistoricoAsync(usuarioId, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    #endregion

    #region Notificações

    /// <summary>
    /// Deve processar check-in e notificações corretamente.
    /// </summary>
    [Fact]
    public async Task RegistrarCheckInAsync_DeveProcessarNotificacoes()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = RepositorioMocks.CriarUsuarioValido();
        
        var dto = new CheckInRegistroDto
        {
            UsuarioId = usuarioId,
            Localizacao = "-23.5505,-46.6333"
        };

        // Setup: usar o ID gerado do usuario, não o do DTO
        _repositorioUsuarioMock
            .Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Setup notification lookup com ID do usuario real (não o do DTO)
        _repositorioNotificacaoMock
            .Setup(r => r.ObterPorUsuarioIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notificacao>());

        _repositorioUsuarioMock
            .Setup(r => r.AtualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _servico.RegistrarCheckInAsync(dto, CancellationToken.None);

        // Assert
        // Verifica que o repositório de notificações foi consultado com o ID do usuario
        _repositorioNotificacaoMock.Verify(
            r => r.ObterPorUsuarioIdAsync(usuario.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
