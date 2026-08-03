namespace PlantClientInstaller.Core.Logs;

public sealed class LogReader : ILogReader
{
    private readonly LogFileProvider _provider;

    public LogReader(LogFileProvider provider)
    {
        _provider = provider;
    }

    public IReadOnlyList<LogEntry> ReadAll() =>
        Parse(File.ReadAllLines(_provider.GetPath()));

    public IReadOnlyList<LogEntry> ReadByLevel(LogLevel level) =>
        ReadAll().Where(e => e.Level == level).ToList();

    public IReadOnlyList<LogEntry> Search(string contains) =>
        ReadAll().Where(e => e.Message.Contains(contains, StringComparison.OrdinalIgnoreCase)).ToList();

    private static IReadOnlyList<LogEntry> Parse(string[] lines)
    {
        var list = new List<LogEntry>();

        foreach (var line in lines)
        {
            // Format: timestamp | level | message | EX: ... | CTX: ...
            var parts = line.Split('|', StringSplitOptions.TrimEntries);

            if (parts.Length < 3)
                continue;

            var timestamp = DateTime.Parse(parts[0]);
            var level = Enum.Parse<LogLevel>(parts[1]);
            var message = parts[2];

            string? exception = parts.FirstOrDefault(p => p.StartsWith("EX:"));
            string? context = parts.FirstOrDefault(p => p.StartsWith("CTX:"));

            list.Add(new LogEntry(
                timestamp,
                level,
                message,
                exception?.Substring(3),
                context?.Substring(4)
            ));
        }

        return list;
    }
}
