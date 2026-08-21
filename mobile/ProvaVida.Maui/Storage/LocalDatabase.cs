using ProvaVida.Maui.Models;
using SQLite;

namespace ProvaVida.Maui.Storage;

/// <summary>
/// Banco SQLite local — singleton, thread-safe via Async APIs.
/// Migrations versionadas aplicadas automaticamente via LocalDatabaseMigrator.
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

            // Aplica migrations pendentes (idempotente — PRAGMA user_version controla versão)
            // V001 cria as tabelas com schema completo — CreateTableAsync não é necessário
            // e conflita com tabelas já existentes ao tentar re-adicionar a PK.
            var migrator = new LocalDatabaseMigrator(_db);
            await migrator.MigrateAsync();

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
        // Início do dia local de 6 dias atrás, convertido para UTC — evita cortar check-ins noturnos
        var inicioLocal = DateTime.Now.AddDays(-6).Date;
        var inicio = new DateTimeOffset(inicioLocal, TimeZoneInfo.Local.GetUtcOffset(inicioLocal));
        return await db.Table<CheckInLocal>()
            .Where(c => c.UsuarioId == usuarioId && c.DataHora >= inicio)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();
    }

    public async Task<bool> ExisteCheckInAsync(string idLocal)
    {
        var db = await GetDbAsync();
        return await db.Table<CheckInLocal>()
            .CountAsync(c => c.IdLocal == idLocal) > 0;
    }

    public async Task<bool> FezCheckInHojeAsync(string usuarioId)
    {
        var db = await GetDbAsync();
        // Janela do dia atual em horário local, convertida para UTC
        var hojeLocal = DateTime.Now.Date;
        var offset    = TimeZoneInfo.Local.GetUtcOffset(hojeLocal);
        var inicio    = new DateTimeOffset(hojeLocal, offset);
        var fim       = new DateTimeOffset(hojeLocal.AddDays(1), offset);
        var count = await db.Table<CheckInLocal>()
            .CountAsync(c => c.UsuarioId == usuarioId
                          && c.DataHora >= inicio
                          && c.DataHora < fim);
        return count > 0;
    }

    /// <summary>
    /// Remove todos os dados locais do usuário (chamado ao excluir conta — LGPD).
    /// </summary>
    public async Task LimparDadosLocaisAsync()
    {
        var db = await GetDbAsync();
        await db.DeleteAllAsync<CheckInLocal>();
        await db.DeleteAllAsync<HeartbeatLocal>();
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
