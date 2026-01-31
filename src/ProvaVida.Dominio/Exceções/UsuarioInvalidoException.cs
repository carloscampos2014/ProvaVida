namespace ProvaVida.Dominio.Exceções;

/// <summary>
/// Exceção lançada quando uma tentativa de criar um usuário é feita sem os dados obrigatórios.
/// </summary>
public sealed class UsuarioInvalidoException : ExcecaoDominio
{
    /// <summary>
    /// Inicializa uma nova instância de UsuarioInvalidoException.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro.</param>
    public UsuarioInvalidoException(string mensagem) : base(mensagem) { }

    /// <summary>
    /// Inicializa uma nova instância de UsuarioInvalidoException com uma exceção interna.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro.</param>
    /// <param name="innerException">A exceção que causou este erro.</param>
    public UsuarioInvalidoException(string mensagem, Exception? innerException) 
        : base(mensagem, innerException) { }
}
