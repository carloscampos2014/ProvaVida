using Xunit;
using ProvaVida.Dominio.ObjetosValor;

namespace ProvaVida.Dominio.Tests.ObjetosValor;

/// <summary>
/// Testes para o Value Object Telefone.
/// Valida criação, normalização (formato brasileiro) e comportamento de igualdade.
/// </summary>
public class TelefoneTests
{
    [Fact]
    public void Criar_ComTelefoneValido_DeveSuceder()
    {
        // Arrange & Act
        var telefone = new Telefone("11987654321");

        // Assert
        Assert.Equal("11987654321", telefone.Valor);
    }

    [Fact]
    public void Criar_ComTelefoneFormatado_DeveExtrairApenasDígitos()
    {
        // Arrange & Act
        var telefone = new Telefone("(11)98765-4321");

        // Assert
        Assert.Equal("11987654321", telefone.Valor);
    }

    [Fact]
    public void Criar_ComTelefoneComEspacos_DeveRemoverEspacos()
    {
        // Arrange & Act
        var telefone = new Telefone("  11 98765-4321  ");

        // Assert
        Assert.Equal("11987654321", telefone.Valor);
    }

    [Fact]
    public void Criar_ComTelefoneFormatadoComPrefixoPais_DeveSuceder()
    {
        // Arrange & Act
        var telefone = new Telefone("+55 (11) 98765-4321");

        // Assert
        Assert.NotNull(telefone.Valor);
        Assert.Contains("11", telefone.Valor);
    }

    [Fact]
    public void Criar_ComTelefoneNulo_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Telefone(null!));
    }

    [Fact]
    public void Criar_ComTelefoneVazio_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Telefone(""));
    }

    [Fact]
    public void Criar_ComTelefoneComPoucosDigitos_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Telefone("1234567"));
    }

    [Fact]
    public void Criar_ComTelefoneInvalido_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Telefone("abc123def"));
    }

    [Fact]
    public void Equals_ComDoisTelefones_IguaisDeve_RetornarTrue()
    {
        // Arrange
        var tel1 = new Telefone("11987654321");
        var tel2 = new Telefone("(11)98765-4321");

        // Act & Assert
        Assert.True(tel1.Equals(tel2));
        Assert.Equal(tel1, tel2);
    }

    [Fact]
    public void Equals_ComDoisTelefones_DifferentDeve_RetornarFalse()
    {
        // Arrange
        var tel1 = new Telefone("11987654321");
        var tel2 = new Telefone("11987654322");

        // Act & Assert
        Assert.False(tel1.Equals(tel2));
        Assert.NotEqual(tel1, tel2);
    }

    [Fact]
    public void OperadorIgualdade_ComDoisTelefones_IguaisDeve_RetornarTrue()
    {
        // Arrange
        var tel1 = new Telefone("11987654321");
        var tel2 = new Telefone("(11)98765-4321");

        // Act & Assert
        Assert.True(tel1 == tel2);
    }

    [Fact]
    public void OperadorDesigualdade_ComDoisTelefones_DifferentDeve_RetornarTrue()
    {
        // Arrange
        var tel1 = new Telefone("11987654321");
        var tel2 = new Telefone("11987654322");

        // Act & Assert
        Assert.True(tel1 != tel2);
    }

    [Fact]
    public void ToString_DeveRetornarValorFormatado()
    {
        // Arrange
        var telefone = new Telefone("(11)98765-4321");

        // Act
        var resultado = telefone.ToString();

        // Assert
        Assert.Equal("(11)98765-4321", resultado);
    }

    [Fact]
    public void GetHashCode_ComDoisTelefones_IguaisDeve_RetornarMesmoHash()
    {
        // Arrange
        var tel1 = new Telefone("11987654321");
        var tel2 = new Telefone("(11)98765-4321");

        // Act & Assert
        Assert.Equal(tel1.GetHashCode(), tel2.GetHashCode());
    }
}
