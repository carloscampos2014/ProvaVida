using FluentAssertions;
using ProvaVida.Mobile.Application.Utils;

namespace ProvaVida.Mobile.Tests.Utils;

/// <summary>
/// Testes unitários para <see cref="Sha256Helper"/>.
/// </summary>
public class Sha256HelperTests
{
    // Hash SHA-256 de "senha123" em hexadecimal lowercase (valor de referência).
    // Verificado via SHA256.ComputeHash no .NET runtime.
    private const string HashEsperadoSenha123 = "55a5e9e78207b4df8699d60886fa070079463547b095d1a05bc719bb4e6cd251";

    [Fact]
    public void Sha256Helper_DeveGerarHashCorreto()
    {
        // Arrange
        const string senha = "senha123";

        // Act
        var hash = Sha256Helper.ComputeHash(senha);

        // Assert
        hash.Should().Be(HashEsperadoSenha123);
    }

    [Fact]
    public void Sha256Helper_HashDeveSerDiferenteDaSenha()
    {
        // Arrange
        const string senha = "senha123";

        // Act
        var hash = Sha256Helper.ComputeHash(senha);

        // Assert
        hash.Should().NotBe(senha);
    }
}
