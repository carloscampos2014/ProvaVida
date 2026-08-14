using ProvaVida.Maui.Models;
using SQLite;

namespace ProvaVida.Maui.Storage;

/// <summary>
/// Banco SQLite local — singleton, thread-safe via Async APIs.
/// </summary>
public class LocalDatabase
{
    private SQLiteAsyncConnection? _db;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db is not null) return _db;

        await _lock.WaitAsync();
        try
        {
            if (_db is not null) return _db;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "provavida.db3");
            _db = new SQLiteAsyncConnection(dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await _db.CreateTableAsync<CheckInLocal>();
            await _db.CreateTableAsync<HeartbeatLocal>();

            return _db;
        }
        finally
        {
            _lock.Release();
        }
    }

    // --- CheckIn ---

    public async Task SalvarCheckInAsync(CheckInLocal item)
    {
        var db = await GetDbAsync();
        await db.InsertOrReplaceAsync(item);
    }

    public async Task<List<CheckInLocal>> ObterCheckInsPendentesAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<CheckInLocal>()
            .Where(c => !c.Sincronizado)
            .ToListAsync();
    }

    public async Task MarcarCheckInSincronizadoAsync(string idLocal)
    {
        var db = await GetDbAsync();
        var item = await db.Table<CheckInLocal>().FirstOrDefaultAsync(c => c.IdLocal == idLocal);
        if (item is null) return;
        item.Sincronizado = true;
        await db.UpdateAsync(item);
    }

    public async Task IncrementarTentativaCheckInAsync(string idLocal)
    {
        var db = await GetDbAsync();
        var item = await db.Table<CheckInLocal>().FirstOrDefaultAsync(c => c.IdLocal == idLocal);
        if (item is null) return;
        item.TentativasSincronizacao++;
        await db.UpdateAsync(item);
    }

    public async Task<List<CheckInLocal>> ObterCheckInsDaSemanaAsync(string usuarioId)
    {
        var db = await GetDbAsync();
        var inicio = DateTime.UtcNow.AddDays(-6).Date;
        return await db.Table<CheckInLocal>()
            .Where(c => c.UsuarioId == usuarioId && c.DataHora >= inicio)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();
    }

    public async Task<bool> FezCheckInHojeAsync(string usuarioId)
    {
        var db = await GetDbAsync();
        var hoje = DateTime.UtcNow.Date;
        var amanha = hoje.AddDays(1);
        var count = await db.Table<CheckInLocal>()
            .CountAsync(c => c.UsuarioId == usuarioId
                          && c.DataHora >= hoje
                          && c.DataHora < amanha);
        return count > 0;
    }

    // --- Heartbeat ---

    public async Task SalvarHeartbeatAsync(HeartbeatLocal item)
    {
        var db = await GetDbAsync();
        await db.InsertOrReplaceAsync(item);
    }

    public async Task<List<HeartbeatLocal>> ObterHeartbeatsPendentesAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<HeartbeatLocal>()
            .Where(h => !h.Sincronizado)
            .ToListAsync();
    }

    public async Task MarcarHeartbeatSincronizadoAsync(string idLocal)
    {
        var db = await GetDbAsync();
        var item = await db.Table<HeartbeatLocal>().FirstOrDefaultAsync(h => h.IdLocal == idLocal);
        if (item is null) return;
        item.Sincronizado = true;
        await db.UpdateAsync(item);
    }
}
