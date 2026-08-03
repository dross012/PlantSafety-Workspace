using System;
using System.Diagnostics;
using System.IO;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Handles SQL Server Express detection, installation, and verification
    /// in a deterministic, enterprise-compliant pipeline.
    /// </summary>
    public sealed class SqlExpressInstaller(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full SQL Express installation pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting SQL Express installation pipeline.");

            ValidatePayload();
            DetectExistingInstance();

            if (_context.SqlExpressDetected)
            {
                _logger.Info($"SQL Express instance '{_context.SqlInstanceName}' already installed. Skipping installation.");
                return;
            }

            InstallSqlExpress();
            VerifyInstallation();

            _logger.Info("SQL Express installation pipeline completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. VALIDATION
        // ------------------------------------------------------------

        private void ValidatePayload()
        {
            _logger.Info("Validating SQL Express installer payload.");

            if (!Directory.Exists(_context.SqlExpressPayloadPath))
            {
                throw new DirectoryNotFoundException(
                    $"SQL Express payload directory not found: {_context.SqlExpressPayloadPath}");
            }

            var installerPath = Path.Combine(_context.SqlExpressPayloadPath, "sqlexpress.exe");

            if (!File.Exists(installerPath))
            {
                throw new FileNotFoundException($"SQL Express installer not found: {installerPath}");
            }

            _logger.Info("SQL Express payload validation completed.");
        }

        // ------------------------------------------------------------
        // 2. DETECTION
        // ------------------------------------------------------------

        private void DetectExistingInstance()
        {
            _logger.Info($"Detecting SQL Express instance '{_context.SqlInstanceName}'.");

            var serviceName = $"MSSQL${_context.SqlInstanceName}";
            var (Exists, Status) = ServiceHelper.GetServiceStatus(serviceName);

            if (Exists)
            {
                _logger.Info($"SQL Express instance '{_context.SqlInstanceName}' detected with status: {Status}.");
                _context.SqlExpressDetected = true;
            }
            else
            {
                _logger.Info($"SQL Express instance '{_context.SqlInstanceName}' not found.");
                _context.SqlExpressDetected = false;
            }
        }

        // ------------------------------------------------------------
        // 3. INSTALLATION
        // ------------------------------------------------------------

        private void InstallSqlExpress()
        {
            _logger.Info("Beginning SQL Express installation.");

            var installerPath = Path.Combine(_context.SqlExpressPayloadPath, "sqlexpress.exe");

            var args =
                $"/QS /ACTION=Install /FEATURES=SQLEngine " +
                $"/INSTANCENAME={_context.SqlInstanceName} " +
                "/SECURITYMODE=SQL " +
                $"/SAPWD={_context.SqlSaPassword} " +
                "/TCPENABLED=1 /NPENABLED=1";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();

            if (process == null)
            {
                throw new InvalidOperationException("SQL Express installation process failed to start.");
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"SQL Express installation failed. Exit code: {process.ExitCode}");
            }

            _logger.Info("SQL Express installation completed successfully.");
        }

        // ------------------------------------------------------------
        // 4. VERIFICATION
        // ------------------------------------------------------------

        private void VerifyInstallation()
        {
            _logger.Info("Verifying SQL Express installation.");

            var serviceName = $"MSSQL${_context.SqlInstanceName}";
            var (Exists, Status) = ServiceHelper.GetServiceStatus(serviceName);

            if (!Exists)
            {
                throw new InvalidOperationException(
                    $"SQL Express instance '{_context.SqlInstanceName}' was not detected after installation.");
            }

            _logger.Info($"SQL Express instance '{_context.SqlInstanceName}' verified successfully with status: {Status}.");
        }
    }
}
