using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ProvaVida.Maui.Models;

namespace ProvaVida.Maui.Storage;

/// <summary>
/// Banco SQLite local — singleton, thread-safe via SemaphoreSlim.
/// Usa Microsoft.Data.Sqlite + Dapper para acesso confiável ao banco.
/// Migrations versionadas aplicadas automaticamente via LocalDatabaseMigrator.
/// </summary>
public class LocalDatabase
{
    private IDbConnection? _db;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<LocalDatabase> _logger;
    private readonly ILogger<LocalDatabaseMigrator> _migratorLogger;

    public LocalDatabase(ILogger<LocalDatabase> logger, ILogger<LocalDatabaseMigrator> migratorLogger)
    {
        _logger = logger;
        _migratorLogger = migratorLogger;
    }

    private async Task<IDbConnection> GetDbAsync()
    {
        if (_db is not null) return _db;

        await _lock.WaitAsync();
        try
        {
            if (_db is not null) return _db;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "provavida.db3");
            _logger.LogInformation("[DB] Abrindo banco em {Path}", dbPath);

            var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();
            _db = conn;

            var migrator = new LocalDatabaseMigrator(_db, _migratorLogger);
            await migrator.MigrateAsync();

            return _db;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[DB] Falha ao inicializar banco SQLite");
            throw;
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
        await db.ExecuteAsync("""
            INSERT OR REPLACE INTO checkins_local
                (id_local, usuario_id, data_hora, latitude, longitude, device_id, sincronizado, tentativas_sincronizacao)
            VALUES
                (@IdLocal, @UsuarioId, @DataHora, @Latitude, @Longitude, @DeviceId, @Sincronizado, @TentativasSincronizacao)
            """, item);
    }

    public async Task<List<CheckInLocal>> ObterCheckInsPendentesAsync()
    {
        var db = await GetDbAsync();
        return (await db.QueryAsync<CheckInLocal>(
            "SELECT * FROM checkins_local WHERE sincronizado = 0")).ToList();
    }

    public async Task MarcarCheckInSincronizadoAsync(string idLocal)
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync(
            "UPDATE checkins_local SET sincronizado = 1 WHERE id_local = @IdLocal",
            new { IdLocal = idLocal });
    }

    public async Task IncrementarTentativaCheckInAsync(string idLocal)
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync(
            "UPDATE checkins_local SET tentativas_sincronizacao = tentativas_sincronizacao + 1 WHERE id_local = @IdLocal",
            new { IdLocal = idLocal });
    }

    public async Task<List<CheckInLocal>> ObterCheckInsDaSemanaAsync(string usuarioId)
    {
        var db = await GetDbAsync();
        // Usa horário local do dispositivo para calcular o início da semana — evita que check-ins
        // feitos após 21h BRT (= 00h UTC do dia seguinte) apareçam no dia errado
        var inicioLocal = DateTime.Now.AddDays(-6).Date;
        var inicioUtc   = inicioLocal.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        return (await db.QueryAsync<CheckInLocal>(
            "SELECT * FROM checkins_local WHERE usuario_id = @UsuarioId AND data_hora >= @Inicio ORDER BY data_hora DESC",
            new { UsuarioId = usuarioId, Inicio = inicioUtc })).ToList();
    }

    public async Task<bool> ExisteCheckInAsync(string idLocal)
    {
        var db = await GetDbAsync();
        return await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM checkins_local WHERE id_local = @IdLocal",
            new { IdLocal = idLocal }) > 0;
    }

    public async Task<bool> FezCheckInHojeAsync(string usuarioId)
    {
        var db = await GetDbAsync();
        // Usa horário local do dispositivo para definir a janela do "hoje" — evita que check-ins
        // feitos após 21h BRT (= 00h UTC do dia seguinte) apareçam como "ontem"
        var hojeLocal = DateTime.Now.Date;
        var inicioUtc = hojeLocal.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        var fimUtc    = hojeLocal.AddDays(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM checkins_local WHERE usuario_id = @UsuarioId AND data_hora >= @Inicio AND data_hora < @Fim",
            new { UsuarioId = usuarioId, Inicio = inicioUtc, Fim = fimUtc });
        return count > 0;
    }

    public async Task LimparDadosLocaisAsync()
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync("DELETE FROM checkins_local");
        await db.ExecuteAsync("DELETE FROM heartbeats_local");
    }

    // --- Heartbeat ---

    public async Task SalvarHeartbeatAsync(HeartbeatLocal item)
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync("""
            INSERT OR REPLACE INTO heartbeats_local
                (id_local, usuario_id, data_hora, sincronizado)
            VALUES
                (@IdLocal, @UsuarioId, @DataHora, @Sincronizado)
            """, item);
    }

    public async Task<List<HeartbeatLocal>> ObterHeartbeatsPendentesAsync()
    {
        var db = await GetDbAsync();
        return (await db.QueryAsync<HeartbeatLocal>(
            "SELECT * FROM heartbeats_local WHERE sincronizado = 0")).ToList();
    }

    public async Task MarcarHeartbeatSincronizadoAsync(string idLocal)
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync(
            "UPDATE heartbeats_local SET sincronizado = 1 WHERE id_local = @IdLocal",
            new { IdLocal = idLocal });
    }
}
