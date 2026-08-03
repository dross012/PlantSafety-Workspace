namespace PlantClientInstaller.Core.InstallConfig;

public sealed class InstallConfigOptions
{
    public string ConfigDirectory { get; }
    public string ConfigFileName { get; }

    public InstallConfigOptions(string configDirectory, string configFileName = "installer.config.json")
    {
        ConfigDirectory = configDirectory;
        ConfigFileName = configFileName;
    }

    public string FullPath => Path.Combine(ConfigDirectory, ConfigFileName);
}
