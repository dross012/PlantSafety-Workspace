using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Orchestrates deterministic upgrades of the Plant Client system.
    /// Enterprise-compliant, drift-free, and fully logged.
    /// </summary>
    public sealed class UpgradeEngine(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full upgrade pipeline.
        /// </summary>
        public int Run()
        {
            _logger.Info("UpgradeEngine started.");

            try
            {
                ValidateUpgradePackage();
                StopServices();
                BackupExistingPayload();
                ApplyUpgradePayload();
                ApplyUpgradeConfig();
                ApplyUpgradeDatabase();
                RestartServices();
                StampVersion();

                _logger.Info("UpgradeEngine completed successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error("UpgradeEngine encountered a fatal error.", ex);
                RollbackUpgrade();
                return -1;
            }
        }

        // ------------------------------------------------------------
        // 1. VALIDATE UPGRADE PACKAGE
        // ------------------------------------------------------------

        private void ValidateUpgradePackage()
        {
            _logger.Info("Validating upgrade package.");

            var upgradePath = Path.Combine(_context.PayloadPath, "Upgrade");

            if (!Directory.Exists(upgradePath))
                throw new DirectoryNotFoundException($"Upgrade package directory not found: {upgradePath}");

            var required = new[]
            {
                "ClientApp",
                "Modules",
                "Config",
                "Database"
            };

            foreach (var folder in required)
            {
                var path = Path.Combine(upgradePath, folder);
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException($"Upgrade component missing: {path}");
            }

            _logger.Info("Upgrade package validated.");
        }

        // ------------------------------------------------------------
        // 2. STOP SERVICES
        // ------------------------------------------------------------

        private void StopServices()
        {
            _logger.Info("Stopping services for upgrade.");

            foreach (var svc in _context.Services)
            {
                try
                {
                    var controller = new ServiceController(svc.ServiceName!);
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));

                    _logger.Info($"Service '{svc.ServiceName}' stopped.");
                }
                catch
                {
                    _logger.Warning($"Service '{svc.ServiceName}' not found or could not be stopped.");
                }
            }
        }

        // ------------------------------------------------------------
        // 3. BACKUP EXISTING PAYLOAD
        // ------------------------------------------------------------

        private void BackupExistingPayload()
        {
            _logger.Info("Backing up existing payload.");

            var backupRoot = Path.Combine(_context.InstallRoot, "Backup");

            if (Directory.Exists(backupRoot))
                Directory.Delete(backupRoot, recursive: true);

            Directory.CreateDirectory(backupRoot);

            CopyDirectory(_context.InstallRoot, backupRoot);

            _logger.Info($"Backup created at: {backupRoot}");
        }

        // ------------------------------------------------------------
        // 4. APPLY UPGRADE PAYLOAD
        // ------------------------------------------------------------

        private void ApplyUpgradePayload()
        {
            _logger.Info("Applying upgrade payload.");

            var upgradePath = Path.Combine(_context.PayloadPath, "Upgrade", "ClientApp");
            var targetPath = Path.Combine(_context.InstallRoot, "ClientApp");

            CopyDirectory(upgradePath, targetPath);

            var moduleUpgradePath = Path.Combine(_context.PayloadPath, "Upgrade", "Modules");
            var moduleTargetPath = Path.Combine(_context.InstallRoot, "Modules");

            CopyDirectory(moduleUpgradePath, moduleTargetPath);

            _logger.Info("Upgrade payload applied.");
        }

        // ------------------------------------------------------------
        // 5. APPLY UPGRADE CONFIG
        // ------------------------------------------------------------

        private void ApplyUpgradeConfig()
        {
            _logger.Info("Applying upgrade configuration.");

            var upgradeConfigPath = Path.Combine(_context.PayloadPath, "Upgrade", "Config");
            var targetConfigPath = _context.OutputConfigPath;

            CopyDirectory(upgradeConfigPath, targetConfigPath);

            _logger.Info("Upgrade configuration applied.");
        }

        // ------------------------------------------------------------
        // 6. APPLY UPGRADE DATABASE
        // ------------------------------------------------------------

        private void ApplyUpgradeDatabase()
        {
            _logger.Info("Applying database upgrade scripts.");

            var dbUpgradePath = Path.Combine(_context.PayloadPath, "Upgrade", "Database");

            foreach (var file in Directory.GetFiles(dbUpgradePath, "*.sql"))
            {
                _logger.Info($"Applying DB upgrade script: {file}");
                ExecuteSqlFile(file);
            }

            _logger.Info("Database upgrade completed.");
        }

        // ------------------------------------------------------------
        // 7. RESTART SERVICES
        // ------------------------------------------------------------

        private void RestartServices()
        {
            _logger.Info("Restarting services after upgrade.");

            foreach (var svc in _context.Services)
            {
                try
                {
                    var controller = new ServiceController(svc.ServiceName!);
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));

                    _logger.Info($"Service '{svc.ServiceName}' restarted.");
                }
                catch
                {
                    _logger.Warning($"Service '{svc.ServiceName}' could not be restarted.");
                }
            }
        }

        // ------------------------------------------------------------
        // 8. VERSION STAMPING
        // ------------------------------------------------------------

        private void StampVersion()
        {
            _logger.Info("Stamping version information.");

            var versionFile = Path.Combine(_context.InstallRoot, "version.txt");
            var version = DateTime.Now.ToString("yyyy.MM.dd.HHmm");

            File.WriteAllText(versionFile, $"PlantClient Version: {version}");

            _logger.Info($"Version stamped: {version}");
        }

        // ------------------------------------------------------------
        // 9. ROLLBACK UPGRADE
        // ------------------------------------------------------------

        private void RollbackUpgrade()
        {
            _logger.Warning("Rolling back upgrade.");

            var backupRoot = Path.Combine(_context.InstallRoot, "Backup");

            if (!Directory.Exists(backupRoot))
            {
                _logger.Warning("Backup directory not found. Cannot rollback payload.");
                return;
            }

            CopyDirectory(backupRoot, _context.InstallRoot);

            _logger.Warning("Upgrade rollback completed.");
        }

        // ------------------------------------------------------------
        // SUPPORTING METHODS
        // ------------------------------------------------------------

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            var source = new DirectoryInfo(sourceDir);
            var target = new DirectoryInfo(targetDir);

            if (!target.Exists)
                target.Create();

            foreach (var file in source.GetFiles())
            {
                var targetFilePath = Path.Combine(target.FullName, file.Name);
                file.CopyTo(targetFilePath, overwrite: true);
            }

            foreach (var dir in source.GetDirectories())
            {
                var targetSubDir = Path.Combine(target.FullName, dir.Name);
                CopyDirectory(dir.FullName, targetSubDir);
            }
        }

        private void ExecuteSqlFile(string filePath)
        {
            var args =
                $"-S localhost\\{_context.SqlInstanceName} " +
                $"-U sa -P {_context.SqlSaPassword} " +
                $"-d {_context.DatabaseName} -i \"{filePath}\"";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sqlcmd.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();

            if (process == null || process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"SQL upgrade script failed: {filePath}. Exit code: {process?.ExitCode}");
            }
        }
    }
}
