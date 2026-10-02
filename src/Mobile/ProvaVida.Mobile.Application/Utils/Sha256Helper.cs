using System.Security.Cryptography;
using System.Text;

namespace ProvaVida.Mobile.Application.Utils;

/// <summary>
/// Utilitário para geração de hash SHA-256.
/// A senha nunca é persistida ou transmitida em texto puro — apenas o hash gerado aqui.
/// </summary>
public static class Sha256Helper
{
    /// <summary>
    /// Computa o hash SHA-256 da senha informada.
    /// </summary>
    /// <param name="senha">Senha em texto puro. Descartada imediatamente após o cálculo do hash.</param>
    /// <returns>Hash SHA-256 em hexadecimal lowercase (64 caracteres).</returns>
    public static string ComputeHash(string senha)
    {
        var bytes = Encoding.UTF8.GetBytes(senha);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
