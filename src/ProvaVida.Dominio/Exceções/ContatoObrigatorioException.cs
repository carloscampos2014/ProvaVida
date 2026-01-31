namespace ProvaVida.Dominio.Exceções;

/// <summary>
/// Exceção lançada quando uma tentativa de criar um usuário é feita sem nenhum contato de emergência.
/// Um usuário DEVE ter pelo menos um contato para que o monitoramento seja ativado.
/// </summary>
public sealed class ContatoObrigatorioException : ExcecaoDominio
{
    /// <summary>
    /// Inicializa uma nova instância de ContatoObrigatorioException.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro.</param>
    public ContatoObrigatorioException(string mensagem) : base(mensagem) { }

    /// <summary>
    /// Inicializa uma nova instância de ContatoObrigatorioException com uma exceção interna.
    /// </summary>
    /// <param name="mensagem">A mensagem descritiva do erro.</param>
    /// <param name="innerException">A exceção que causou este erro.</param>
    public ContatoObrigatorioException(string mensagem, Exception? innerException) 
        : base(mensagem, innerException) { }
}
