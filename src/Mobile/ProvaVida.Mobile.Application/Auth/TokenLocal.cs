namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Entidade local que representa os tokens de autenticação persistidos no SQLite do dispositivo.
/// </summary>
/// <remarks>
/// POCO puro mapeado por Dapper. Armazena o access token, o refresh token e o hash SHA-256
/// da senha para possibilitar o login silencioso quando o refresh token expira.
/// </remarks>
public class TokenLocal
{
    /// <summary>Identificador único do registro de token.</summary>
    public Guid Id { get; set; }

    /// <summary>Identificador do usuário dono deste token (FK para <c>usuarios.id</c>).</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>JWT de curta duração utilizado para autenticar requisições à API.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Token de longa duração usado para renovar o access token sem nova senha.</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Data e hora de expiração do access token (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Hash SHA-256 da senha, usado no login silencioso quando o refresh token expira.</summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Data e hora de criação do registro (UTC).</summary>
    public DateTime CriadoEm { get; set; }
}
