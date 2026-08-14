namespace ProvaVida.Application.Interfaces;

public record EmailMensagem(
    string Para,
    string NomePara,
    string Assunto,
    string CorpoHtml
);

public interface IEmailService
{
    Task EnviarAsync(EmailMensagem mensagem, CancellationToken ct = default);
}
