namespace ProvaVida.Shared.Common;

/// <summary>
/// Representa o resultado de uma operação sem retorno de dados.
/// </summary>
public class Result
{
    /// <summary>Indica se a operação foi bem-sucedida.</summary>
    public bool Success { get; protected set; }

    /// <summary>Mensagem de erro quando <see cref="Success"/> é false.</summary>
    public string MessageErro { get; protected set; } = string.Empty;

    /// <summary>Cria um resultado de sucesso.</summary>
    public static Result Ok() => new() { Success = true };

    /// <summary>Cria um resultado de falha com a mensagem informada.</summary>
    public static Result Fail(string messageErro) => new() { Success = false, MessageErro = messageErro };
}
