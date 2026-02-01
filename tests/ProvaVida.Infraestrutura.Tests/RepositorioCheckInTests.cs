using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Dominio.ObjetosValor;
using ProvaVida.Infraestrutura.Contexto;
using ProvaVida.Infraestrutura.Repositorios;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

public class RepositorioCheckInTests
{
    private ProvaVidaDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<ProvaVidaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProvaVidaDbContext(opcoes);
    }

    [Fact]
    public async Task AdicionarAsync_DeveSalvarCheckInComSucesso()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioCheckIn(contexto);
        var repositorioUsuario = new RepositorioUsuario(contexto);
        
        var usuario = Usuario.Criar("Teste", "teste@example.com", "11987654321", "hash");
        await repositorioUsuario.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        var checkIn = CheckIn.Criar(usuario.Id, DateTime.UtcNow);

        // Act
        await repositorio.AdicionarAsync(checkIn);
        await contexto.SaveChangesAsync();

        // Assert
        var recuperado = await repositorio.ObterPorIdAsync(checkIn.Id);
        Assert.NotNull(recuperado);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodosOsCheckIns()
    {
        // Arrange
        using var contexto = CriarContexto();
        var repositorio = new RepositorioCheckIn(contexto);
        var repositorioUsuario = new RepositorioUsuario(contexto);
        
        var usuario = Usuario.Criar("Teste", "teste@example.com", "11987654321", "hash");
        await repositorioUsuario.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();

        var checkIn1 = CheckIn.Criar(usuario.Id);
        var checkIn2 = CheckIn.Criar(usuario.Id);

        await repositorio.AdicionarAsync(checkIn1);
        await repositorio.AdicionarAsync(checkIn2);
        await contexto.SaveChangesAsync();

        // Act
        var checkIns = await repositorio.ObterTodosAsync();

        // Assert
        Assert.True(checkIns.Count >= 2);
    }
}
