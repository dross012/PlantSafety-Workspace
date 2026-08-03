using System;
using System.IO;
using PlantClientInstaller.Core;
using PlantClientInstaller.Core.Bootstrapper;

namespace PlantClientInstaller.Bootstrapper
{
    /// <summary>
    /// Bootstrapper entry point for the Plant Client Installer.
    /// Deterministic, enterprise-compliant, and drift-free.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.Title = "Plant Client Installer";

            try
            {
                // ------------------------------------------------------------
                // 1. Resolve base path (installer EXE directory)
                // ------------------------------------------------------------
                var basePath = AppContext.BaseDirectory;

                // ------------------------------------------------------------
                // 2. Initialize logging
                // ------------------------------------------------------------
                var logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "PlantClientInstaller",
                    "Logs");

                var logger = new Logger(logDir);
                logger.Info("Bootstrapper started.");
                logger.Info($"Installer base path: {basePath}");

                // ------------------------------------------------------------
                // 3. Create installer context
                // ------------------------------------------------------------
                var context = InstallerContext.CreateDefault(basePath);
                logger.Info("InstallerContext created.");

                // ------------------------------------------------------------
                // 4. Initialize installer engine
                // ------------------------------------------------------------
                var engine = new InstallerEngine(context, logger);
                logger.Info("InstallerEngine initialized.");

                // ------------------------------------------------------------
                // 5. Execute installer pipeline
                // ------------------------------------------------------------
                var result = engine.Run();

                logger.Info($"InstallerEngine returned exit code: {result}");
                logger.Info("Bootstrapper exiting.");

                return result;
            }
            catch (Exception ex)
            {
                // ------------------------------------------------------------
                // 6. Fatal bootstrapper error
                // ------------------------------------------------------------
                Console.WriteLine("A fatal error occurred during installer startup.");
                Console.WriteLine(ex.Message);

                try
                {
                    var logDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "PlantClientInstaller",
                        "Logs");

                    var logger = new Logger(logDir);
                    logger.Fatal("Bootstrapper encountered a fatal error.", ex);
                }
                catch
                {
                    // If logging fails, we still exit cleanly.
                }

                return -1;
            }
        }
    }
}
