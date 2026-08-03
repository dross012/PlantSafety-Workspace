using System;
using System.IO;
using System.Text;
using System.Threading;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Deterministic, thread-safe, enterprise-grade logger used by the installer pipeline.
    /// Writes to both console and file with atomic operations and zero drift.
    /// </summary>
    public sealed class Logger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _sync = new object();

        public Logger(string logDirectory)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
                throw new ArgumentNullException(nameof(logDirectory));

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            _logFilePath = Path.Combine(logDirectory, "installer.log");
        }

        // ------------------------------------------------------------
        // PUBLIC LOGGING API
        // ------------------------------------------------------------

        public void Info(string message)
        {
            Write("INFO", message);
        }

        public void Warning(string message)
        {
            Write("WARN", message);
        }

        public void Error(string message, Exception ex = null)
        {
            var fullMessage = ex == null
                ? message
                : $"{message}{Environment.NewLine}{ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}";

            Write("ERROR", fullMessage);
        }

        public void Fatal(string message, Exception ex = null)
        {
            var fullMessage = ex == null
                ? message
                : $"{message}{Environment.NewLine}{ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}";

            Write("FATAL", fullMessage);
        }

        // ------------------------------------------------------------
        // INTERNAL WRITE LOGIC
        // ------------------------------------------------------------

        private void Write(string level, string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var formatted = $"{timestamp} [{level}] {message}";

            // Console output
            Console.WriteLine(formatted);

            // File output (atomic)
            lock (_sync)
            {
                File.AppendAllText(_logFilePath, formatted + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
