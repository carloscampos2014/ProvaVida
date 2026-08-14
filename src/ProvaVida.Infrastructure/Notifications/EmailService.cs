using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Infrastructure.Notifications;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarAsync(EmailMensagem mensagem, CancellationToken ct = default)
    {
        var host     = _configuration["Email:Host"]     ?? throw new InvalidOperationException("Email:Host não configurado.");
        var port     = int.Parse(_configuration["Email:Port"] ?? "587");
        var usuario  = _configuration["Email:Usuario"]  ?? throw new InvalidOperationException("Email:Usuario não configurado.");
        var senha    = _configuration["Email:Senha"]    ?? throw new InvalidOperationException("Email:Senha não configurado.");
        var nomeReme = _configuration["Email:NomeRemetente"] ?? "ProvaVida";
        var emailRem = _configuration["Email:EmailRemetente"] ?? usuario;

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(nomeReme, emailRem));
        email.To.Add(new MailboxAddress(mensagem.NomePara, mensagem.Para));
        email.Subject = mensagem.Assunto;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = mensagem.CorpoHtml
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(usuario, senha, ct);
        await client.SendAsync(email, ct);
        await client.DisconnectAsync(true, ct);
    }
}
