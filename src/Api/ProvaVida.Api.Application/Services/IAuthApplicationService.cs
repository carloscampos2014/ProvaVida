using ProvaVida.Api.Application.Commands;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Api.Application.Services;

/// <summary>
/// Contrato da camada de aplicação para operações de autenticação.
/// </summary>
/// <remarks>
/// Todos os métodos retornam <see cref="Result"/> ou <see cref="Result{T}"/> — nunca lançam exceção
/// para fluxos de negócio esperados. Exceções de infraestrutura são capturadas e convertidas em
/// <c>Result.Fail</c> com mensagem genérica.
/// </remarks>
public interface IAuthApplicationService
{
    /// <summary>
    /// Cadastra um novo usuário no sistema.
    /// </summary>
    /// <param name="command">Dados do novo usuário.</param>
    /// <returns>
    /// <see cref="Result{Usuario}"/> com o usuário criado se bem-sucedido;
    /// falha com mensagem explicativa se e-mail já cadastrado ou dados inválidos.
    /// </returns>
    Task<Result<Usuario>> RegisterAsync(CadastrarUsuarioCommand command);

    /// <summary>
    /// Autentica um usuário e retorna os tokens de acesso.
    /// </summary>
    /// <param name="command">Credenciais do usuário (e-mail + hash SHA-256).</param>
    /// <returns>
    /// <see cref="Result{TokenResponse}"/> com os tokens se credenciais válidas;
    /// falha com mensagem genérica se credenciais inválidas (sem revelar qual campo está errado).
    /// </returns>
    Task<Result<TokenResponse>> LoginAsync(LoginCommand command);

    /// <summary>
    /// Renova os tokens de autenticação usando o refresh token atual (Refresh Token Rotation).
    /// </summary>
    /// <param name="command">Refresh token atual.</param>
    /// <returns>
    /// <see cref="Result{TokenResponse}"/> com os novos tokens se o refresh token for válido e ativo;
    /// falha se o token for inválido, revogado ou expirado.
    /// </returns>
    Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenCommand command);

    /// <summary>
    /// Encerra a sessão do usuário revogando o refresh token informado.
    /// </summary>
    /// <param name="command">Refresh token a ser revogado.</param>
    /// <returns>
    /// <see cref="Result"/> de sucesso se o token foi revogado;
    /// falha se o token não for encontrado.
    /// </returns>
    Task<Result> LogoutAsync(LogoutCommand command);
}
