namespace ProvaVida.Infraestrutura.Servicos;

/// <summary>
/// Serviço de hash de senha usando bcrypt.
/// Implementa algoritmo seguro com salt aleatório.
/// </summary>
public interface IServicoHashSenha
{
    /// <summary>
    /// Gera um hash seguro da senha.
    /// </summary>
    string Hashar(string senha);

    /// <summary>
    /// Verifica se a senha corresponde ao hash armazenado.
    /// </summary>
    bool Verificar(string senha, string hashArmazenado);
}

/// <summary>
/// Implementação do serviço de hash usando bcrypt.
/// </summary>
public class ServicoHashSenha : IServicoHashSenha
{
    /// <summary>
    /// Número de rounds para bcrypt (custo computacional).
    /// Recomendado 12-14 para produção. Aumentar em 1 duplica o tempo.
    /// </summary>
    private const int NumeroRoundsBcrypt = 12;

    public string Hashar(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha não pode ser vazia.", nameof(senha));

        return BCrypt.Net.BCrypt.HashPassword(senha, NumeroRoundsBcrypt);
    }

    public bool Verificar(string senha, string hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha não pode ser vazia.", nameof(senha));

        if (string.IsNullOrWhiteSpace(hashArmazenado))
            throw new ArgumentException("Hash não pode estar vazio.", nameof(hashArmazenado));

        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hashArmazenado);
        }
        catch (ArgumentException)
        {
            // Hash inválido
            return false;
        }
    }
}
