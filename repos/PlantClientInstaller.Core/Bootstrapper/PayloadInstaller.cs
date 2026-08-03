using System;
using System.IO;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Installs the client application payload (app, modules, drivers, assets)
    /// in a deterministic, enterprise-compliant pipeline.
    /// </summary>
    public sealed class PayloadInstaller(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full payload installation pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting payload installation pipeline.");

            ValidatePaths();
            InstallClientApp();
            InstallModules();
            InstallDrivers();
            InstallAssets();
            VerifyInstallation();

            _logger.Info("Payload installation pipeline completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. VALIDATION
        // ------------------------------------------------------------

        private void ValidatePaths()
        {
            _logger.Info("Validating payload paths.");

            if (!Directory.Exists(_context.PayloadPath))
            {
                throw new DirectoryNotFoundException(
                    $"Payload directory not found: {_context.PayloadPath}");
            }

            if (!Directory.Exists(_context.ClientAppPayloadPath))
            {
                throw new DirectoryNotFoundException(
                    $"Client app payload directory not found: {_context.ClientAppPayloadPath}");
            }

            if (!Directory.Exists(_context.InstallRoot))
            {
                Directory.CreateDirectory(_context.InstallRoot);
                _logger.Info($"Created install root directory: {_context.InstallRoot}");
            }
        }

        // ------------------------------------------------------------
        // 2. CLIENT APP INSTALLATION
        // ------------------------------------------------------------

        private void InstallClientApp()
        {
            _logger.Info("Installing client application payload.");

            var source = _context.ClientAppPayloadPath;
            var target = Path.Combine(_context.InstallRoot, "ClientApp");

            CopyDirectory(source, target);
            _logger.Info($"Client application installed to: {target}");
        }

        // ------------------------------------------------------------
        // 3. MODULE INSTALLATION
        // ------------------------------------------------------------

        private void InstallModules()
        {
            if (!Directory.Exists(_context.ModulePayloadPath))
            {
                _logger.Info("Module payload directory not found. Skipping module installation.");
                return;
            }

            _logger.Info("Installing module payload.");

            var source = _context.ModulePayloadPath;
            var target = Path.Combine(_context.InstallRoot, "Modules");

            CopyDirectory(source, target);
            _logger.Info($"Modules installed to: {target}");
        }

        // ------------------------------------------------------------
        // 4. DRIVER INSTALLATION
        // ------------------------------------------------------------

        private void InstallDrivers()
        {
            if (!Directory.Exists(_context.DriverPayloadPath))
            {
                _logger.Info("Driver payload directory not found. Skipping driver payload copy.");
                return;
            }

            _logger.Info("Copying driver payload.");

            var source = _context.DriverPayloadPath;
            var target = Path.Combine(_context.InstallRoot, "Drivers");

            CopyDirectory(source, target);
            _logger.Info($"Drivers copied to: {target}");
        }

        // ------------------------------------------------------------
        // 5. ASSET INSTALLATION
        // ------------------------------------------------------------

        private void InstallAssets()
        {
            if (!Directory.Exists(_context.AssetPayloadPath))
            {
                _logger.Info("Asset payload directory not found. Skipping asset installation.");
                return;
            }

            _logger.Info("Installing UI/assets payload.");

            var source = _context.AssetPayloadPath;
            var target = Path.Combine(_context.InstallRoot, "Assets");

            CopyDirectory(source, target);
            _logger.Info($"Assets installed to: {target}");
        }

        // ------------------------------------------------------------
        // 6. VERIFICATION
        // ------------------------------------------------------------

        private void VerifyInstallation()
        {
            _logger.Info("Verifying payload installation.");

            var clientExe = Path.Combine(_context.InstallRoot, "ClientApp", _context.ClientExecutableName);

            if (!File.Exists(clientExe))
            {
                throw new InvalidOperationException(
                    $"Client executable not found after installation: {clientExe}");
            }

            _logger.Info($"Payload verification succeeded. Client executable: {clientExe}");
        }

        // ------------------------------------------------------------
        // SUPPORTING METHODS
        // ------------------------------------------------------------

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            var source = new DirectoryInfo(sourceDir);
            var target = new DirectoryInfo(targetDir);

            if (!target.Exists)
            {
                target.Create();
            }

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
    }
}
