using PlantClientInstaller.Core.Bootstrapper;

namespace PlantClientInstaller.Bootstrapper.Bootstrapper
{
    public sealed class BootstrapperLogger(string logDir) : ILogger
    {
        private readonly Logger _inner = new(logDir);

        public void Info(string m) => _inner.Info(m);
        public void Warning(string m) => _inner.Warning(m);
        public void Error(string m, Exception? e = null) => _inner.Error(m, e);
        public void Fatal(string m, Exception? e = null) => _inner.Fatal(m, e);
    }
}
