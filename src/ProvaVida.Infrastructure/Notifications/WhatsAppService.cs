using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Infrastructure.Notifications;

/// <summary>
/// Integração com a WhatsApp Business API (Meta).
/// Requer WHATSAPP_TOKEN e WHATSAPP_PHONE_NUMBER_ID nas variáveis de ambiente / appsettings.
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public WhatsAppService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
    }

    public async Task EnviarAsync(string numero, string mensagem, CancellationToken ct = default)
    {
        var token         = _configuration["WhatsApp:Token"]         ?? throw new InvalidOperationException("WhatsApp:Token não configurado.");
        var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"] ?? throw new InvalidOperationException("WhatsApp:PhoneNumberId não configurado.");

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = LimparNumero(numero),
            type = "text",
            text = new { body = mensagem }
        };

        var response = await _http.PostAsJsonAsync(
            $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages",
            payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"WhatsApp API erro {(int)response.StatusCode}: {body}");
        }
    }

    /// <summary>
    /// Remove caracteres não numéricos e garante o código de país.
    /// Ex: "(11) 99999-9999" → "5511999999999"
    /// </summary>
    private static string LimparNumero(string numero)
    {
        var digits = new string(numero.Where(char.IsDigit).ToArray());
        return digits.StartsWith("55") ? digits : "55" + digits;
    }
}
