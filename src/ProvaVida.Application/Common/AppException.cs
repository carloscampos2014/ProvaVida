namespace ProvaVida.Application.Common;

/// <summary>
/// Exceção de negócio mapeada para respostas HTTP adequadas na API.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public static AppException NaoEncontrado(string mensagem) => new(mensagem, 404);
    public static AppException NaoAutorizado(string mensagem) => new(mensagem, 401);
    public static AppException Conflito(string mensagem) => new(mensagem, 409);
}
