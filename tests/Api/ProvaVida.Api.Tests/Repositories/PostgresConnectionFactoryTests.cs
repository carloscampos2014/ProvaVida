using FluentAssertions;
using Npgsql;
using ProvaVida.Api.Infrastructure;

namespace ProvaVida.Api.Tests.Repositories;

/// <summary>
/// Testes unitários para <see cref="PostgresConnectionFactory"/>.
/// </summary>
public class PostgresConnectionFactoryTests
{
    [Fact]
    public void Create_DeveRetornarNpgsqlConnection()
    {
        // Arrange — usa uma connection string fictícia; a conexão não é aberta
        var factory = new PostgresConnectionFactory("Host=localhost;Database=test;Username=test;Password=test");

        // Act
        var connection = factory.Create();

        // Assert
        connection.Should().NotBeNull();
        connection.Should().BeOfType<NpgsqlConnection>();
    }

    [Fact]
    public void Create_DeveRetornarNovaInstanciaACadaChamada()
    {
        // Arrange
        var factory = new PostgresConnectionFactory("Host=localhost;Database=test;Username=test;Password=test");

        // Act
        var conn1 = factory.Create();
        var conn2 = factory.Create();

        // Assert — cada chamada deve retornar uma instância diferente
        conn1.Should().NotBeSameAs(conn2);
    }
}
