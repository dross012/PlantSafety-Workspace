namespace PlantClientInstaller.Core.Logs;

public sealed class LogEntry
{
    public DateTime Timestamp { get; }
    public LogLevel Level { get; }
    public string Message { get; }
    public string? Exception { get; }
    public string? Context { get; }

    public LogEntry(DateTime timestamp, LogLevel level, string message, string? exception, string? context)
    {
        Timestamp = timestamp;
        Level = level;
        Message = message;
        Exception = exception;
        Context = context;
    }

    public override string ToString() =>
        $"{Timestamp:O} | {Level} | {Message}" +
        (Exception is not null ? $" | EX: {Exception}" : "") +
        (Context is not null ? $" | CTX: {Context}" : "");
}
