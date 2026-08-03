namespace PlantClientInstaller.Core.Logs;

public sealed class LogWriter : ILogWriter
{
    private readonly LogFileProvider _provider;
    private readonly LogPolicy _policy;
    private readonly object _sync = new();

    public LogWriter(LogFileProvider provider, LogPolicy policy)
    {
        _provider = provider;
        _policy = policy;
    }

    public void Write(LogLevel level, string message, Exception? exception = null, LogContext? context = null)
    {
        if (level == LogLevel.Trace && !_policy.IncludeTrace)
            return;

        var entry = new LogEntry(
            DateTime.UtcNow,
            level,
            message,
            _policy.IncludeExceptionDetails ? exception?.ToString() : null,
            _policy.IncludeContext ? context?.ToString() : null
        );

        lock (_sync)
        {
            _provider.RotateIfNeeded();
            File.AppendAllLines(_provider.GetPath(), new[] { entry.ToString() });
        }
    }

    public void Info(string message, LogContext? context = null) =>
        Write(LogLevel.Info, message, null, context);

    public void Warn(string message, LogContext? context = null) =>
        Write(LogLevel.Warn, message, null, context);

    public void Error(string message, Exception exception, LogContext? context = null) =>
        Write(LogLevel.Error, message, exception, context);
}
