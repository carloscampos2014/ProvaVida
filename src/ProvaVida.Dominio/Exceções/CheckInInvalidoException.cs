namespace ProvaVida.Dominio.Exceções;

/// <summary>
/// Exceção lançada quando há tentativa de executar operações inválidas com um check-in.
/// Por exemplo: criação de check-in com datas inválidas.
/// </summary>
public sealed class CheckInInvalidoException : ExcecaoDominio
{
    /// <summary>
    /// Inicializa uma nova instância de CheckInInvalidoException.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro.</param>
    public CheckInInvalidoException(string mensagem) : base(mensagem) { }

    /// <summary>
    /// Inicializa uma nova instância de CheckInInvalidoException com uma exceção interna.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro.</param>
    /// <param name="innerException">A exceção que causou este erro.</param>
    public CheckInInvalidoException(string mensagem, Exception? innerException) 
        : base(mensagem, innerException) { }
}
