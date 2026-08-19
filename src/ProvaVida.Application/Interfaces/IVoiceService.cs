namespace ProvaVida.Application.Interfaces;

public interface IVoiceService
{
    /// <summary>
    /// Faz uma ligação para o número informado com a mensagem lida por TTS.
    /// </summary>
    /// <param name="para">Número de telefone no formato E.164 (ex: +5511999999999)</param>
    /// <param name="mensagem">Texto a ser lido durante a chamada</param>
    Task LigarAsync(string para, string mensagem, CancellationToken ct = default);
}
