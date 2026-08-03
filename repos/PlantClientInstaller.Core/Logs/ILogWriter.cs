using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace PlantClientInstaller.Core.Logs;

public interface ILogWriter
{
    void Write(LogLevel level, string message, Exception? exception = null, LogContext? context = null);
    void Info(string message, LogContext? context = null);
    void Warn(string message, LogContext? context = null);
    void Error(string message, Exception exception, LogContext? context = null);
}
