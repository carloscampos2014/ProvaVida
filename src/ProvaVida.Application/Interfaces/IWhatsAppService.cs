namespace ProvaVida.Application.Interfaces;

public interface IWhatsAppService
{
    Task EnviarAsync(string numero, string mensagem, CancellationToken ct = default);
}
