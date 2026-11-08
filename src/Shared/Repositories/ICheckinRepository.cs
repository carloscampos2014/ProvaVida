using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Shared.Repositories;

/// <summary>
/// Repositório de check-ins — contrato especializado.
/// </summary>
public interface ICheckinRepository : IRepository<Checkin>
{
    /// <summary>Busca check-ins de um usuário, ordenados por data desc.</summary>
    Task<Result<IEnumerable<Checkin>>> GetByUsuarioIdAsync(Guid usuarioId);

    /// <summary>Busca check-in de um usuário para uma data específica. Fail se não encontrado.</summary>
    Task<Result<Checkin>> GetByUsuarioIdAndDataAsync(Guid usuarioId, DateOnly data);

    /// <summary>Busca check-ins não sincronizados (Sincronizado=false). Usado pelo SyncWorker.</summary>
    Task<Result<IEnumerable<Checkin>>> GetPendingSyncAsync();
}
