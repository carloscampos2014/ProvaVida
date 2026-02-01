using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.ObjetosValor;
using ProvaVida.Infraestrutura.Contexto;
using ProvaVida.Infraestrutura.Repositorios;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

public class RepositorioContatoEmergenciaTests
{
    private ProvaVidaDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<ProvaVidaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProvaVidaDbContext(opcoes);
    }

    [Fact]
    public async Task AdicionarAsync_DeveSalvarContatoComSucesso()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioContatoEmergencia(contexto);
        var repositorioUsuario = new RepositorioUsuario(contexto);
        
        var usuario = Usuario.Criar("Teste", "teste@example.com", "11987654321", "hash");
        await repositorioUsuario.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        var contato = ContatoEmergencia.Criar(usuario.Id, "Mãe", "mae@example.com", "11999999999");

        // Act
        await repositorio.AdicionarAsync(contato);
        await contexto.SaveChangesAsync();

        // Assert
        var recuperado = await repositorio.ObterPorIdAsync(contato.Id);
        Assert.NotNull(recuperado);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodosOsContatos()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioContatoEmergencia(contexto);
        var repositorioUsuario = new RepositorioUsuario(contexto);
        
        var usuario = Usuario.Criar("Teste", "teste@example.com", "11987654321", "hash");
        await repositorioUsuario.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        var contato1 = ContatoEmergencia.Criar(usuario.Id, "Mãe", "mae@example.com", "11999999999");
        var contato2 = ContatoEmergencia.Criar(usuario.Id, "Irmão", "irmao@example.com", "21999999999");

        await repositorio.AdicionarAsync(contato1);
        await repositorio.AdicionarAsync(contato2);
        await contexto.SaveChangesAsync();

        // Act
        var contatos = await repositorio.ObterTodosAsync();

        // Assert
        Assert.True(contatos.Count >= 2);
    }
}
