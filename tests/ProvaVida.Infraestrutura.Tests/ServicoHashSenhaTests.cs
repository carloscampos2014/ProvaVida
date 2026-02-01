using ProvaVida.Infraestrutura.Servicos;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

public class ServicoHashSenhaTests
{
    [Fact]
    public void Hashar_DeveProduzireshaDiferenteDaSenha()
    {
        // Arrange
        var servico = new ServicoHashSenha();
        var senha = "MinhaSenh@Forte123";

        // Act
        var hash = servico.Hashar(senha);

        // Assert
        Assert.NotEqual(senha, hash);
        Assert.True(hash.Length > 0);
    }

    [Fact]
    public void Verificar_DeveRetornarVerdadeiroPara_SenhaCorreta()
    {
        // Arrange
        var servico = new ServicoHashSenha();
        var senha = "SenhaCorreta123!";
        var hash = servico.Hashar(senha);

        // Act
        var resultado = servico.Verificar(senha, hash);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void Verificar_DeveRetornarFalsoPara_SenhaIncorreta()
    {
        // Arrange
        var servico = new ServicoHashSenha();
        var senha = "SenhaCorreta123!";
        var senhaErrada = "SenhaErrada123!";
        var hash = servico.Hashar(senha);

        // Act
        var resultado = servico.Verificar(senhaErrada, hash);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void Hashar_DeveProduzireshDiferentesCadaVez()
    {
        // Arrange
        var servico = new ServicoHashSenha();
        var senha = "Mesma@Senha789";

        // Act
        var hash1 = servico.Hashar(senha);
        var hash2 = servico.Hashar(senha);

        // Assert
        Assert.NotEqual(hash1, hash2);
        // Mas ambos devem validar a mesma senha
        Assert.True(servico.Verificar(senha, hash1));
        Assert.True(servico.Verificar(senha, hash2));
    }
}
