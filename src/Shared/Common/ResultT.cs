namespace ProvaVida.Shared.Common;

/// <summary>
/// Representa o resultado de uma operação que retorna dados do tipo <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Tipo do dado retornado na operação.</typeparam>
public class Result<T> : Result
{
    /// <summary>Dado retornado quando <see cref="Result.Success"/> é true.</summary>
    public T? Data { get; private set; }

    /// <summary>Cria um resultado de sucesso com o dado informado.</summary>
    public static Result<T> Ok(T data) => new() { Success = true, Data = data };

    /// <summary>Cria um resultado de falha com a mensagem informada.</summary>
    public new static Result<T> Fail(string messageErro) => new() { Success = false, MessageErro = messageErro };
}
