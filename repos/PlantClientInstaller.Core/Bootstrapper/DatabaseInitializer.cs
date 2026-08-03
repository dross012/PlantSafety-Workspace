using System;
using System.Diagnostics;
using System.IO;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Creates and initializes the Plant Client database using deterministic,
    /// enterprise-compliant rules. No drift, no implicit behavior.
    /// </summary>
    public sealed class DatabaseInitializer(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full database initialization pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting database initialization pipeline.");

            ValidatePaths();
            CreateDatabaseIfMissing();
            ApplySchemaScripts();
            ApplySeedScripts();

            _logger.Info("Database initialization pipeline completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. VALIDATION
        // ------------------------------------------------------------

        private void ValidatePaths()
        {
            _logger.Info("Validating database script paths.");

            if (!Directory.Exists(_context.DatabaseScriptPath))
            {
                throw new DirectoryNotFoundException(
                    $"Database script directory not found: {_context.DatabaseScriptPath}");
            }

            var schemaDir = Path.Combine(_context.DatabaseScriptPath, "Schema");
            var seedDir = Path.Combine(_context.DatabaseScriptPath, "Seed");

            if (!Directory.Exists(schemaDir))
                throw new DirectoryNotFoundException($"Schema directory not found: {schemaDir}");

            if (!Directory.Exists(seedDir))
                _logger.Warning($"Seed directory not found: {seedDir} (skipping seed scripts)");
        }

        // ------------------------------------------------------------
        // 2. CREATE DATABASE IF MISSING
        // ------------------------------------------------------------

        private void CreateDatabaseIfMissing()
        {
            _logger.Info("Checking if database exists.");

            var sql = $"IF DB_ID('{_context.DatabaseName}') IS NULL CREATE DATABASE [{_context.DatabaseName}]";

            ExecuteSql(sql, "Database creation");

            _logger.Info($"Database '{_context.DatabaseName}' verified or created.");
        }

        // ------------------------------------------------------------
        // 3. APPLY SCHEMA SCRIPTS
        // ------------------------------------------------------------

        private void ApplySchemaScripts()
        {
            _logger.Info("Applying schema scripts.");

            var schemaDir = Path.Combine(_context.DatabaseScriptPath, "Schema");

            foreach (var file in Directory.GetFiles(schemaDir, "*.sql"))
            {
                _logger.Info($"Applying schema script: {file}");
                ExecuteSqlFile(file);
            }

            _logger.Info("Schema scripts applied successfully.");
        }

        // ------------------------------------------------------------
        // 4. APPLY SEED SCRIPTS
        // ------------------------------------------------------------

        private void ApplySeedScripts()
        {
            var seedDir = Path.Combine(_context.DatabaseScriptPath, "Seed");

            if (!Directory.Exists(seedDir))
            {
                _logger.Info("No seed scripts found. Skipping.");
                return;
            }

            _logger.Info("Applying seed scripts.");

            foreach (var file in Directory.GetFiles(seedDir, "*.sql"))
            {
                _logger.Info($"Applying seed script: {file}");
                ExecuteSqlFile(file);
            }

            _logger.Info("Seed scripts applied successfully.");
        }

        // ------------------------------------------------------------
        // SUPPORTING METHODS
        // ------------------------------------------------------------

        private void ExecuteSql(string sql, string description)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"installer_sql_{Guid.NewGuid()}.sql");
            File.WriteAllText(tempFile, sql);

            ExecuteSqlFile(tempFile);

            File.Delete(tempFile);
        }

        private void ExecuteSqlFile(string filePath)
        {
            var args =
                $"-S localhost\\{_context.SqlInstanceName} " +
                $"-U sa -P {_context.SqlSaPassword} " +
                $"-d master -i \"{filePath}\"";

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
                    $"SQL script execution failed for '{filePath}'. Exit code: {process?.ExitCode}");
            }
        }
    }
}
