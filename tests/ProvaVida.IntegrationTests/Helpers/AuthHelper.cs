using System.Net.Http.Json;
using System.Text.Json;

namespace ProvaVida.IntegrationTests.Helpers;

public static class AuthHelper
{
    private static int _counter = 0;

    /// <summary>
    /// Cria um usuário via POST /auth/cadastro e retorna o token JWT após login.
    /// </summary>
    public static async Task<(string Token, string Email)> CriarUsuarioELogarAsync(
        HttpClient client,
        string? email = null)
    {
        var idx = Interlocked.Increment(ref _counter);
        email ??= $"teste{idx}@integracao.test";

        var cadastroPayload = new
        {
            Nome                        = $"Usuario Teste {idx}",
            Email                       = email,
            WhatsApp                    = $"1199999{idx:D4}",
            Senha                       = "Senha@123",
            ContatoEmergenciaNome       = "Contato Emergência",
            ContatoEmergenciaEmail      = $"contato{idx}@integracao.test",
            ContatoEmergenciaWhatsApp   = $"1188888{idx:D4}"
        };

        var cadastroResponse = await client.PostAsJsonAsync("/auth/cadastro", cadastroPayload);
        cadastroResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = email,
            Senha = "Senha@123"
        });
        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(loginBody);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        return (token, email);
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
