using SQLite;
using System.Reflection;

namespace ProvaVida.Maui.Storage;

/// <summary>
/// Aplica migrations SQL versionadas no banco SQLite local.
/// Usa PRAGMA user_version como controle de versão — nativo do SQLite, sem dependência externa.
/// Scripts ficam em Storage/Migrations/ embarcados como EmbeddedResource (V001_, V002_, ...).
/// Cada migration roda dentro de transação — falha é relançada sem atualizar a versão.
/// </summary>
public sealed class LocalDatabaseMigrator
{
    private readonly SQLiteAsyncConnection _db;

    // Migrations em ordem de aplicação — adicionar novas entradas ao final
    private static readonly IReadOnlyList<string> Scripts =
    [
        "V001_InitialSchema",
        "V002_DateTimeOffset",
    ];

    public LocalDatabaseMigrator(SQLiteAsyncConnection db)
    {
        _db = db;
    }

    /// <summary>
    /// Aplica todas as migrations pendentes.
    /// Idempotente — versões já aplicadas são ignoradas.
    /// </summary>
    public async Task MigrateAsync()
    {
        var versaoAtual = await ObterVersaoAsync();

        for (var i = versaoAtual; i < Scripts.Count; i++)
        {
            var sql = CarregarScript(Scripts[i]);
            await _db.RunInTransactionAsync(conn => conn.Execute(sql));
            await AtualizarVersaoAsync(i + 1);
        }
    }

    private async Task<int> ObterVersaoAsync()
    {
        // PRAGMA user_version retorna 0 para bancos novos ou sem versão definida
        var resultado = await _db.ExecuteScalarAsync<int>("PRAGMA user_version");
        return resultado;
    }

    private async Task AtualizarVersaoAsync(int versao)
    {
        // Pragma não aceita parâmetro bind — interpolação é segura aqui (é um int)
        await _db.ExecuteAsync($"PRAGMA user_version = {versao}");
    }

    private static string CarregarScript(string nomeScript)
    {
        var assembly  = Assembly.GetExecutingAssembly();
        var nomeRecurso = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.Contains(nomeScript, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"Script de migration não encontrado: {nomeScript}. " +
                $"Verifique se o arquivo está marcado como EmbeddedResource no .csproj.");

        using var stream = assembly.GetManifestResourceStream(nomeRecurso)
            ?? throw new InvalidOperationException($"Não foi possível abrir o recurso: {nomeRecurso}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
