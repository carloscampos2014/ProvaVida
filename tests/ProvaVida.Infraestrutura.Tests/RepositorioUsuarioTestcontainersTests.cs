using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProvaVida.Dominio.Entidades;
using ProvaVida.Infraestrutura.Contexto;
using ProvaVida.Infraestrutura.Repositorios;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

/// <summary>
/// Teste de integração usando PostgreSQL via Testcontainers.
/// Priorize SQLite para integração contínua (CI). Este teste é opcional/local.
/// </summary>
[Trait("Categoria", "Opcional-Postgres")]
public class RepositorioUsuarioTestcontainersTests : IClassFixture<TestcontainersPostgresFixture>
{
    private readonly TestcontainersPostgresFixture _fixture;

    public RepositorioUsuarioTestcontainersTests(TestcontainersPostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private ProvaVidaDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<ProvaVidaDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        return new ProvaVidaDbContext(opcoes);
    }

    [Fact]
    public async Task AdicionarAsync_DeveSalvarUsuarioNoPostgresComSucesso()
    {
        using var contexto = CriarContexto();
        await contexto.Database.EnsureCreatedAsync();
        var repositorio = new RepositorioUsuario(contexto);
        var usuario = Usuario.Criar("Maria Teste", "maria@teste.com", "11999999999", "hash");
        await repositorio.AdicionarAsync(usuario);
        await contexto.SaveChangesAsync();
        var usuarioRecuperado = await repositorio.ObterPorIdAsync(usuario.Id);
        Assert.NotNull(usuarioRecuperado);
        Assert.Equal("Maria Teste", usuarioRecuperado.Nome);
    }
}
