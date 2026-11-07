using ProvaVida.Shared.Entities;

namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Gerencia o estado de sessão do usuário no dispositivo.
/// </summary>
/// <remarks>
/// A sessão é determinada exclusivamente pela existência de um registro de <see cref="Usuario"/>
/// no SQLite local — o estado do JWT não é relevante para determinar se o usuário está logado.
/// </remarks>
public interface ISessionManager
{
    /// <summary>
    /// Verifica se existe um usuário logado no dispositivo.
    /// </summary>
    /// <returns>
    /// <c>true</c> se houver um registro de <see cref="Usuario"/> no SQLite; <c>false</c> caso contrário.
    /// </returns>
    Task<bool> IsLoggedInAsync();

    /// <summary>
    /// Retorna os dados do usuário atualmente logado no dispositivo.
    /// </summary>
    /// <returns>
    /// O <see cref="Usuario"/> salvo no SQLite, ou <c>null</c> se não houver sessão ativa.
    /// </returns>
    Task<Usuario?> GetCurrentUserAsync();

    /// <summary>
    /// Persiste os dados do usuário e o hash SHA-256 da senha no SQLite local.
    /// </summary>
    /// <param name="usuario">Dados do usuário retornados pela API após login bem-sucedido.</param>
    /// <param name="senhaHash">Hash SHA-256 da senha, usado para renovação silenciosa de token.</param>
    Task SaveSessionAsync(Usuario usuario, string senhaHash);

    /// <summary>
    /// Encerra a sessão excluindo todos os dados do usuário e de check-ins do SQLite local.
    /// </summary>
    /// <remarks>
    /// Conforme <c>Fluxo-Logoff.md</c>: exclui check-ins antes de excluir o usuário.
    /// Após este método, <see cref="IsLoggedInAsync"/> retornará <c>false</c>.
    /// </remarks>
    Task ClearSessionAsync();
}
