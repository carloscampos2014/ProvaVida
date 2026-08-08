using FluentAssertions;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.Tests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Criar_ComDadosValidos_RetornaUsuarioAtivo()
    {
        var usuario = Usuario.Criar("João Silva", "joao@email.com", "11999999999",
            "hash", "Maria", "maria@email.com", "11888888888");

        usuario.Nome.Should().Be("João Silva");
        usuario.Email.Should().Be("joao@email.com");
        usuario.Ativo.Should().BeTrue();
        usuario.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Criar_NormalizeEmailParaLowercase()
    {
        var usuario = Usuario.Criar("João", "JOAO@EMAIL.COM", "11999999999",
            "hash", "Maria", "MARIA@EMAIL.COM", "11888888888");

        usuario.Email.Should().Be("joao@email.com");
        usuario.ContatoEmergenciaEmail.Should().Be("maria@email.com");
    }

    [Fact]
    public void AtualizarDados_AlteraPropriedadesCorretamente()
    {
        var usuario = Usuario.Criar("João", "joao@email.com", "11999999999",
            "hash", "Maria", "maria@email.com", "11888888888");

        usuario.AtualizarDados("João Atualizado", "11777777777",
            "Pedro", "pedro@email.com", "11666666666");

        usuario.Nome.Should().Be("João Atualizado");
        usuario.WhatsApp.Should().Be("11777777777");
        usuario.ContatoEmergenciaNome.Should().Be("Pedro");
    }

    [Fact]
    public void Anonimizar_SubstituiDadosPessoais()
    {
        var usuario = Usuario.Criar("João Silva", "joao@email.com", "11999999999",
            "hash_seguro", "Maria", "maria@email.com", "11888888888");

        usuario.Anonimizar();

        usuario.Nome.Should().StartWith("[removido-");
        usuario.Email.Should().EndWith("@anonimizado.invalid");
        usuario.WhatsApp.Should().Be("[removido]");
        usuario.SenhaHash.Should().BeEmpty();
        usuario.Ativo.Should().BeFalse();
    }
}
