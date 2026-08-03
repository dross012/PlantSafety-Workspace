using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PlantClientInstaller.Core.Logs;

public interface ILogReader
{
    IReadOnlyList<LogEntry> ReadAll();
    IReadOnlyList<LogEntry> ReadByLevel(LogLevel level);
    IReadOnlyList<LogEntry> Search(string contains);
}
