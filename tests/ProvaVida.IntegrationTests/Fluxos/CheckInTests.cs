using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProvaVida.IntegrationTests.Helpers;
using ProvaVida.IntegrationTests.Infrastructure;

namespace ProvaVida.IntegrationTests.Fluxos;

public class CheckInTests : IClassFixture<ProvaVidaWebFactory>, IAsyncLifetime
{
    private readonly ProvaVidaWebFactory _factory;
    private readonly HttpClient _client;
    private readonly DatabaseCleaner _cleaner;

    public CheckInTests(ProvaVidaWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cleaner = new DatabaseCleaner("Host=localhost;Port=5432;Database=provavida_dev;Username=postgres;Password=12345678");
    }

    public async Task InitializeAsync() => await _cleaner.LimparAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CheckIn_Novo_Retorna204ESalvaNoBanco()
    {
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        var idLocal = Guid.NewGuid();
        var res = await _client.PostAsJsonAsync("/checkin", new
        {
            IdLocal = idLocal,
            DataHora = DateTime.UtcNow,
            Latitude = -23.5,
            Longitude = -46.6,
            DeviceId = "device-integracao"
        });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CheckIn_Duplicado_Retorna200Idempotente()
    {
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        var payload = new
        {
            IdLocal = Guid.NewGuid(),
            DataHora = DateTime.UtcNow,
            Latitude = (double?)null,
            Longitude = (double?)null,
            DeviceId = "device"
        };

        await _client.PostAsJsonAsync("/checkin", payload);
        var res2 = await _client.PostAsJsonAsync("/checkin", payload);

        res2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CheckIn_SemLocalizacao_Retorna204()
    {
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        var res = await _client.PostAsJsonAsync("/checkin", new
        {
            IdLocal = Guid.NewGuid(),
            DataHora = DateTime.UtcNow,
            Latitude = (double?)null,
            Longitude = (double?)null,
            DeviceId = "device"
        });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Historico_RetornaCheckInsDoPeriodo()
    {
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        // Faz 2 check-ins
        for (int i = 0; i < 2; i++)
        {
            await _client.PostAsJsonAsync("/checkin", new
            {
                IdLocal = Guid.NewGuid(),
                DataHora = DateTime.UtcNow.AddMinutes(-i),
                Latitude = (double?)null,
                Longitude = (double?)null,
                DeviceId = "device"
            });
        }

        var res = await _client.GetAsync("/checkin/historico");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        var items = JsonDocument.Parse(body).RootElement;
        items.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Heartbeat_Retorna204()
    {
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        var res = await _client.PostAsync("/heartbeat", null);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CheckIn_SemToken_Retorna401()
    {
        var res = await _client.PostAsJsonAsync("/checkin", new
        {
            IdLocal = Guid.NewGuid(),
            DataHora = DateTime.UtcNow,
            DeviceId = "device"
        });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
