using PlantClientInstaller.Core.Bootstrapper;

namespace PlantClientInstaller.Bootstrapper.Bootstrapper
{
    public sealed class BootstrapperContext
    {
        public static InstallerContext Create(string basePath)
        {
            return InstallerContext.CreateDefault(basePath);
        }
    }
}
