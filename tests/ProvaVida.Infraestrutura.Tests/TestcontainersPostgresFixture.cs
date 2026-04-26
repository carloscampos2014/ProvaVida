using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using ProvaVida.Infraestrutura.Contexto;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

public class TestcontainersPostgresFixture : IAsyncLifetime
{
    public TestcontainerDatabaseContainer Container { get; private set; } = default!;
    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync()
    {
        Container = new TestcontainersBuilder<TestcontainerDatabaseContainer>()
            .WithDatabase(new TestcontainersDatabaseConfiguration("postgres", "postgres", "postgres"))
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
