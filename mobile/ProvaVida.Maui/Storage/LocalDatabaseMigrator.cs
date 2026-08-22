using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.Logging;

namespace ProvaVida.Maui.Storage;

/// <summary>
/// Aplica migrations SQL versionadas no banco SQLite local.
/// Usa PRAGMA user_version como controle de versão — nativo do SQLite, sem dependência externa.
/// Scripts ficam em Storage/Migrations/ embarcados como EmbeddedResource (V001_, V002_, ...).
/// </summary>
public sealed class LocalDatabaseMigrator
{
    private readonly IDbConnection _db;
    private readonly ILogger<LocalDatabaseMigrator> _logger;

    private static readonly IReadOnlyList<string> Scripts =
    [
        "V001_InitialSchema",
        "V002_DateTimeOffset",
    ];

    public LocalDatabaseMigrator(IDbConnection db, ILogger<LocalDatabaseMigrator> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        var versaoAtual = await ObterVersaoAsync();
        _logger.LogInformation("[Migration] Versao atual do banco: {Versao}", versaoAtual);

        for (var i = versaoAtual; i < Scripts.Count; i++)
        {
            var nomeScript = Scripts[i];
            _logger.LogInformation("[Migration] Aplicando {Script}...", nomeScript);

            try
            {
                var sql = CarregarScript(nomeScript);

                // Dividir por ';' e executar cada statement individualmente
                // Microsoft.Data.Sqlite não suporta múltiplos statements em uma única chamada
                var statements = sql
                    .Split(';')
                    .Select(s =>
                    {
                        // Remove linhas de comentário antes de avaliar se o statement é vazio
                        var linhas = s.Split('\n')
                            .Where(l => !l.Trim().StartsWith("--"))
                            .ToArray();
                        return string.Join('\n', linhas).Trim();
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s));

                foreach (var stmt in statements)
                    await _db.ExecuteAsync(stmt);

                await AtualizarVersaoAsync(i + 1);
                _logger.LogInformation("[Migration] {Script} aplicada com sucesso. Banco na versao {Versao}.", nomeScript, i + 1);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[Migration] FALHA ao aplicar {Script}. Banco pode estar inconsistente.", nomeScript);
                throw; // propaga — app não deve abrir com banco incompleto
            }
        }
    }

    private async Task<int> ObterVersaoAsync()
        => await _db.ExecuteScalarAsync<int>("PRAGMA user_version");

    private async Task AtualizarVersaoAsync(int versao)
        => await _db.ExecuteAsync($"PRAGMA user_version = {versao}");

    private static string CarregarScript(string nomeScript)
    {
        var assembly = Assembly.GetExecutingAssembly();
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
