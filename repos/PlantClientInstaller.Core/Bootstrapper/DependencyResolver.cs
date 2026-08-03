using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Resolves all installer dependencies in a deterministic, idempotent,
    /// enterprise-compliant pipeline. No drift, no implicit behavior.
    /// </summary>
    public sealed class DependencyResolver(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full dependency resolution pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting dependency resolution pipeline.");

            ValidatePaths();
            DetectDotNetRuntime();
            DetectSqlExpress();
            InstallSqlExpressIfMissing();
            InstallCertificates();
            InstallDrivers();

            _logger.Info("Dependency resolution pipeline completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. VALIDATION
        // ------------------------------------------------------------

        private void ValidatePaths()
        {
            _logger.Info("Validating dependency payload paths.");

            if (!Directory.Exists(_context.PayloadPath))
            {
                throw new DirectoryNotFoundException(
                    $"Payload directory not found: {_context.PayloadPath}");
            }

            if (!Directory.Exists(_context.SqlExpressPayloadPath))
            {
                throw new DirectoryNotFoundException(
                    $"SQL Express payload directory not found: {_context.SqlExpressPayloadPath}");
            }

            if (!Directory.Exists(_context.CertificatePayloadPath))
            {
                _logger.Info("Certificate payload directory not found. Skipping certificate installation.");
            }

            if (!Directory.Exists(_context.DriverPayloadPath))
            {
                _logger.Info("Driver payload directory not found. Skipping driver installation.");
            }
        }

        // ------------------------------------------------------------
        // 2. DOTNET RUNTIME DETECTION
        // ------------------------------------------------------------

        private void DetectDotNetRuntime()
        {
            _logger.Info("Checking for required .NET runtime.");

            // Because installer is self-contained, runtime is not required.
            // This method is included for future extensibility.
            _logger.Info(".NET runtime check skipped (self-contained installer).");
        }

        // ------------------------------------------------------------
        // 3. SQL EXPRESS DETECTION
        // ------------------------------------------------------------

        private bool _sqlExpressInstalled;

        private void DetectSqlExpress()
        {
            _logger.Info("Detecting SQL Express installation.");

            var instanceName = _context.SqlInstanceName;

            var serviceName = $"MSSQL${instanceName}";
            var (Exists, Status) = ServiceHelper.GetServiceStatus(serviceName);

            if (Exists)
            {
                _logger.Info($"SQL Express instance '{instanceName}' detected.");
                _sqlExpressInstalled = true;
            }
            else
            {
                _logger.Info($"SQL Express instance '{instanceName}' not found.");
                _sqlExpressInstalled = false;
            }
        }

        // ------------------------------------------------------------
        // 4. SQL EXPRESS INSTALLATION
        // ------------------------------------------------------------

        private void InstallSqlExpressIfMissing()
        {
            if (_sqlExpressInstalled)
            {
                _logger.Info("SQL Express already installed. Skipping installation.");
                return;
            }

            _logger.Info("Installing SQL Express.");

            var installerPath = Path.Combine(_context.SqlExpressPayloadPath, "sqlexpress.exe");

            if (!File.Exists(installerPath))
            {
                throw new FileNotFoundException($"SQL Express installer not found: {installerPath}");
            }

            var args =
                $"/QS /ACTION=Install /FEATURES=SQLEngine " +
                $"/INSTANCENAME={_context.SqlInstanceName} " +
                "/SECURITYMODE=SQL " +
                $"/SAPWD={_context.SqlSaPassword} " +
                "/TCPENABLED=1";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();

            if (process == null || process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"SQL Express installation failed. Exit code: {process?.ExitCode}");
            }

            _logger.Info("SQL Express installation completed successfully.");
        }

        // ------------------------------------------------------------
        // 5. CERTIFICATE INSTALLATION
        // ------------------------------------------------------------

        private void InstallCertificates()
        {
            if (!Directory.Exists(_context.CertificatePayloadPath))
            {
                return;
            }

            _logger.Info("Installing certificates.");

            foreach (var file in Directory.GetFiles(_context.CertificatePayloadPath, "*.cer"))
            {
                _logger.Info($"Installing certificate: {file}");

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "certutil.exe",
                    Arguments = $"-addstore Root \"{file}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                process?.WaitForExit();

                if (process == null || process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Certificate installation failed for {file}. Exit code: {process?.ExitCode}");
                }
            }

            _logger.Info("Certificate installation completed.");
        }

        // ------------------------------------------------------------
        // 6. DRIVER INSTALLATION
        // ------------------------------------------------------------

        private void InstallDrivers()
        {
            if (!Directory.Exists(_context.DriverPayloadPath))
            {
                return;
            }

            _logger.Info("Installing drivers.");

            foreach (var file in Directory.GetFiles(_context.DriverPayloadPath, "*.msi"))
            {
                _logger.Info($"Installing driver: {file}");

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{file}\" /quiet /norestart",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                process?.WaitForExit();

                if (process == null || process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Driver installation failed for {file}. Exit code: {process?.ExitCode}");
                }
            }

            _logger.Info("Driver installation completed.");
        }
    }

    // ------------------------------------------------------------
    // SUPPORTING TYPES
    // ------------------------------------------------------------

    public static class ServiceHelper
    {
        public static (bool Exists, string Status) GetServiceStatus(string serviceName)
        {
            try
            {
                var controller = new System.ServiceProcess.ServiceController(serviceName);
                var status = controller.Status.ToString();
                return (true, status);
            }
            catch
            {
                return (false, "NotFound");
            }
        }
    }
}
