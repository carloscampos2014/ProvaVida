using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProvaVida.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ProvaVida.Infrastructure.Notifications;

/// <summary>
/// Envia mensagens WhatsApp via Twilio.
/// No sandbox, o destinatário precisa ter enviado "join <palavra>" para +14155238886 antes de receber.
/// Em produção, basta um número Twilio aprovado para WhatsApp Business.
/// </summary>
public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly string _fromNumber;
    private readonly ILogger<TwilioWhatsAppService> _logger;

    public TwilioWhatsAppService(IConfiguration configuration, ILogger<TwilioWhatsAppService> logger)
    {
        var accountSid = configuration["Twilio:AccountSid"]
            ?? throw new InvalidOperationException("Twilio:AccountSid não configurado.");
        var authToken = configuration["Twilio:AuthToken"]
            ?? throw new InvalidOperationException("Twilio:AuthToken não configurado.");
        _fromNumber = configuration["Twilio:FromWhatsApp"]
            ?? throw new InvalidOperationException("Twilio:FromWhatsApp não configurado.");

        TwilioClient.Init(accountSid, authToken);
        _logger = logger;
    }

    public async Task EnviarAsync(string para, string mensagem, CancellationToken ct = default)
    {
        // Garante o prefixo whatsapp:
        var toNumber = para.StartsWith("whatsapp:") ? para : $"whatsapp:+{para.TrimStart('+')}";

        _logger.LogInformation("Enviando WhatsApp via Twilio para {Para}", toNumber);

        var message = await MessageResource.CreateAsync(
            to:   new Twilio.Types.PhoneNumber(toNumber),
            from: new Twilio.Types.PhoneNumber(_fromNumber),
            body: mensagem);

        if (message.ErrorCode.HasValue)
        {
            throw new InvalidOperationException(
                $"Twilio erro {message.ErrorCode}: {message.ErrorMessage}");
        }

        _logger.LogInformation("WhatsApp Twilio enviado. SID={Sid} Status={Status}",
            message.Sid, message.Status);
    }
}
