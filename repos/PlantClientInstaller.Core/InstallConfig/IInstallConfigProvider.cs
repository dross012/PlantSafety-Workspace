namespace PlantClientInstaller.Core.InstallConfig;

public interface IInstallConfigProvider
{
    InstallConfigModel Load();
    bool TryLoad(out InstallConfigModel? config);
}
