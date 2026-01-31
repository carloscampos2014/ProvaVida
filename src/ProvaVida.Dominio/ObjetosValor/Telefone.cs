using System.Text.RegularExpressions;

namespace ProvaVida.Dominio.ObjetosValor;

/// <summary>
/// Value Object que encapsula e valida um número de telefone no formato brasileiro.
/// Aceita formatos como: 11987654321, (11)98765-4321, 11 98765-4321, +55 11 98765-4321.
/// </summary>
public sealed class Telefone : IEquatable<Telefone>
{
    /// <summary>
    /// Apenas 11 dígitos (dois dígitos de DDD + nove dígitos de número).
    /// </summary>
    private const int TelefoneDigitosEsperados = 11;

    /// <summary>
    /// O valor do telefone normalizado (apenas dígitos).
    /// </summary>
    public string Valor { get; }

    /// <summary>
    /// O telefone no formato original fornecido.
    /// </summary>
    public string Formatado { get; }

    /// <summary>
    /// Cria uma nova instância de Telefone com validação.
    /// </summary>
    /// <param name="valor">O número de telefone a ser validado.</param>
    /// <exception cref="ArgumentException">Lançado se o telefone for nulo, vazio ou inválido.</exception>
    public Telefone(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Telefone não pode ser nulo ou vazio.", nameof(valor));

        Formatado = valor.Trim();
        var apenasDigitos = ExtrairApenasDigitos(Formatado);
        
        // Remove código de país se presente (+55)
        if (apenasDigitos.StartsWith("55") && apenasDigitos.Length == 13)
            apenasDigitos = apenasDigitos.Substring(2);

        if (apenasDigitos.Length != TelefoneDigitosEsperados)
            throw new ArgumentException(
                $"Telefone não é válido. Esperado {TelefoneDigitosEsperados} dígitos, recebido {apenasDigitos.Length}.", 
                nameof(valor));

        if (!apenasDigitos.All(char.IsDigit))
            throw new ArgumentException("Telefone contém caracteres não numéricos.", nameof(valor));

        Valor = apenasDigitos;
    }

    /// <summary>
    /// Remove caracteres especiais do telefone, deixando apenas dígitos.
    /// </summary>
    private static string ExtrairApenasDigitos(string telefone)
    {
        return new string(telefone.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Compara dois Value Objects de Telefone.
    /// </summary>
    public bool Equals(Telefone? outro)
    {
        if (outro is null) return false;
        return Valor == outro.Valor;
    }

    /// <summary>
    /// Compara este Value Object com outro objeto.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Telefone telefone && Equals(telefone);
    }

    /// <summary>
    /// Retorna o código hash do Value Object.
    /// </summary>
    public override int GetHashCode()
    {
        return Valor.GetHashCode();
    }

    /// <summary>
    /// Retorna o valor do telefone no formato original.
    /// </summary>
    public override string ToString()
    {
        return Formatado;
    }

    /// <summary>
    /// Operador de igualdade entre dois telefones.
    /// </summary>
    public static bool operator ==(Telefone? esquerda, Telefone? direita)
    {
        if (esquerda is null) return direita is null;
        return esquerda.Equals(direita);
    }

    /// <summary>
    /// Operador de desigualdade entre dois telefones.
    /// </summary>
    public static bool operator !=(Telefone? esquerda, Telefone? direita)
    {
        return !(esquerda == direita);
    }
}
