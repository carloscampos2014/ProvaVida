using FluentAssertions;
using Moq;
using ProvaVida.Aplicacao.Dtos.Usuarios;
using ProvaVida.Aplicacao.Exceções;
using ProvaVida.Aplicacao.Servicos;
using ProvaVida.Aplicacao.Tests.Helpers;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Enums;
using ProvaVida.Dominio.ObjetosValor;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Servicos;
using Xunit;

namespace ProvaVida.Aplicacao.Tests.Servicos;

/// <summary>
/// Testes unitários para AutenticacaoService.
/// Valida: registro, autenticação, verificação de email duplicado.
/// </summary>
public class AutenticacaoServiceTests
{
    private readonly Mock<IRepositorioUsuario> _repositorioUsuarioMock;
    private readonly Mock<IServicoHashSenha> _servicoHashSenhaMock;
    private readonly IAutenticacaoService _servico;

    public AutenticacaoServiceTests()
    {
        _repositorioUsuarioMock = RepositorioMocks.CriarRepositorioUsuarioMock();
        _servicoHashSenhaMock = RepositorioMocks.CriarServicoHashSenhaMock();
        
        _servico = new AutenticacaoService(
            _repositorioUsuarioMock.Object,
            _servicoHashSenhaMock.Object
        );
    }

    #region RegistrarAsync

    /// <summary>
    /// Deve registrar um novo usuário com dados válidos.
    /// </summary>
    [Fact]
    public async Task RegistrarAsync_ComDadosValidos_DeveCriarUsuario()
    {
        // Arrange
        var dto = new UsuarioRegistroDto
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Telefone = "11987654321",
            Senha = "SenhaForte123!"
        };

        _repositorioUsuarioMock
            .Setup(r => r.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        _servicoHashSenhaMock
            .Setup(s => s.Hashar(dto.Senha))
            .Returns("hash_bcrypt_senhaforte_12rounds");

        _repositorioUsuarioMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _servico.RegistrarAsync(dto, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Email.Should().Be(dto.Email);
        resultado.Nome.Should().Be(dto.Nome);
        
        _repositorioUsuarioMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _servicoHashSenhaMock.Verify(
            s => s.Hashar(dto.Senha),
            Times.Once);
    }

    /// <summary>
    /// Deve lançar exceção ao registrar com email duplicado.
    /// </summary>
    [Fact]
    public async Task RegistrarAsync_ComEmailDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var usuarioExistente = RepositorioMocks.CriarUsuarioValido(email: "existente@teste.com");
        
        var dto = new UsuarioRegistroDto
        {
            Nome = "Novo Usuário",
            Email = "existente@teste.com",
            Telefone = "11987654321",
            Senha = "SenhaForte123!"
        };

        _repositorioUsuarioMock
            .Setup(r => r.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        // Act & Assert
        await Assert.ThrowsAsync<UsuarioJaExisteException>(
            () => _servico.RegistrarAsync(dto, CancellationToken.None));
    }

    /// <summary>
    /// Deve lançar exceção ao registrar com email vazio.
    /// </summary>
    [Fact]
    public async Task RegistrarAsync_ComEmailVazio_DeveLancarExcecao()
    {
        // Arrange
        var dto = new UsuarioRegistroDto
        {
            Nome = "Usuário",
            Email = string.Empty,
            Telefone = "11987654321",
            Senha = "SenhaForte123!"
        };

        // Act & Assert
        // O service lança AplicacaoException quando dados inválidos
        await Assert.ThrowsAsync<AplicacaoException>(
            () => _servico.RegistrarAsync(dto, CancellationToken.None));
    }

    #endregion

    #region AutenticarAsync

    /// <summary>
    /// Deve autenticar usuário com credenciais válidas.
    /// </summary>
    [Fact]
    public async Task AutenticarAsync_ComCredenciaisValidas_DeveRetornarUsuario()
    {
        // Arrange
        var usuarioExistente = RepositorioMocks.CriarUsuarioValido(
            email: "usuario@teste.com"
        );
        
        var dto = new UsuarioLoginDto
        {
            Email = "usuario@teste.com",
            Senha = "SenhaCorreta123"
        };

        _repositorioUsuarioMock
            .Setup(r => r.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        _servicoHashSenhaMock
            .Setup(s => s.Verificar(dto.Senha, usuarioExistente.SenhaHash))
            .Returns(true);

        // Act
        var resultado = await _servico.AutenticarAsync(dto, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Email.Should().Be(dto.Email);
        resultado.Nome.Should().Be(usuarioExistente.Nome);
    }

    /// <summary>
    /// Deve lançar exceção ao autenticar com senha inválida.
    /// </summary>
    [Fact]
    public async Task AutenticarAsync_ComSenhaInvalida_DeveLancarExcecao()
    {
        // Arrange
        var usuarioExistente = RepositorioMocks.CriarUsuarioValido(
            email: "usuario@teste.com"
        );
        
        var dto = new UsuarioLoginDto
        {
            Email = "usuario@teste.com",
            Senha = "SenhaErrada123"
        };

        _repositorioUsuarioMock
            .Setup(r => r.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        _servicoHashSenhaMock
            .Setup(s => s.Verificar(dto.Senha, usuarioExistente.SenhaHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<SenhaInvalidaException>(
            () => _servico.AutenticarAsync(dto, CancellationToken.None));
    }

    /// <summary>
    /// Deve lançar exceção ao autenticar com usuário inexistente.
    /// </summary>
    [Fact]
    public async Task AutenticarAsync_UsuarioNaoExistente_DeveLancarExcecao()
    {
        // Arrange
        var dto = new UsuarioLoginDto
        {
            Email = "inexistente@teste.com",
            Senha = "QualquerSenha123"
        };

        _repositorioUsuarioMock
            .Setup(r => r.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(
            () => _servico.AutenticarAsync(dto, CancellationToken.None));
    }

    #endregion

    #region EmailJaExisteAsync

    /// <summary>
    /// Deve retornar verdadeiro quando email já existe.
    /// </summary>
    [Fact]
    public async Task EmailJaExisteAsync_ComEmailExistente_DeveRetornarVerdadeiro()
    {
        // Arrange
        var usuarioExistente = RepositorioMocks.CriarUsuarioValido(
            email: "existente@teste.com"
        );

        _repositorioUsuarioMock
            .Setup(r => r.ObterPorEmailAsync("existente@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioExistente);

        // Act
        var resultado = await _servico.EmailJaExisteAsync("existente@teste.com", CancellationToken.None);

        // Assert
        resultado.Should().BeTrue();
    }

    /// <summary>
    /// Deve retornar falso quando email não existe.
    /// </summary>
    [Fact]
    public async Task EmailJaExisteAsync_ComEmailNovoAsync_DeveRetornarFalso()
    {
        // Arrange
        _repositorioUsuarioMock
            .Setup(r => r.ObterPorEmailAsync("novo@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _servico.EmailJaExisteAsync("novo@teste.com", CancellationToken.None);

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion
}
