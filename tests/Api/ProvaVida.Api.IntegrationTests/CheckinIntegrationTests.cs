using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using ProvaVida.Shared.Dtos;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProvaVida.Api.IntegrationTests;

/// <summary>
/// Testes de integração do endpoint POST /checkins.
/// Sobe a API real via WebApplicationFactory com PostgreSQL em container Docker.
/// </summary>
[Trait("Category", "Integration")]
public class CheckinIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private WebApplicationFactory<Program> _factory = null!;

    // Segredo JWT exclusivo destes testes (>= 32 chars para HMAC-SHA256)
    private const string TestJwtSecret = "checkin-integration-test-secret-min32c";

    public CheckinIntegrationTests()
    {
        var dockerEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST") ?? "tcp://localhost:2375";
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDockerEndpoint(dockerEndpoint)
            .WithDatabase("provavida_checkin_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    /// <summary>Sobe o container e a API com as env vars corretas antes dos testes.</summary>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Env vars precisam estar definidas ANTES de WebApplicationFactory construir o host,
        // pois Program.cs as lê via Environment.GetEnvironmentVariable.
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("JWT_SECRET", TestJwtSecret);

        _factory = new WebApplicationFactory<Program>();

        // Forçar a construção antecipada do host (executa migrations)
        _ = _factory.Server;
    }

    /// <summary>Para a API, destrói a factory e para o container ao final.</summary>
    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.StopAsync();

        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", null);
        Environment.SetEnvironmentVariable("JWT_SECRET", null);
    }

    // ────────────────────────── Cenário 1 ──────────────────────────

    [Fact]
    public async Task Post_Checkins_ComJwtValido_DeveRetornar201ComCheckinResponse()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var client = CriarClienteComJwt(usuarioId);
        var request = CheckinRequestDeTeste(DateOnly.FromDateTime(DateTime.Today));

        // Act
        var response = await client.PostAsJsonAsync("/checkins", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var checkin = await response.Content.ReadFromJsonAsync<CheckinResponse>();
        checkin.Should().NotBeNull();
        checkin!.UsuarioId.Should().Be(usuarioId);
        checkin.Data.Should().Be(request.Data);
        checkin.Latitude.Should().BeApproximately(request.Latitude, 0.0001);
        checkin.Longitude.Should().BeApproximately(request.Longitude, 0.0001);
        checkin.IdentificacaoAparelho.Should().Be(request.IdentificacaoAparelho);
        checkin.Sincronizado.Should().BeTrue();
    }

    // ────────────────────────── Cenário 2 ──────────────────────────

    [Fact]
    public async Task Post_Checkins_SemJwt_DeveRetornar401()
    {
        // Arrange — cliente sem Authorization header
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var request = CheckinRequestDeTeste(DateOnly.FromDateTime(DateTime.Today));

        // Act
        var response = await client.PostAsJsonAsync("/checkins", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ────────────────────────── Cenário 3 ──────────────────────────

    [Fact]
    public async Task Post_Checkins_Duplicado_MesmoUsuarioMesmaData_DeveRetornar409()
    {
        // Arrange — dois clientes com o mesmo usuário (mesmo JWT)
        var usuarioId = Guid.NewGuid();
        var client = CriarClienteComJwt(usuarioId);
        var data = DateOnly.FromDateTime(DateTime.Today);

        var request = CheckinRequestDeTeste(data);

        // Primeiro check-in — deve criar com sucesso
        var primeiraResposta = await client.PostAsJsonAsync("/checkins", request);
        primeiraResposta.StatusCode.Should().Be(
            HttpStatusCode.Created,
            because: "o primeiro check-in do dia deve ser aceito");

        // Act — segundo check-in no mesmo dia
        var segundaResposta = await client.PostAsJsonAsync("/checkins", request);

        // Assert
        segundaResposta.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ────────────────────── Helpers privados ──────────────────────

    /// <summary>Cria um <see cref="HttpClient"/> com Bearer token JWT para o usuário informado.</summary>
    private HttpClient CriarClienteComJwt(Guid usuarioId)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var jwt = GerarJwtTeste(usuarioId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return client;
    }

    /// <summary>Gera um JWT de teste assinado com <see cref="TestJwtSecret"/>.</summary>
    private static string GerarJwtTeste(Guid usuarioId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Email, "teste@provavida.com"),
            new Claim(ClaimTypes.Name, "Usuário de Teste")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Retorna um <see cref="CheckinRequest"/> válido para a data informada.</summary>
    private static CheckinRequest CheckinRequestDeTeste(DateOnly data) => new(
        Data: data,
        Latitude: -23.5505,
        Longitude: -46.6333,
        IdentificacaoAparelho: "Integration/TestRunner"
    );
}
