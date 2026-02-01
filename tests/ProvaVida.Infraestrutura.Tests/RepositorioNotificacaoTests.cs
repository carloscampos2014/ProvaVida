using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.Enums;
using ProvaVida.Dominio.ObjetosValor;
using ProvaVida.Infraestrutura.Contexto;
using ProvaVida.Infraestrutura.Repositorios;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

public class RepositorioNotificacaoTests
{
    private ProvaVidaDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<ProvaVidaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProvaVidaDbContext(opcoes);
    }

    [Fact]
    public async Task AdicionarAsync_DeveSalvarNotificacaoComSucesso()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioNotificacao(contexto);
        var repositorioUsuario = new RepositorioUsuario(contexto);
        
        var usuario = Usuario.Criar("Teste", "teste@example.com", "11987654321", "hash");
        await repositorioUsuario.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        var notificacao = Notificacao.CriarLembreteUsuario(usuario.Id, MeioNotificacao.Email);

        // Act
        await repositorio.AdicionarAsync(notificacao);
        await contexto.SaveChangesAsync();

        // Assert
        var recuperada = await repositorio.ObterPorIdAsync(notificacao.Id);
        Assert.NotNull(recuperada);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodasAsNotificacoes()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioNotificacao(contexto);
        var repositorioUsuario = new RepositorioUsuario(contexto);
        
        var usuario = Usuario.Criar("Teste", "teste@example.com", "11987654321", "hash");
        await repositorioUsuario.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        var notificacao1 = Notificacao.CriarLembreteUsuario(usuario.Id, MeioNotificacao.Email);
        var notificacao2 = Notificacao.CriarLembreteUsuario(usuario.Id, MeioNotificacao.WhatsApp);

        await repositorio.AdicionarAsync(notificacao1);
        await repositorio.AdicionarAsync(notificacao2);
        await contexto.SaveChangesAsync();

        // Act
        var notificacoes = await repositorio.ObterTodosAsync();

        // Assert
        Assert.True(notificacoes.Count >= 2);
    }
}
