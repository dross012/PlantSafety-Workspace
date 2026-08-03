using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Performs deterministic environment validation before installer execution.
    /// No drift, no implicit behavior, strictly enterprise-compliant.
    /// </summary>
    public sealed class EnvironmentDetector(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full environment validation pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting environment validation pipeline.");

            ValidateOperatingSystem();
            ValidateArchitecture();
            ValidateAdminRights();
            ValidateDiskSpace();
            ValidateRequiredDirectories();
            ValidateSqlPortAvailability();
            ValidateConflictingProcesses();

            _logger.Info("Environment validation pipeline completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. OS VALIDATION
        // ------------------------------------------------------------

        private void ValidateOperatingSystem()
        {
            _logger.Info("Validating operating system.");

            var os = Environment.OSVersion;

            if (os.Platform != PlatformID.Win32NT)
                throw new InvalidOperationException("Installer requires Windows NT platform.");

            if (os.Version.Major < 10)
                throw new InvalidOperationException("Installer requires Windows 10 or later.");

            _logger.Info($"Operating system validated: {os.VersionString}");
        }

        // ------------------------------------------------------------
        // 2. ARCHITECTURE VALIDATION
        // ------------------------------------------------------------

        private void ValidateArchitecture()
        {
            _logger.Info("Validating system architecture.");

            if (!Environment.Is64BitOperatingSystem)
                throw new InvalidOperationException("Installer requires a 64-bit operating system.");

            _logger.Info("System architecture validated: 64-bit.");
        }

        // ------------------------------------------------------------
        // 3. ADMIN RIGHTS VALIDATION
        // ------------------------------------------------------------

        private void ValidateAdminRights()
        {
            _logger.Info("Validating administrative privileges.");

            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                throw new InvalidOperationException("Installer must be run as Administrator.");

            _logger.Info("Administrative privileges validated.");
        }

        // ------------------------------------------------------------
        // 4. DISK SPACE VALIDATION
        // ------------------------------------------------------------

        private void ValidateDiskSpace()
        {
            _logger.Info("Validating disk space.");

            var drive = new DriveInfo(Path.GetPathRoot(_context.InstallRoot));

            const long requiredBytes = 2L * 1024 * 1024 * 1024; // 2 GB

            if (drive.AvailableFreeSpace < requiredBytes)
            {
                throw new InvalidOperationException(
                    $"Insufficient disk space. Required: 2GB. Available: {drive.AvailableFreeSpace / (1024 * 1024)} MB.");
            }

            _logger.Info($"Disk space validated: {drive.AvailableFreeSpace / (1024 * 1024)} MB available.");
        }

        // ------------------------------------------------------------
        // 5. REQUIRED DIRECTORIES
        // ------------------------------------------------------------

        private void ValidateRequiredDirectories()
        {
            _logger.Info("Validating required directories.");

            if (!Directory.Exists(_context.PayloadPath))
                throw new DirectoryNotFoundException($"Payload directory not found: {_context.PayloadPath}");

            if (!Directory.Exists(_context.ConfigTemplatePath))
                throw new DirectoryNotFoundException($"Config template directory not found: {_context.ConfigTemplatePath}");

            _logger.Info("Required directories validated.");
        }

        // ------------------------------------------------------------
        // 6. SQL PORT AVAILABILITY
        // ------------------------------------------------------------

        private void ValidateSqlPortAvailability()
        {
            _logger.Info("Checking SQL Express port availability.");

            const int sqlPort = 1433;

            var usedPorts = GetUsedTcpPorts();

            if (usedPorts.Contains(sqlPort))
            {
                _logger.Warning($"Port {sqlPort} is already in use. SQL Express may not bind correctly.");
            }
            else
            {
                _logger.Info($"Port {sqlPort} is available.");
            }
        }

        private static int[] GetUsedTcpPorts()
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "netstat.exe",
                Arguments = "-ano",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            var output = process?.StandardOutput.ReadToEnd();
            process?.WaitForExit();

            if (output == null)
                return Array.Empty<int>();

            return output
                .Split('\n')
                .Where(line => line.Contains("TCP"))
                .Select(line =>
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) return -1;

                    var endpoint = parts[1];
                    var portStr = endpoint.Split(':').Last();

                    return int.TryParse(portStr, out var port) ? port : -1;
                })
                .Where(port => port > 0)
                .Distinct()
                .ToArray();
        }

        // ------------------------------------------------------------
        // 7. CONFLICTING PROCESSES
        // ------------------------------------------------------------

        private void ValidateConflictingProcesses()
        {
            _logger.Info("Checking for conflicting processes.");

            var conflicts = new[]
            {
                "sqlservr",
                "PlantClient",
                "PlantClient.WorkerService",
                "PlantClient.SyncService"
            };

            foreach (var name in conflicts)
            {
                var running = Process.GetProcessesByName(name);
                if (running.Length > 0)
                {
                    throw new InvalidOperationException(
                        $"Conflicting process detected: {name}. Please stop it before running the installer.");
                }
            }

            _logger.Info("No conflicting processes detected.");
        }
    }
}
