namespace ProvaVida.Dominio.Exceções;

/// <summary>
/// Classe base para todas as exceções de negócio do domínio.
/// Deve ser utilizada para diferenciar erros de domínio de outros tipos de erro.
/// </summary>
public abstract class ExcecaoDominio : Exception
{
    /// <summary>
    /// Inicializa uma nova instância de ExcecaoDominio.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro de domínio.</param>
    protected ExcecaoDominio(string mensagem) : base(mensagem) { }

    /// <summary>
    /// Inicializa uma nova instância de ExcecaoDominio com uma exceção interna.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro de domínio.</param>
    /// <param name="innerException">A exceção que causou este erro.</param>
    protected ExcecaoDominio(string mensagem, Exception? innerException) 
        : base(mensagem, innerException) { }
}
