using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.ObjetosValor;
using ProvaVida.Infraestrutura.Contexto;
using ProvaVida.Infraestrutura.Repositorios;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

public class RepositorioUsuarioTests
{
    private ProvaVidaDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<ProvaVidaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProvaVidaDbContext(opcoes);
    }

    [Fact]
    public async Task AdicionarAsync_DeveSalvarUsuarioComSucesso()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioUsuario(contexto);
        var usuario = Usuario.Criar("João Silva", "joao@example.com", "11987654321", "senhaHashBcrypt");

        // Act
        await repositorio.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        // Assert
        var usuarioRecuperado = await repositorio.ObterPorIdAsync(usuario.Id);
        Assert.NotNull(usuarioRecuperado);
        Assert.Equal("João Silva", usuarioRecuperado.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveDevolverNuloQuandoNaoExiste()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioUsuario(contexto);
        var idInexistente = Guid.NewGuid();

        // Act
        var resultado = await repositorio.ObterPorIdAsync(idInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodosOsUsuarios()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioUsuario(contexto);
        
        var usuario1 = Usuario.Criar("User 1", "user1@example.com", "11987654321", "hash1");
        var usuario2 = Usuario.Criar("User 2", "user2@example.com", "21987654321", "hash2");
        var usuario3 = Usuario.Criar("User 3", "user3@example.com", "31987654321", "hash3");

        await repositorio.AdicionarAsync(usuario1);
        await repositorio.AdicionarAsync(usuario2);
        await repositorio.AdicionarAsync(usuario3);
        await contexto.SaveChangesAsync();

        // Act
        var usuarios = await repositorio.ObterTodosAsync();

        // Assert
        Assert.True(usuarios.Count >= 3);
    }

    [Fact]
    public async Task ObterPorEmailAsync_DeveEncontrarUsuarioPorEmail()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioUsuario(contexto);
        var usuario = Usuario.Criar("Teste", "teste@example.com", "11987654321", "hash");

        await repositorio.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        // Act
        var usuarioEncontrado = await repositorio.ObterPorEmailAsync("teste@example.com");

        // Assert
        Assert.NotNull(usuarioEncontrado);
        Assert.Equal("Teste", usuarioEncontrado.Nome);
    }

    [Fact]
    public async Task ObterPorEmailAsync_DeveDevolverNuloQuandoEmailNaoExiste()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioUsuario(contexto);

        // Act
        var resultado = await repositorio.ObterPorEmailAsync("inexistente@example.com");

        // Assert
        Assert.Null(resultado);
    }
}
