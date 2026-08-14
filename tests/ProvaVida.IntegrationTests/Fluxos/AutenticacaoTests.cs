using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProvaVida.IntegrationTests.Helpers;
using ProvaVida.IntegrationTests.Infrastructure;

namespace ProvaVida.IntegrationTests.Fluxos;

public class AutenticacaoTests : IClassFixture<ProvaVidaWebFactory>, IAsyncLifetime
{
    private readonly ProvaVidaWebFactory _factory;
    private readonly HttpClient _client;
    private readonly DatabaseCleaner _cleaner;

    public AutenticacaoTests(ProvaVidaWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cleaner = new DatabaseCleaner("Host=localhost;Port=5432;Database=provavida_dev;Username=postgres;Password=12345678");
    }

    public async Task InitializeAsync() => await _cleaner.LimparAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Cadastro_Login_Logoff_FluxoCompleto()
    {
        // Cadastro
        var cadastro = new
        {
            Nome = "João Integração",
            Email = "joao.integracao@test.com",
            WhatsApp = "11999990001",
            Senha = "Senha@123",
            ContatoEmergenciaNome = "Maria",
            ContatoEmergenciaEmail = "maria@test.com",
            ContatoEmergenciaWhatsApp = "11888880001"
        };

        var resCadastro = await _client.PostAsJsonAsync("/auth/cadastro", cadastro);
        resCadastro.StatusCode.Should().Be(HttpStatusCode.Created);

        // Login
        var resLogin = await _client.PostAsJsonAsync("/auth/login",
            new { Email = "joao.integracao@test.com", Senha = "Senha@123" });
        resLogin.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await resLogin.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody!.Token.Should().NotBeNullOrEmpty();

        // Logoff
        AuthHelper.SetBearerToken(_client, loginBody.Token);
        var resLogoff = await _client.PostAsync("/auth/logoff", null);
        resLogoff.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Login_CredenciaisInvalidas_Retorna401()
    {
        var res = await _client.PostAsJsonAsync("/auth/login",
            new { Email = "nao.existe@test.com", Senha = "senha_errada" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cadastro_EmailDuplicado_Retorna409()
    {
        var payload = new
        {
            Nome = "Duplicado",
            Email = "duplicado@test.com",
            WhatsApp = "11999990002",
            Senha = "Senha@123",
            ContatoEmergenciaNome = "Contato",
            ContatoEmergenciaEmail = "contato@test.com",
            ContatoEmergenciaWhatsApp = "11888880002"
        };

        await _client.PostAsJsonAsync("/auth/cadastro", payload);
        var res2 = await _client.PostAsJsonAsync("/auth/cadastro", payload);

        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AlterarConta_DadosValidos_Retorna204()
    {
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        var res = await _client.PutAsJsonAsync("/conta", new
        {
            Nome = "Nome Atualizado",
            WhatsApp = "11777770001",
            ContatoEmergenciaNome = "Contato Novo",
            ContatoEmergenciaEmail = "novo@test.com",
            ContatoEmergenciaWhatsApp = "11666660001"
        });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ExcluirConta_SenhaCorreta_Retorna204()
    {
        var (token, _) = await AuthHelper.CriarUsuarioELogarAsync(_client);
        AuthHelper.SetBearerToken(_client, token);

        var res = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/conta")
        {
            Content = JsonContent.Create(new { Senha = "Senha@123" })
        });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private record LoginResponse(string Token, DateTime ExpiraEm);
}
