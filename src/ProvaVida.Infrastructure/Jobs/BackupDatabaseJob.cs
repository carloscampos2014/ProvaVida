using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ProvaVida.Infrastructure.Jobs;

/// <summary>
/// Job Hangfire que executa pg_dump diariamente e salva em disco.
/// Retém os últimos 30 arquivos, removendo os mais antigos automaticamente.
/// </summary>
public class BackupDatabaseJob
{
    private const int MaxBackups = 30;
    private const string BackupDir = "/opt/provavida/backups";

    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupDatabaseJob> _logger;

    public BackupDatabaseJob(IConfiguration configuration, ILogger<BackupDatabaseJob> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 1)]
    public async Task ExecutarAsync()
    {
        var cs = _configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        var (host, port, database, username, password) = ParseConnectionString(cs);

        Directory.CreateDirectory(BackupDir);

        var nomeArquivo = $"provavida-{DateTime.UtcNow:yyyyMMdd-HHmmss}.sql";
        var caminhoCompleto = Path.Combine(BackupDir, nomeArquivo);

        _logger.LogInformation("Iniciando backup do banco {Database} para {Arquivo}", database, caminhoCompleto);

        await ExecutarPgDumpAsync(host, port, database, username, password, caminhoCompleto);

        RemoverBackupsAntigos();

        _logger.LogInformation("Backup concluído: {Arquivo}", caminhoCompleto);
    }

    public static async Task ExecutarPgDumpAsync(
        string host, string port, string database,
        string username, string password, string destino)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pg_dump",
            Arguments = $"-h {host} -p {port} -U {username} -d {database} -F p --no-password",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        // PGPASSWORD evita prompt interativo
        psi.Environment["PGPASSWORD"] = password;

        using var processo = System.Diagnostics.Process.Start(psi)
            ?? throw new InvalidOperationException("Falha ao iniciar pg_dump.");

        await using var arquivo = File.Create(destino);
        var copyTask = processo.StandardOutput.BaseStream.CopyToAsync(arquivo);
        var stderr   = await processo.StandardError.ReadToEndAsync();

        await copyTask;
        await processo.WaitForExitAsync();

        if (processo.ExitCode != 0)
        {
            // Remove arquivo parcial antes de lançar exceção
            if (File.Exists(destino)) File.Delete(destino);
            throw new InvalidOperationException($"pg_dump falhou (exit {processo.ExitCode}): {stderr}");
        }
    }

    public static async Task ExecutarPsqlRestoreAsync(
        string host, string port, string database,
        string username, string password, string arquivoSql)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "psql",
            Arguments = $"-h {host} -p {port} -U {username} -d {database} --no-password -f \"{arquivoSql}\"",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        psi.Environment["PGPASSWORD"] = password;

        using var processo = System.Diagnostics.Process.Start(psi)
            ?? throw new InvalidOperationException("Falha ao iniciar psql.");

        var stdout = await processo.StandardOutput.ReadToEndAsync();
        var stderr = await processo.StandardError.ReadToEndAsync();
        await processo.WaitForExitAsync();

        if (processo.ExitCode != 0)
            throw new InvalidOperationException($"psql falhou (exit {processo.ExitCode}): {stderr}");
    }

    private void RemoverBackupsAntigos()
    {
        var arquivos = Directory.GetFiles(BackupDir, "provavida-*.sql")
            .OrderByDescending(f => f)
            .Skip(MaxBackups)
            .ToList();

        foreach (var arquivo in arquivos)
        {
            File.Delete(arquivo);
            _logger.LogInformation("Backup antigo removido: {Arquivo}", arquivo);
        }
    }

    public static (string host, string port, string database, string username, string password)
        ParseConnectionString(string cs)
    {
        var builder = new NpgsqlConnectionStringBuilder(cs);
        return (
            builder.Host   ?? "localhost",
            (builder.Port > 0 ? builder.Port : 5432).ToString(),
            builder.Database ?? "provavida",
            builder.Username ?? "postgres",
            builder.Password ?? string.Empty
        );
    }

    public static IEnumerable<FileInfo> ListarBackups()
    {
        if (!Directory.Exists(BackupDir)) return [];
        return new DirectoryInfo(BackupDir)
            .GetFiles("provavida-*.sql")
            .OrderByDescending(f => f.Name);
    }
}
