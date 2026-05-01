using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using ProvaVida.Infraestrutura.Contexto;
using Xunit;

namespace ProvaVida.Infraestrutura.Tests;

/// <summary>
/// Fixture para testes de integração com PostgreSQL via Testcontainers.
/// Priorize SQLite para CI/CD. Este é opcional e para ambiente local.
/// </summary>
public class TestcontainersPostgresFixture : IAsyncLifetime
{
    private IContainer? _container;

    public string? ConnectionString { get; private set; }
    public bool IsDockerAvailable { get; private set; } = true;

    public async Task InitializeAsync()
    {
        try
        {
            _container = new ContainerBuilder()
                .WithImage("postgres:16-alpine")
                .WithEnvironment("POSTGRES_PASSWORD", "postgres")
                .WithEnvironment("POSTGRES_USER", "postgres")
                .WithEnvironment("POSTGRES_DB", "provavida_test")
                .WithPortBinding(5432, 5432)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();

            await _container.StartAsync();
            ConnectionString = "Host=localhost;Port=5432;Database=provavida_test;Username=postgres;Password=postgres";
        }
        catch (ArgumentException)
        {
            IsDockerAvailable = false;
            ConnectionString = null;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
            await _container.DisposeAsync();
    }
}
