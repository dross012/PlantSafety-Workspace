using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Orchestrates deterministic uninstallation of the Plant Client system.
    /// Enterprise-compliant, drift-free, and fully logged.
    /// </summary>
    public sealed class UninstallerEngine(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full uninstallation pipeline.
        /// </summary>
        public int Run(bool removeDatabase = false)
        {
            _logger.Info("UninstallerEngine started.");

            try
            {
                StopAndRemoveServices();
                RemovePayload();
                RemoveConfigFiles();
                RemoveCertificates();
                RemoveRegistryKeys();
                RemoveRollbackJournal();
                RemoveLogs();

                if (removeDatabase)
                    DropDatabase();

                _logger.Info("UninstallerEngine completed successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Fatal("UninstallerEngine encountered a fatal error.", ex);
                return -1;
            }
        }

        // ------------------------------------------------------------
        // 1. STOP AND REMOVE SERVICES
        // ------------------------------------------------------------

        private void StopAndRemoveServices()
        {
            _logger.Info("Stopping and removing Windows services.");

            foreach (var svc in _context.Services)
            {
                try
                {
                    var controller = new ServiceController(svc.ServiceName);

                    _logger.Info($"Stopping service '{svc.ServiceName}'...");
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));

                    _logger.Info($"Deleting service '{svc.ServiceName}'...");
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"delete \"{svc.ServiceName}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });

                    process?.WaitForExit();
                }
                catch
                {
                    _logger.Warning($"Service '{svc.ServiceName}' not found or could not be removed.");
                }
            }

            _logger.Info("Service removal completed.");
        }

        // ------------------------------------------------------------
        // 2. REMOVE PAYLOAD
        // ------------------------------------------------------------

        private void RemovePayload()
        {
            _logger.Info("Removing installed payload.");

            if (Directory.Exists(_context.InstallRoot))
            {
                Directory.Delete(_context.InstallRoot, recursive: true);
                _logger.Info($"Removed install root: {_context.InstallRoot}");
            }
            else
            {
                _logger.Warning($"Install root not found: {_context.InstallRoot}");
            }
        }

        // ------------------------------------------------------------
        // 3. REMOVE CONFIG FILES
        // ------------------------------------------------------------

        private void RemoveConfigFiles()
        {
            _logger.Info("Removing configuration files.");

            if (Directory.Exists(_context.OutputConfigPath))
            {
                Directory.Delete(_context.OutputConfigPath, recursive: true);
                _logger.Info($"Removed config directory: {_context.OutputConfigPath}");
            }
            else
            {
                _logger.Warning($"Config directory not found: {_context.OutputConfigPath}");
            }
        }

        // ------------------------------------------------------------
        // 4. REMOVE CERTIFICATES
        // ------------------------------------------------------------

        private void RemoveCertificates()
        {
            _logger.Info("Removing installed certificates.");

            if (!Directory.Exists(_context.CertificatePayloadPath))
            {
                _logger.Warning("Certificate payload directory not found. Skipping certificate removal.");
                return;
            }

            foreach (var file in Directory.GetFiles(_context.CertificatePayloadPath, "*.cer"))
            {
                _logger.Info($"Removing certificate: {file}");

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "certutil.exe",
                    Arguments = $"-delstore Root \"{file}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                process?.WaitForExit();
            }

            _logger.Info("Certificate removal completed.");
        }

        // ------------------------------------------------------------
        // 5. REMOVE REGISTRY KEYS
        // ------------------------------------------------------------

        private void RemoveRegistryKeys()
        {
            _logger.Info("Removing registry keys.");

            var keyPath = @"SOFTWARE\PlantClient";

            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true);
                if (key != null)
                {
                    Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(keyPath);
                    _logger.Info($"Removed registry key: {keyPath}");
                }
                else
                {
                    _logger.Warning($"Registry key not found: {keyPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to remove registry key: {keyPath}. {ex.Message}");
            }
        }

        // ------------------------------------------------------------
        // 6. REMOVE ROLLBACK JOURNAL
        // ------------------------------------------------------------

        private void RemoveRollbackJournal()
        {
            _logger.Info("Removing rollback journal.");

            if (File.Exists(_context.RollbackJournalPath))
            {
                File.Delete(_context.RollbackJournalPath);
                _logger.Info($"Removed rollback journal: {_context.RollbackJournalPath}");
            }
            else
            {
                _logger.Warning($"Rollback journal not found: {_context.RollbackJournalPath}");
            }
        }

        // ------------------------------------------------------------
        // 7. REMOVE LOGS
        // ------------------------------------------------------------

        private void RemoveLogs()
        {
            _logger.Info("Removing installer logs.");

            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "PlantClientInstaller",
                "Logs");

            if (Directory.Exists(logDir))
            {
                Directory.Delete(logDir, recursive: true);
                _logger.Info($"Removed log directory: {logDir}");
            }
            else
            {
                _logger.Warning($"Log directory not found: {logDir}");
            }
        }

        // ------------------------------------------------------------
        // 8. DROP DATABASE (OPTIONAL)
        // ------------------------------------------------------------

        private void DropDatabase()
        {
            _logger.Warning("Dropping database (optional flag enabled).");

            var sql = $"DROP DATABASE IF EXISTS [{_context.DatabaseName}]";

            var tempFile = Path.Combine(Path.GetTempPath(), $"uninstall_sql_{Guid.NewGuid()}.sql");
            File.WriteAllText(tempFile, sql);

            var args =
                $"-S localhost\\{_context.SqlInstanceName} " +
                $"-U sa -P {_context.SqlSaPassword} " +
                $"-d master -i \"{tempFile}\"";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sqlcmd.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();
            File.Delete(tempFile);

            if (process == null || process.ExitCode != 0)
            {
                _logger.Warning($"Database drop failed. Exit code: {process?.ExitCode}");
            }
            else
            {
                _logger.Info($"Database '{_context.DatabaseName}' dropped successfully.");
            }
        }
    }
}
