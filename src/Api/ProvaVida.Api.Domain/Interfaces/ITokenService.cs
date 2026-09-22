using System.Security.Claims;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Api.Domain.Interfaces;

/// <summary>
/// Contrato para geração e validação de tokens JWT e refresh tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera um Access Token JWT para o usuário informado.
    /// </summary>
    /// <param name="usuario">Usuário autenticado cujas claims serão incluídas no token.</param>
    /// <returns>String JWT assinada com HMAC-SHA256.</returns>
    string GenerateAccessToken(Usuario usuario);

    /// <summary>
    /// Gera um Refresh Token criptograficamente seguro.
    /// </summary>
    /// <returns>String Base64 de 64 bytes randômicos.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Extrai as claims de um Access Token expirado sem validar a expiração.
    /// </summary>
    /// <param name="token">Token JWT possivelmente expirado, mas com assinatura válida.</param>
    /// <returns>
    /// <see cref="ClaimsPrincipal"/> com as claims do token se a assinatura for válida;
    /// <c>null</c> caso contrário.
    /// </returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
