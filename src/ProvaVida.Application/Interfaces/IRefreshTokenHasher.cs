namespace ProvaVida.Application.Interfaces;

/// <summary>
/// Gera hashes de refresh tokens para armazenamento seguro.
/// </summary>
public interface IRefreshTokenHasher
{
    string Hash(string token);
}
