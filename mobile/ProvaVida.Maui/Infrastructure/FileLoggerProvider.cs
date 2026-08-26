using Microsoft.Extensions.Logging;

namespace ProvaVida.Maui.Infrastructure;

/// <summary>
/// Provider de log que grava em arquivo no AppDataDirectory.
/// Implementação síncrona — garante gravação mesmo em AOT Android.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logPath;
    private readonly LogLevel _minimumLevel;

    public FileLoggerProvider(string logPath, LogLevel minimumLevel = LogLevel.Warning)
    {
        _logPath = logPath;
        _minimumLevel = minimumLevel;
    }

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(categoryName, _logPath, _minimumLevel);

    public void Dispose() { }
}

internal sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly string _logPath;
    private readonly LogLevel _minimumLevel;
    private static readonly object _writeLock = new();

    public FileLogger(string category, string logPath, LogLevel minimumLevel)
    {
        _category     = category;
        _logPath      = logPath;
        _minimumLevel = minimumLevel;
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

        // Gravação síncrona com lock — garante que o log não é perdido em AOT Android
        lock (_writeLock)
        {
            try
            {
                File.AppendAllText(_logPath, mensagem + "\n");
            }
            catch { /* log não pode crashar o app */ }
        }
    }
}
