using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Implementação de <see cref="ISessionManager"/> que persiste e recupera
/// o estado de sessão do usuário a partir do SQLite local.
/// </summary>
/// <remarks>
/// O estado "logado" é determinado exclusivamente pela existência de um registro de
/// <see cref="Usuario"/> no SQLite — o estado do JWT não é relevante.
/// Conforme <c>Fluxo-Logoff.md</c>, <see cref="ClearSessionAsync"/> exclui os
/// check-ins antes do usuário, garantindo a integridade referencial.
/// </remarks>
public class SessionManager : ISessionManager
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICheckinRepository _checkinRepository;

    /// <summary>
    /// Inicializa o gerenciador de sessão com os repositórios SQLite necessários.
    /// </summary>
    /// <param name="usuarioRepository">Repositório SQLite de usuários.</param>
    /// <param name="checkinRepository">Repositório SQLite de check-ins.</param>
    public SessionManager(
        IUsuarioRepository usuarioRepository,
        ICheckinRepository checkinRepository)
    {
        _usuarioRepository = usuarioRepository;
        _checkinRepository = checkinRepository;
    }

    /// <inheritdoc/>
    public async Task<bool> IsLoggedInAsync()
    {
        var result = await _usuarioRepository.GetAllAsync();
        return result.Success && result.Data?.Any() == true;
    }

    /// <inheritdoc/>
    public async Task<Usuario?> GetCurrentUserAsync()
    {
        var result = await _usuarioRepository.GetAllAsync();
        return result.Success ? result.Data?.FirstOrDefault() : null;
    }

    /// <inheritdoc/>
    public async Task SaveSessionAsync(Usuario usuario, string senhaHash)
    {
        usuario.SenhaHash = senhaHash;
        await _usuarioRepository.UpsertAsync(usuario);
    }

    /// <inheritdoc/>
    public async Task ClearSessionAsync()
    {
        // Conforme Fluxo-Logoff.md: excluir check-ins antes do usuário
        var checkins = await _checkinRepository.GetAllAsync();
        if (checkins.Success && checkins.Data is not null)
        {
            foreach (var checkin in checkins.Data)
                await _checkinRepository.DeleteAsync(checkin.Id);
        }

        var usuarios = await _usuarioRepository.GetAllAsync();
        if (usuarios.Success && usuarios.Data is not null)
        {
            foreach (var usuario in usuarios.Data)
                await _usuarioRepository.DeleteAsync(usuario.Id);
        }
    }
}
