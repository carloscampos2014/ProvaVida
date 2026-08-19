using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProvaVida.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ProvaVida.Infrastructure.Notifications;

/// <summary>
/// Envia SMS via Twilio.
/// No trial, o destinatário precisa ser um número verificado na conta Twilio.
/// Em produção, funciona para qualquer número com um número Twilio comprado.
/// </summary>
public class TwilioSmsService : ISmsService
{
    private readonly string _fromNumber;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        var accountSid = configuration["Twilio:AccountSid"]
            ?? throw new InvalidOperationException("Twilio:AccountSid não configurado.");
        var authToken = configuration["Twilio:AuthToken"]
            ?? throw new InvalidOperationException("Twilio:AuthToken não configurado.");
        _fromNumber = configuration["Twilio:FromSms"]
            ?? throw new InvalidOperationException("Twilio:FromSms não configurado.");

        TwilioClient.Init(accountSid, authToken);
        _logger = logger;
    }

    public async Task EnviarAsync(string para, string mensagem, CancellationToken ct = default)
    {
        // Garante o formato E.164 (+55...)
        var toNumber = para.StartsWith("+") ? para : $"+{para}";

        _logger.LogInformation("Enviando SMS via Twilio para {Para}", toNumber);

        var message = await MessageResource.CreateAsync(
            to:   new Twilio.Types.PhoneNumber(toNumber),
            from: new Twilio.Types.PhoneNumber(_fromNumber),
            body: mensagem);

        if (message.ErrorCode.HasValue)
        {
            throw new InvalidOperationException(
                $"Twilio SMS erro {message.ErrorCode}: {message.ErrorMessage}");
        }

        _logger.LogInformation("SMS Twilio enviado. SID={Sid} Status={Status}",
            message.Sid, message.Status);
    }
}
