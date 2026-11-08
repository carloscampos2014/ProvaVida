using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
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
        Environment.SetEnvironmentVariable("JWT_SECRET", "checkin-integration-test-secret-min32c");

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
        // Arrange — registrar usuário real para satisfazer FK checkins.usuario_id → usuarios.id
        var (client, usuarioId) = await CriarClienteComUsuarioRealAsync();
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
        // Arrange — registrar usuário real para satisfazer FK
        var (client, _) = await CriarClienteComUsuarioRealAsync();
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

    /// <summary>
    /// Registra um novo usuário via /auth/register, faz login via /auth/login e retorna
    /// um cliente HTTP autenticado com o JWT real emitido pela API.
    /// Necessário porque checkins.usuario_id tem FK para usuarios.id.
    /// </summary>
    private async Task<(HttpClient client, Guid usuarioId)> CriarClienteComUsuarioRealAsync()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // SHA-256 simulado válido (64 hex chars)
        var sha256Hash = new string('a', 64);
        var email = $"checkin_{Guid.NewGuid():N}@test.com";

        // Registrar usuário real no banco
        var registerRequest = new RegisterRequest(
            Nome: "Usuário Teste Checkin",
            Email: email,
            Whatsapp: "11999998888",
            SenhaHash: sha256Hash,
            ContatoEmergenciaNome: "Contato Emergência",
            ContatoEmergenciaEmail: "contato@test.com",
            ContatoEmergenciaWhatsapp: "11888887777"
        );
        var registerResponse = await client.PostAsJsonAsync("/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        // Login para obter JWT real com o usuarioId persistido no banco
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest(email, sha256Hash));
        loginResponse.EnsureSuccessStatusCode();
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        // Ler o usuarioId direto do response do register (evita parsing de claims JWT)
        using var registerDoc = await JsonDocument.ParseAsync(
            await registerResponse.Content.ReadAsStreamAsync());
        var usuarioId = Guid.Parse(registerDoc.RootElement.GetProperty("id").GetString()!);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        return (client, usuarioId);
    }

    /// <summary>Retorna um <see cref="CheckinRequest"/> válido para a data informada.</summary>
    private static CheckinRequest CheckinRequestDeTeste(DateOnly data) => new(
        Data: data,
        Latitude: -23.5505,
        Longitude: -46.6333,
        IdentificacaoAparelho: "Integration/TestRunner"
    );
}
