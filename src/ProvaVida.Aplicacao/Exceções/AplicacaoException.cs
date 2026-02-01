namespace ProvaVida.Aplicacao.Exceções;

/// <summary>
/// Exceção base para a camada de Aplicação.
/// Todas as exceções de negócio derivam desta.
/// </summary>
public class AplicacaoException : Exception
{
    /// <summary>Cria uma exceção de aplicação com mensagem.</summary>
    public AplicacaoException(string mensagem) : base(mensagem) { }

    /// <summary>Cria uma exceção de aplicação com mensagem e exceção interna.</summary>
    public AplicacaoException(string mensagem, Exception innerException) 
        : base(mensagem, innerException) { }
}

/// <summary>
/// Lançada quando tentamos registrar email já existente.
/// </summary>
public class UsuarioJaExisteException : AplicacaoException
{
    public UsuarioJaExisteException(string mensagem) : base(mensagem) { }
}

/// <summary>
/// Lançada quando usuário não é encontrado no banco.
/// </summary>
public class UsuarioNaoEncontradoException : AplicacaoException
{
    public UsuarioNaoEncontradoException(string mensagem) : base(mensagem) { }
}

/// <summary>
/// Lançada quando senha está incorreta durante login.
/// </summary>
public class SenhaInvalidaException : AplicacaoException
{
    public SenhaInvalidaException(string mensagem) : base(mensagem) { }
}

/// <summary>
/// Lançada quando usuário está inativo e tenta fazer check-in.
/// </summary>
public class UsuarioInativoException : AplicacaoException
{
    public UsuarioInativoException(string mensagem) : base(mensagem) { }
}

/// <summary>
/// Lançada quando contato não é encontrado.
/// </summary>
public class ContatoNaoEncontradoException : AplicacaoException
{
    public ContatoNaoEncontradoException(string mensagem) : base(mensagem) { }
}
