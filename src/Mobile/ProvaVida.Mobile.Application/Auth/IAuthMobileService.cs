using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;

namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Contrato para as operações de autenticação do app mobile: cadastro, login e logoff.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><see cref="RegisterAsync"/> valida os dados, gera o hash SHA-256 no ViewModel
///   antes da chamada e envia para <c>POST /auth/register</c>.</item>
///   <item><see cref="LoginAsync"/> gera o hash SHA-256 internamente a partir da senha em texto puro,
///   chama <c>POST /auth/login</c> e persiste usuário, tokens e hash no SQLite local.</item>
///   <item><see cref="LogoutAsync"/> revoga o refresh token na API via <c>POST /auth/logout</c>
///   e limpa a sessão local via <see cref="ISessionManager.ClearSessionAsync"/>.</item>
/// </list>
/// </remarks>
public interface IAuthMobileService
{
    /// <summary>
    /// Cadastra um novo usuário enviando os dados para a API.
    /// </summary>
    /// <param name="request">Dados do novo usuário com o hash SHA-256 já computado.</param>
    /// <returns>
    /// <see cref="Result"/> de sucesso se o cadastro foi aceito pela API;
    /// falha com mensagem explicativa se e-mail já cadastrado, dados inválidos ou erro de rede.
    /// </returns>
    Task<Result> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Autentica o usuário: computa o hash SHA-256 da senha, chama a API e persiste sessão no SQLite.
    /// </summary>
    /// <param name="email">E-mail do usuário.</param>
    /// <param name="senha">Senha em texto puro — o hash é gerado internamente e a senha é descartada.</param>
    /// <returns>
    /// <see cref="Result"/> de sucesso com usuário e tokens salvos no SQLite;
    /// falha com "Login ou senha inválidos." se credenciais incorretas.
    /// </returns>
    Task<Result> LoginAsync(string email, string senha);

    /// <summary>
    /// Encerra a sessão: revoga o refresh token na API e limpa os dados locais do SQLite.
    /// </summary>
    /// <returns>
    /// <see cref="Result"/> de sucesso após limpeza local, mesmo que a chamada à API falhe
    /// (best-effort para garantir limpeza local).
    /// </returns>
    Task<Result> LogoutAsync();
}
