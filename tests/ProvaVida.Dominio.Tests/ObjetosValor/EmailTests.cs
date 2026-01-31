using Xunit;
using ProvaVida.Dominio.ObjetosValor;

namespace ProvaVida.Dominio.Tests.ObjetosValor;

/// <summary>
/// Testes para o Value Object Email.
/// Valida criação, normalização e comportamento de igualdade.
/// </summary>
public class EmailTests
{
    [Fact]
    public void Criar_ComEmailValido_DeveSuceder()
    {
        // Arrange & Act
        var email = new Email("usuario@example.com");

        // Assert
        Assert.Equal("usuario@example.com", email.Valor);
    }

    [Fact]
    public void Criar_ComEmailValido_DeveNormalizarParaMinusculas()
    {
        // Arrange & Act
        var email = new Email("USUARIO@EXAMPLE.COM");

        // Assert
        Assert.Equal("usuario@example.com", email.Valor);
    }

    [Fact]
    public void Criar_ComEmailComEspacos_DeveRemoverEspacos()
    {
        // Arrange & Act
        var email = new Email("  usuario@example.com  ");

        // Assert
        Assert.Equal("usuario@example.com", email.Valor);
    }

    [Fact]
    public void Criar_ComEmailNulo_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(null!));
    }

    [Fact]
    public void Criar_ComEmailVazio_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(""));
    }

    [Fact]
    public void Criar_ComEmailSemArroba_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Email("usuarioexample.com"));
    }

    [Fact]
    public void Criar_ComEmailSemDominio_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Email("usuario@"));
    }

    [Fact]
    public void Criar_ComEmailSemExtensao_DeveLancarException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new Email("usuario@example"));
    }

    [Fact]
    public void Equals_ComDoiEmails_IguaisDeve_RetornarTrue()
    {
        // Arrange
        var email1 = new Email("usuario@example.com");
        var email2 = new Email("usuario@example.com");

        // Act & Assert
        Assert.True(email1.Equals(email2));
        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Equals_ComDoiEmails_DifferentDeve_RetornarFalse()
    {
        // Arrange
        var email1 = new Email("usuario1@example.com");
        var email2 = new Email("usuario2@example.com");

        // Act & Assert
        Assert.False(email1.Equals(email2));
        Assert.NotEqual(email1, email2);
    }

    [Fact]
    public void OperadorIgualdade_ComDoiEmails_IguaisDeve_RetornarTrue()
    {
        // Arrange
        var email1 = new Email("usuario@example.com");
        var email2 = new Email("usuario@example.com");

        // Act & Assert
        Assert.True(email1 == email2);
    }

    [Fact]
    public void OperadorDesigualdade_ComDoiEmails_DifferentDeve_RetornarTrue()
    {
        // Arrange
        var email1 = new Email("usuario1@example.com");
        var email2 = new Email("usuario2@example.com");

        // Act & Assert
        Assert.True(email1 != email2);
    }

    [Fact]
    public void ToString_DeveRetornarValorDoEmail()
    {
        // Arrange
        var email = new Email("usuario@example.com");

        // Act
        var resultado = email.ToString();

        // Assert
        Assert.Equal("usuario@example.com", resultado);
    }

    [Fact]
    public void GetHashCode_ComDoiEmails_IguaisDeve_RetornarMesmoHash()
    {
        // Arrange
        var email1 = new Email("usuario@example.com");
        var email2 = new Email("usuario@example.com");

        // Act & Assert
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }
}
