using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Handles deterministic rollback of installer actions using a strict,
    /// journal-driven, enterprise-compliant pipeline. No drift, no implicit behavior.
    /// </summary>
    public sealed class RollbackManager(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes rollback using the journal entries in reverse order.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting rollback pipeline.");

            if (!File.Exists(_context.RollbackJournalPath))
            {
                _logger.Info("No rollback journal found. Nothing to rollback.");
                return;
            }

            var entries = LoadJournalEntries();

            foreach (var entry in entries)
            {
                try
                {
                    ExecuteRollbackEntry(entry);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Rollback step failed for: {entry.Action}", ex);
                }
            }

            _logger.Info("Rollback pipeline completed.");
        }

        // ------------------------------------------------------------
        // 1. LOAD JOURNAL
        // ------------------------------------------------------------

        private List<RollbackEntry> LoadJournalEntries()
        {
            _logger.Info("Loading rollback journal.");

            var lines = File.ReadAllLines(_context.RollbackJournalPath);
            var entries = new List<RollbackEntry>();

            foreach (var line in lines)
            {
                var parts = line.Split('|');

                if (parts.Length != 2)
                    continue;

                entries.Add(new RollbackEntry
                {
                    Action = parts[0],
                    Target = parts[1]
                });
            }

            // Reverse order for deterministic rollback
            entries.Reverse();

            _logger.Info($"Loaded {entries.Count} rollback entries.");
            return entries;
        }

        // ------------------------------------------------------------
        // 2. EXECUTE ROLLBACK ENTRY
        // ------------------------------------------------------------

        private void ExecuteRollbackEntry(RollbackEntry entry)
        {
            _logger.Info($"Executing rollback action: {entry.Action} → {entry.Target}");

            switch (entry.Action)
            {
                case "DeleteFile":
                    DeleteFile(entry.Target!);
                    break;

                case "DeleteDirectory":
                    DeleteDirectory(entry.Target!);
                    break;

                case "RemoveService":
                    RemoveService(entry.Target!);
                    break;

                case "RemoveRegistryKey":
                    RemoveRegistryKey(entry.Target!);
                    break;

                case "RestoreFile":
                    RestoreFile(entry.Target!);
                    break;

                default:
                    _logger.Info($"Unknown rollback action '{entry.Action}'. Skipping.");
                    break;
            }
        }

        // ------------------------------------------------------------
        // 3. ROLLBACK OPERATIONS
        // ------------------------------------------------------------

        private void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                _logger.Info($"Deleted file: {path}");
            }
        }

        private void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                _logger.Info($"Deleted directory: {path}");
            }
        }

        private void RemoveService(string serviceName)
        {
            try
            {
                var controller = new ServiceController(serviceName);

                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));

                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"delete \"{serviceName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                process?.WaitForExit();

                _logger.Info($"Removed service: {serviceName}");
            }
            catch
            {
                _logger.Info($"Service '{serviceName}' not found or could not be removed.");
            }
        }

        private void RemoveRegistryKey(string keyPath)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true);
                if (key != null)
                {
                    Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(keyPath);
                    _logger.Info($"Removed registry key: {keyPath}");
                }
            }
            catch
            {
                _logger.Info($"Registry key '{keyPath}' not found or could not be removed.");
            }
        }

        private void RestoreFile(string backupPath)
        {
            var originalPath = backupPath.Replace(".backup", "");

            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, originalPath, overwrite: true);
                _logger.Info($"Restored file: {originalPath}");
            }
        }
    }

    // ------------------------------------------------------------
    // SUPPORTING TYPES
    // ------------------------------------------------------------

    public sealed class RollbackEntry
    {
        public string? Action { get; init; }
        public string? Target { get; init; }
    }
}
