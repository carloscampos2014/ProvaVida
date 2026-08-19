using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProvaVida.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ProvaVida.Infrastructure.Notifications;

/// <summary>
/// Faz ligações automáticas via Twilio com mensagem TTS em pt-BR.
/// No trial, só pode ligar para números verificados na conta Twilio.
/// </summary>
public class TwilioVoiceService : IVoiceService
{
    private readonly string _fromNumber;
    private readonly ILogger<TwilioVoiceService> _logger;

    public TwilioVoiceService(IConfiguration configuration, ILogger<TwilioVoiceService> logger)
    {
        var accountSid = configuration["Twilio:AccountSid"]
            ?? throw new InvalidOperationException("Twilio:AccountSid não configurado.");
        var authToken = configuration["Twilio:AuthToken"]
            ?? throw new InvalidOperationException("Twilio:AuthToken não configurado.");
        _fromNumber = configuration["Twilio:FromVoice"]
            ?? throw new InvalidOperationException("Twilio:FromVoice não configurado.");

        TwilioClient.Init(accountSid, authToken);
        _logger = logger;
    }

    public async Task LigarAsync(string para, string mensagem, CancellationToken ct = default)
    {
        var toNumber = para.StartsWith("+") ? para : $"+{para}";

        _logger.LogInformation("Iniciando ligação via Twilio para {Para}", toNumber);

        // TwiML inline — lê a mensagem em pt-BR e repete uma vez
        var twiml = $"<Response><Say language=\"pt-BR\" voice=\"Polly.Vitoria\">{System.Net.WebUtility.HtmlEncode(mensagem)}</Say><Pause length=\"1\"/><Say language=\"pt-BR\" voice=\"Polly.Vitoria\">{System.Net.WebUtility.HtmlEncode(mensagem)}</Say></Response>";

        var call = await CallResource.CreateAsync(
            to:   new PhoneNumber(toNumber),
            from: new PhoneNumber(_fromNumber),
            twiml: new Twilio.Types.Twiml(twiml));

        if (call.Status == CallResource.StatusEnum.Failed)
        {
            throw new InvalidOperationException(
                $"Twilio Voice falhou. SID={call.Sid}");
        }

        _logger.LogInformation("Ligação Twilio iniciada. SID={Sid} Status={Status}",
            call.Sid, call.Status);
    }
}
