using System.Security.Cryptography;
using System.Text;

namespace ProvaVida.Shared.Common;

/// <summary>
/// Helpers de hashing de senha com PBKDF2-SHA256 + salt aleatório.
/// O input esperado é o SHA-256 hex gerado no app mobile;
/// o PBKDF2 é aplicado no servidor antes de armazenar ou comparar.
/// </summary>
public static class PasswordHashHelper
{
    private const int Iterations = 100_000;
    private const int HashSize = 32;
    private const int SaltSize = 16;

    /// <summary>
    /// Deriva um hash seguro a partir do SHA-256 recebido, gerando um salt aleatório.
    /// </summary>
    /// <param name="sha256Input">SHA-256 hex de 64 caracteres enviado pelo mobile.</param>
    /// <returns>Tupla (hashedPassword em Base64, salt em Base64).</returns>
    public static (string hashedPassword, string salt) Hash(string sha256Input)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(sha256Input),
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);
        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    /// <summary>
    /// Verifica o SHA-256 recebido contra o hash armazenado usando comparação em tempo constante.
    /// Suporta backwards-compat: se storedSalt for vazio, compara SHA-256 diretamente.
    /// </summary>
    /// <param name="sha256Input">SHA-256 hex enviado pelo mobile na tentativa de login.</param>
    /// <param name="storedHash">Hash PBKDF2 em Base64 armazenado no banco.</param>
    /// <param name="storedSalt">Salt em Base64 armazenado no banco.</param>
    /// <returns><c>true</c> se as credenciais conferem; <c>false</c> caso contrário.</returns>
    public static bool Verify(string sha256Input, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(storedSalt))
        {
            // Backwards compat: registro antigo com SHA-256 puro (sem PBKDF2)
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(sha256Input),
                Encoding.UTF8.GetBytes(storedHash));
        }

        var salt = Convert.FromBase64String(storedSalt);
        var computed = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(sha256Input),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);
        return CryptographicOperations.FixedTimeEquals(computed, Convert.FromBase64String(storedHash));
    }
}
