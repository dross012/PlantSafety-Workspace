using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Installs and verifies Windows Services required by the client application
    /// using deterministic, enterprise-compliant rules.
    /// </summary>
    public sealed class ServiceInstaller(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full service installation pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting Windows Service installation pipeline.");

            ValidatePaths();

            foreach (var svc in _context.Services)
            {
                InstallService(svc);
                VerifyService(svc);
            }

            _logger.Info("Windows Service installation pipeline completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. VALIDATION
        // ------------------------------------------------------------

        private void ValidatePaths()
        {
            _logger.Info("Validating service payload paths.");

            if (!Directory.Exists(_context.ServicePayloadPath))
            {
                throw new DirectoryNotFoundException(
                    $"Service payload directory not found: {_context.ServicePayloadPath}");
            }

            if (!File.Exists(_context.ServiceRegistrationScript))
            {
                throw new FileNotFoundException(
                    $"Service registration script not found: {_context.ServiceRegistrationScript}");
            }
        }

        // ------------------------------------------------------------
        // 2. INSTALLATION
        // ------------------------------------------------------------

        private void InstallService(ServiceDefinition svc)
        {
            _logger.Info($"Installing service '{svc.ServiceName}'.");

            if (ServiceExists(svc.ServiceName!))
            {
                _logger.Info($"Service '{svc.ServiceName}' already exists. Skipping installation.");
                return;
            }

            var exePath = Path.Combine(_context.ServicePayloadPath, svc.ExecutableName!);

            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException(
                    $"Service executable not found: {exePath}");
            }

            var args =
                $"-ExecutionPolicy Bypass " +
                $"-File \"{_context.ServiceRegistrationScript}\" " +
                $"-ServiceName \"{svc.ServiceName}\" " +
                $"-DisplayName \"{svc.DisplayName}\" " +
                $"-Description \"{svc.Description}\" " +
                $"-ExePath \"{exePath}\" " +
                $"-Account \"{_context.ServiceAccount}\" " +
                $"-Password \"{_context.ServiceAccountPassword}\"";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();

            if (process == null || process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Service installation failed for '{svc.ServiceName}'. Exit code: {process?.ExitCode}");
            }

            _logger.Info($"Service '{svc.ServiceName}' installed successfully.");
        }

        // ------------------------------------------------------------
        // 3. VERIFICATION
        // ------------------------------------------------------------

        private void VerifyService(ServiceDefinition svc)
        {
            _logger.Info($"Verifying service '{svc.ServiceName}'.");

            if (!ServiceExists(svc.ServiceName!))
            {
                throw new InvalidOperationException(
                    $"Service '{svc.ServiceName}' was not detected after installation.");
            }

            var controller = new ServiceController(svc.ServiceName!);

            try
            {
                controller.Refresh();
                _logger.Info($"Service '{svc.ServiceName}' verified with status: {controller.Status}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to verify service '{svc.ServiceName}'.", ex);
            }
        }

        // ------------------------------------------------------------
        // SUPPORTING METHODS
        // ------------------------------------------------------------

        private static bool ServiceExists(string serviceName)
        {
            try
            {
                var controller = new ServiceController(serviceName);
                var _ = controller.Status; // triggers exception if not found
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    // ------------------------------------------------------------
    // SUPPORTING TYPES
    // ------------------------------------------------------------

    public sealed class ServiceDefinition
    {
        public string? ServiceName { get; init; }
        public string? DisplayName { get; init; }
        public string? Description { get; init; }
        public string? ExecutableName { get; init; }
    }
}
