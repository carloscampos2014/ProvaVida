using ProvaVida.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ProvaVida.Infrastructure.Security;

/// <summary>
/// Gera hashes SHA-256 de refresh tokens para armazenamento seguro.
/// O token original nunca é armazenado — apenas o hash.
/// </summary>
public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
