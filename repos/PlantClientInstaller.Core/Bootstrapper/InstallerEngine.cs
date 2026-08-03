using System;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Orchestrates the entire installer pipeline using deterministic,
    /// enterprise-compliant rules. No drift, no implicit behavior.
    /// </summary>
    public sealed class InstallerEngine(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private readonly DependencyResolver _dependencyResolver = new DependencyResolver(context, logger);
        private readonly SqlExpressInstaller _sqlInstaller = new SqlExpressInstaller(context, logger);
        private readonly PayloadInstaller _payloadInstaller = new PayloadInstaller(context, logger);
        private readonly ServiceInstaller _serviceInstaller = new ServiceInstaller(context, logger);
        private readonly ConfigGenerator _configGenerator = new ConfigGenerator(context, logger);
        private readonly RollbackManager _rollbackManager = new RollbackManager(context, logger);

        /// <summary>
        /// Executes the full installer pipeline.
        /// </summary>
        public int Run()
        {
            _logger.Info("InstallerEngine started.");

            try
            {
                _context.Validate();
                _logger.Info("InstallerContext validated.");

                ExecutePhase("Dependency Resolution", _dependencyResolver.Run);
                ExecutePhase("SQL Express Installation", _sqlInstaller.Run);
                ExecutePhase("Payload Installation", _payloadInstaller.Run);
                ExecutePhase("Service Installation", _serviceInstaller.Run);
                ExecutePhase("Configuration Generation", _configGenerator.Run);

                _logger.Info("InstallerEngine completed successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error("InstallerEngine encountered a fatal error.", ex);
                ExecuteRollback();
                return -1;
            }
        }

        // ------------------------------------------------------------
        // PHASE EXECUTION
        // ------------------------------------------------------------

        private void ExecutePhase(string name, Action action)
        {
            _logger.Info($"--- BEGIN PHASE: {name} ---");

            try
            {
                action();
                _logger.Info($"--- END PHASE: {name} (SUCCESS) ---");
            }
            catch (Exception ex)
            {
                _logger.Error($"--- END PHASE: {name} (FAILED) ---", ex);
                throw;
            }
        }

        // ------------------------------------------------------------
        // ROLLBACK
        // ------------------------------------------------------------

        private void ExecuteRollback()
        {
            _logger.Warning("InstallerEngine initiating rollback.");
            try
            {
                _rollbackManager.Run();
                _logger.Warning("Rollback completed.");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Rollback encountered a fatal error.", ex);
            }
        }
    }
}
