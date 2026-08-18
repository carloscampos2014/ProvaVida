using System.Diagnostics;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Application.UseCases.TestarNotificacao;

public class TestarNotificacaoUseCase
{
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;

    public TestarNotificacaoUseCase(IEmailService emailService, IWhatsAppService whatsAppService)
    {
        _emailService    = emailService;
        _whatsAppService = whatsAppService;
    }

    public async Task<TesteNotificacaoOutput> TestarEmailAsync(string destinatario, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _emailService.EnviarAsync(new EmailMensagem(
                Para: destinatario,
                NomePara: destinatario,
                Assunto: "[ProvaVida Admin] Teste de e-mail",
                CorpoHtml: """
                    <h2>Teste de envio — ProvaVida</h2>
                    <p>Este é um e-mail de teste disparado pelo painel admin do ProvaVida.</p>
                    <p>Se você recebeu esta mensagem, o envio de e-mails está funcionando corretamente.</p>
                    """
            ), ct);
            sw.Stop();
            return new TesteNotificacaoOutput(true, $"E-mail enviado com sucesso.", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new TesteNotificacaoOutput(false, $"Falha: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }

    public async Task<TesteNotificacaoOutput> TestarWhatsAppAsync(string telefone, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _whatsAppService.EnviarAsync(
                telefone,
                "✅ [ProvaVida Admin] Teste de WhatsApp — se você recebeu esta mensagem, o envio está funcionando corretamente.",
                ct);
            sw.Stop();
            return new TesteNotificacaoOutput(true, "Mensagem WhatsApp enviada com sucesso.", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new TesteNotificacaoOutput(false, $"Falha: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }
}

public record TesteNotificacaoOutput(bool Sucesso, string Mensagem, long DuracaoMs);
