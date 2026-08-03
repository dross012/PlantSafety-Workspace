using PlantClientInstaller.Core;
using PlantClientInstaller.Core.Bootstrapper;

namespace PlantClientInstaller.Bootstrapper.Bootstrapper
{
    public sealed class BootstrapperEngine(InstallerContext context, ILogger logger)
    {
        private readonly InstallerEngine _engine = new InstallerEngine(context, logger);
        private readonly InstallerContext _context = context;
        private readonly ILogger _logger = logger;

        public int Install() => _engine.Run();
        public int Uninstall() => new UninstallerEngine(_context, _logger).Run();
        public int Upgrade() => new UpgradeEngine(_context, _logger).Run();
        public InstallerContext Context => _context;
        public ILogger Logger => _logger;
    }
}
