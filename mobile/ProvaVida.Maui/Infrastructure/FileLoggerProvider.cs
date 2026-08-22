using Microsoft.Extensions.Logging;

namespace ProvaVida.Maui.Infrastructure;

/// <summary>
/// Provider de log que grava em arquivo no AppDataDirectory.
/// Implementação simples sem reflection — compatível com AOT do Android.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logPath;
    private readonly LogLevel _minimumLevel;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public FileLoggerProvider(string logPath, LogLevel minimumLevel = LogLevel.Warning)
    {
        _logPath = logPath;
        _minimumLevel = minimumLevel;
    }

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(categoryName, _logPath, _minimumLevel, _writeLock);

    public void Dispose() { }
}

internal sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly string _logPath;
    private readonly LogLevel _minimumLevel;
    private readonly SemaphoreSlim _writeLock;

    public FileLogger(string category, string logPath, LogLevel minimumLevel, SemaphoreSlim writeLock)
    {
        _category     = category;
        _logPath      = logPath;
        _minimumLevel = minimumLevel;
        _writeLock    = writeLock;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var mensagem = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {formatter(state, exception)}";
        if (exception != null)
            mensagem += $"\n{exception}";

        // Fire-and-forget — não bloqueia a thread principal
        _ = Task.Run(async () =>
        {
            await _writeLock.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_logPath, mensagem + "\n");
            }
            catch { /* log não pode crashar o app */ }
            finally { _writeLock.Release(); }
        });
    }
}
