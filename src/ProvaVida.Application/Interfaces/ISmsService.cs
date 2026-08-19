namespace ProvaVida.Application.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// Envia um SMS para o número informado.
    /// </summary>
    /// <param name="para">Número de telefone no formato E.164 (ex: +5511999999999)</param>
    /// <param name="mensagem">Texto da mensagem (máx 160 chars para SMS simples)</param>
    Task EnviarAsync(string para, string mensagem, CancellationToken ct = default);
}
