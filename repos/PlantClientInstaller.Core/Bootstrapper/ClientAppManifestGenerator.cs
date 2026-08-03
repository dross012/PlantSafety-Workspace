using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Generates a deterministic manifest for the installed client application.
    /// Enterprise-compliant, drift-free, and fully logged.
    /// </summary>
    public sealed class ClientAppManifestGenerator(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the manifest generation pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting ClientApp manifest generation.");

            ValidatePaths();
            var manifest = BuildManifest();
            WriteManifest(manifest);

            _logger.Info("ClientApp manifest generation completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. VALIDATION
        // ------------------------------------------------------------

        private void ValidatePaths()
        {
            _logger.Info("Validating manifest paths.");

            var clientAppPath = Path.Combine(_context.InstallRoot, "ClientApp");

            if (!Directory.Exists(clientAppPath))
                throw new DirectoryNotFoundException($"ClientApp directory not found: {clientAppPath}");

            var exePath = Path.Combine(clientAppPath, _context.ClientExecutableName);

            if (!File.Exists(exePath))
                throw new FileNotFoundException($"Client executable not found: {exePath}");
        }

        // ------------------------------------------------------------
        // 2. BUILD MANIFEST OBJECT
        // ------------------------------------------------------------

        private ClientAppManifest BuildManifest()
        {
            _logger.Info("Building manifest object.");

            var clientAppPath = Path.Combine(_context.InstallRoot, "ClientApp");
            var exePath = Path.Combine(clientAppPath, _context.ClientExecutableName);

            var versionFile = Path.Combine(_context.InstallRoot, "version.txt");
            var version = File.Exists(versionFile)
                ? File.ReadAllText(versionFile).Trim()
                : "Unknown";

            var manifest = new ClientAppManifest
            {
                ApplicationName = "PlantClient",
                Executable = _context.ClientExecutableName,
                InstallRoot = _context.InstallRoot,
                Version = version,
                GeneratedAt = DateTime.UtcNow.ToString("o"),
                Files = BuildFileEntries(clientAppPath)
            };

            return manifest;
        }

        private FileEntry[] BuildFileEntries(string root)
        {
            _logger.Info("Collecting file entries for manifest.");

            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
            var entries = new FileEntry[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                entries[i] = new FileEntry
                {
                    Path = file.Replace(root, "").TrimStart('\\'),
                    SizeBytes = new FileInfo(file).Length,
                    Sha256 = ComputeSha256(file)
                };
            }

            return entries;
        }

        private string ComputeSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        // ------------------------------------------------------------
        // 3. WRITE MANIFEST FILE
        // ------------------------------------------------------------

        private void WriteManifest(ClientAppManifest manifest)
        {
            _logger.Info("Writing manifest file.");

            var manifestPath = Path.Combine(_context.InstallRoot, "clientapp.manifest.json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(manifest, options);
            File.WriteAllText(manifestPath, json);

            _logger.Info($"Manifest written: {manifestPath}");
        }
    }

    // ------------------------------------------------------------
    // MANIFEST MODEL
    // ------------------------------------------------------------

    public sealed class ClientAppManifest
    {
        public string ApplicationName { get; init; }
        public string Executable { get; init; }
        public string InstallRoot { get; init; }
        public string Version { get; init; }
        public string GeneratedAt { get; init; }
        public FileEntry[] Files { get; init; }
    }

    public sealed class FileEntry
    {
        public string Path { get; init; }
        public long SizeBytes { get; init; }
        public string Sha256 { get; init; }
    }
}
