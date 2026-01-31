using System.Text.RegularExpressions;

namespace ProvaVida.Dominio.ObjetosValor;

/// <summary>
/// Value Object que encapsula e valida um endereço de email.
/// Um email é considerado válido se segue o padrão básico RFC 5322.
/// </summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex VerificadorEmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    /// <summary>
    /// O valor do email validado.
    /// </summary>
    public string Valor { get; }

    /// <summary>
    /// Cria uma nova instância de Email com validação.
    /// </summary>
    /// <param name="valor">O endereço de email a ser validado.</param>
    /// <exception cref="ArgumentException">Lançado se o email for nulo, vazio ou inválido.</exception>
    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email não pode ser nulo ou vazio.", nameof(valor));

        var emailLimpo = valor.ToLowerInvariant().Trim();
        
        if (!VerificadorEmailRegex.IsMatch(emailLimpo))
            throw new ArgumentException("Email não é válido.", nameof(valor));

        Valor = emailLimpo;
    }

    /// <summary>
    /// Compara dois Value Objects de Email.
    /// </summary>
    public bool Equals(Email? outro)
    {
        if (outro is null) return false;
        return Valor == outro.Valor;
    }

    /// <summary>
    /// Compara este Value Object com outro objeto.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Email email && Equals(email);
    }

    /// <summary>
    /// Retorna o código hash do Value Object.
    /// </summary>
    public override int GetHashCode()
    {
        return Valor.GetHashCode();
    }

    /// <summary>
    /// Retorna o valor do email como string.
    /// </summary>
    public override string ToString()
    {
        return Valor;
    }

    /// <summary>
    /// Operador de igualdade entre dois emails.
    /// </summary>
    public static bool operator ==(Email? esquerda, Email? direita)
    {
        if (esquerda is null) return direita is null;
        return esquerda.Equals(direita);
    }

    /// <summary>
    /// Operador de desigualdade entre dois emails.
    /// </summary>
    public static bool operator !=(Email? esquerda, Email? direita)
    {
        return !(esquerda == direita);
    }
}
